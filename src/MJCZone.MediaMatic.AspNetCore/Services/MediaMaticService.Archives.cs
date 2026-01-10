// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Formats.Tar;
using System.IO.Compression;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.AspNetCore.Models;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for archive operations.
/// </summary>
public partial class MediaMaticService
{
    private const string ArchivesFolder = "__archives";

    private enum ArchiveFormat
    {
        Zip,
        Tar,
        TarGz,
    }

    #region Archive Methods

    /// <inheritdoc />
    public async Task<ArchiveResultDto> CreateFolderArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        ArchiveRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(folderPath, nameof(folderPath))
            .NotNull(request, nameof(request))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, folderPath);

        // Get the underlying blob storage for recursive listing
        if (connection is not VfsConnectionBase connectionBase)
        {
            throw new InvalidOperationException("Invalid VFS connection type");
        }

        var blobStorage = connectionBase.BlobStorage;

        // Get all files in the folder recursively
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = fullPath,
                    Recurse = true,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        var fileList = blobs.Where(b => !b.IsFolder).Select(b => b.FullPath).ToList();

        if (fileList.Count == 0)
        {
            throw new InvalidOperationException($"Folder '{folderPath}' is empty or does not exist.");
        }

        // Determine archive format and extension
        var (extension, format) = GetArchiveFormat(request.Compression);

        // Generate archive name
        var archiveName =
            request.Name
            ?? $"{folderPath.Replace("/", "_", StringComparison.Ordinal).TrimEnd('_')}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var archiveId = $"{archiveName}{extension}";
        var archivePath = CombineBucketAndPath(bucketName, $"{ArchivesFolder}/{archiveId}");

        // Create the archive
        using var archiveStream = new MemoryStream();
        await CreateArchiveAsync(archiveStream, format, fileList, fullPath, connection, cancellationToken)
            .ConfigureAwait(false);

        // Upload the archive
        archiveStream.Position = 0;
        await connection.UploadFileAsync(archiveStream, archivePath, true, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(
                context,
                true,
                $"Created archive '{archiveId}' from folder '{folderPath}' ({fileList.Count} files)"
            )
            .ConfigureAwait(false);

        return new ArchiveResultDto
        {
            ArchiveId = archiveId,
            ArchivePath = archivePath,
            FileCount = fileList.Count,
        };
    }

    /// <inheritdoc />
    public async Task<ArchiveResultDto> CreateFileListArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        ArchiveRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNull(request, nameof(request))
            .Assert();

        if (request.Paths == null || request.Paths.Count == 0)
        {
            throw new ArgumentException("Paths list is required and cannot be empty.", nameof(request));
        }

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);

        // Get the underlying blob storage for recursive listing
        if (connection is not VfsConnectionBase connectionBase)
        {
            throw new InvalidOperationException("Invalid VFS connection type");
        }

        var blobStorage = connectionBase.BlobStorage;

        // Resolve all file paths (expand folders recursively if needed)
        var filePaths = new List<string>();
        foreach (var path in request.Paths)
        {
            var fullPath = CombineBucketAndPath(bucketName, path);

            if (path.EndsWith('/'))
            {
                // It's a folder - get all files recursively
                var blobs = await blobStorage
                    .ListAsync(
                        new ListOptions
                        {
                            FolderPath = fullPath,
                            Recurse = true,
                            FilePrefix = null,
                        },
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                filePaths.AddRange(blobs.Where(b => !b.IsFolder).Select(b => b.FullPath));
            }
            else
            {
                // It's a file
                filePaths.Add(fullPath);
            }
        }

        if (filePaths.Count == 0)
        {
            throw new InvalidOperationException("No files found in the specified paths.");
        }

        // Determine archive format and extension
        var (extension, format) = GetArchiveFormat(request.Compression);

        // Generate archive name
        var archiveName = request.Name ?? $"archive_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var archiveId = $"{archiveName}{extension}";
        var archivePath = CombineBucketAndPath(bucketName, $"{ArchivesFolder}/{archiveId}");

        // Create the archive - use empty base path so filenames are used as entry names
        using var archiveStream = new MemoryStream();
        var filesAdded = await CreateArchiveAsync(
                archiveStream,
                format,
                filePaths,
                null, // No base path - use filenames only
                connection,
                cancellationToken
            )
            .ConfigureAwait(false);

        // Upload the archive
        archiveStream.Position = 0;
        await connection.UploadFileAsync(archiveStream, archivePath, true, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Created archive '{archiveId}' from file list ({filesAdded} files)")
            .ConfigureAwait(false);

        return new ArchiveResultDto
        {
            ArchiveId = archiveId,
            ArchivePath = archivePath,
            FileCount = filesAdded,
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArchiveFileDto>> ListArchivesAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var archiveFolderPath = CombineBucketAndPath(bucketName, ArchivesFolder);

        var archives = new List<ArchiveFileDto>();

        try
        {
            var files = await connection.ListFilesAsync(archiveFolderPath, cancellationToken).ConfigureAwait(false);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (
                    fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                    || fileName.EndsWith(".tar", StringComparison.OrdinalIgnoreCase)
                    || fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                )
                {
                    archives.Add(
                        new ArchiveFileDto
                        {
                            ArchiveId = fileName,
                            FileName = fileName,
                            Path = filePath,
                            Size = 0, // Would need metadata call to get size
                            CreatedAt = DateTime.UtcNow, // Would need metadata call to get actual date
                        }
                    );
                }
            }
        }
        catch (KeyNotFoundException)
        {
            // Archives folder doesn't exist yet, return empty list
        }

        await LogAuditEventAsync(context, true, $"Listed archives ({archives.Count} found)").ConfigureAwait(false);

        return archives;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(archiveId, nameof(archiveId))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var archivePath = CombineBucketAndPath(bucketName, $"{ArchivesFolder}/{archiveId}");

        var stream = await connection.DownloadAsync(archivePath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Downloaded archive '{archiveId}'").ConfigureAwait(false);

        return stream;
    }

    /// <inheritdoc />
    public async Task DeleteArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(archiveId, nameof(archiveId))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var archivePath = CombineBucketAndPath(bucketName, $"{ArchivesFolder}/{archiveId}");

        await connection.DeleteAsync(archivePath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Deleted archive '{archiveId}'").ConfigureAwait(false);
    }

    #endregion // Archive Methods

    #region Archive Helpers

    private static (string Extension, ArchiveFormat Format) GetArchiveFormat(string? compression)
    {
        return compression?.ToLowerInvariant() switch
        {
            "tar" => (".tar", ArchiveFormat.Tar),
            "tar.gz" or "tgz" => (".tar.gz", ArchiveFormat.TarGz),
            _ => (".zip", ArchiveFormat.Zip),
        };
    }

    private static async Task<int> CreateArchiveAsync(
        Stream outputStream,
        ArchiveFormat format,
        IReadOnlyList<string> filePaths,
        string? basePath,
        IVfsConnection connection,
        CancellationToken cancellationToken
    )
    {
        return format switch
        {
            ArchiveFormat.Zip => await CreateZipArchiveAsync(
                    outputStream,
                    filePaths,
                    basePath,
                    connection,
                    cancellationToken
                )
                .ConfigureAwait(false),
            ArchiveFormat.Tar => await CreateTarArchiveAsync(
                    outputStream,
                    filePaths,
                    basePath,
                    connection,
                    gzip: false,
                    cancellationToken
                )
                .ConfigureAwait(false),
            ArchiveFormat.TarGz => await CreateTarArchiveAsync(
                    outputStream,
                    filePaths,
                    basePath,
                    connection,
                    gzip: true,
                    cancellationToken
                )
                .ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
    }

    private static async Task<int> CreateZipArchiveAsync(
        Stream outputStream,
        IReadOnlyList<string> filePaths,
        string? basePath,
        IVfsConnection connection,
        CancellationToken cancellationToken
    )
    {
        var filesAdded = 0;

        using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    using var fileStream = await connection
                        .DownloadAsync(filePath, cancellationToken)
                        .ConfigureAwait(false);

                    var entryName = GetEntryName(filePath, basePath);
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    await fileStream.CopyToAsync(entryStream, cancellationToken).ConfigureAwait(false);
                    filesAdded++;
                }
                catch (KeyNotFoundException)
                {
                    // File not found, skip it
                }
            }
        }

        return filesAdded;
    }

    private static async Task<int> CreateTarArchiveAsync(
        Stream outputStream,
        IReadOnlyList<string> filePaths,
        string? basePath,
        IVfsConnection connection,
        bool gzip,
        CancellationToken cancellationToken
    )
    {
        var filesAdded = 0;

        // If gzip, wrap the output stream
        Stream tarStream = gzip
            ? new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true)
            : outputStream;

        try
        {
            var tarWriter = new TarWriter(tarStream, leaveOpen: true);
            await using (tarWriter.ConfigureAwait(false))
            {
                foreach (var filePath in filePaths)
                {
                    try
                    {
                        using var fileStream = await connection
                            .DownloadAsync(filePath, cancellationToken)
                            .ConfigureAwait(false);

                        // Copy to memory stream to get the length (TarEntry needs it)
                        using var memoryStream = new MemoryStream();
                        await fileStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                        memoryStream.Position = 0;

                        var entryName = GetEntryName(filePath, basePath);
                        var entry = new PaxTarEntry(TarEntryType.RegularFile, entryName) { DataStream = memoryStream };

                        await tarWriter.WriteEntryAsync(entry, cancellationToken).ConfigureAwait(false);
                        filesAdded++;
                    }
                    catch (KeyNotFoundException)
                    {
                        // File not found, skip it
                    }
                }
            }
        }
        finally
        {
            if (gzip && tarStream is GZipStream gzipStream)
            {
                await gzipStream.DisposeAsync().ConfigureAwait(false);
            }
        }

        return filesAdded;
    }

    private static string GetEntryName(string filePath, string? basePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            // No base path - use filename only
            return Path.GetFileName(filePath);
        }

        // Get relative path from base
        if (filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return filePath.Substring(basePath.Length).TrimStart('/');
        }

        return Path.GetFileName(filePath);
    }

    #endregion // Archive Helpers
}
