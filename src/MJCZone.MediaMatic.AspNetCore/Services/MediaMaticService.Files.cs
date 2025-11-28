// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for file operations.
/// </summary>
public partial class MediaMaticService
{
    #region File Methods

    /// <inheritdoc />
    public async Task<MediaMetadata> GetFileMetadataAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"File not found: {filePath}");
        }

        var metadata = await connection.GetMetadataAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Retrieved metadata for file '{filePath}'").ConfigureAwait(false);

        return metadata;
    }

    /// <inheritdoc />
    public async Task<Stream> TransformImageAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .NotNull(options, nameof(options))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"File not found: {filePath}");
        }

        // Download the file
        var sourceStream = await connection.DownloadAsync(fullPath, cancellationToken).ConfigureAwait(false);

        // Process the image with options
        var hasResize = options.Width.HasValue || options.Height.HasValue;
        var hasFormatConversion = options.Format.HasValue;

        Stream resultStream = sourceStream;

        if (hasResize)
        {
            var processedImage = await _imageProcessor
                .ResizeAsync(sourceStream, options.Width, options.Height, options, cancellationToken)
                .ConfigureAwait(false);
            resultStream = processedImage.stream;
        }
        else if (hasFormatConversion)
        {
            var processedImage = await _imageProcessor
                .ConvertFormatAsync(sourceStream, options.Format!.Value, options.Quality, cancellationToken)
                .ConfigureAwait(false);
            resultStream = processedImage.stream;
        }

        await LogAuditEventAsync(context, true, $"Transformed image '{filePath}'").ConfigureAwait(false);

        return resultStream;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListFilesAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string? path,
        bool recursive,
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
        var fullPath = CombineBucketAndPath(bucketName, path);

        // Note: recursive parameter is not yet used - VfsMethodsBase.ListFilesAsync doesn't support it.
        // For recursive browsing, use the ListAsync method instead (BrowseEndpoints use this).
        _ = recursive; // Suppress unused parameter warning for now
        var files = await connection.ListFilesAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Listed files in path '{path ?? "root"}'").ConfigureAwait(false);

        return files;
    }

    /// <inheritdoc />
    public async Task<BrowseResponseDto> ListAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string? path,
        BrowseOptions options,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNull(options, nameof(options))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, path);

        // Get the underlying blob storage to access rich metadata
        if (connection is not VfsConnectionBase connectionBase)
        {
            throw new InvalidOperationException("Invalid VFS connection type");
        }

        var blobStorage = connectionBase.BlobStorage;

        // List blobs with FluentStorage
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = fullPath,
                    Recurse = options.Recursive,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // Apply wildcard filter if specified
        if (!string.IsNullOrEmpty(options.Filter))
        {
            var filterRegex = WildcardToRegex(options.Filter);
            blobs = blobs.Where(b => filterRegex.IsMatch(Path.GetFileName(b.FullPath))).ToList();
        }

        // Build response based on requested type
        var folders = new List<FolderInfoDto>();
        var files = new List<FileInfoDto>();

        foreach (var blob in blobs)
        {
            if (blob.IsFolder)
            {
                if (options.Type == BrowseType.All || options.Type == BrowseType.Folders)
                {
                    folders.Add(
                        new FolderInfoDto { Path = blob.FullPath, Name = Path.GetFileName(blob.FullPath.TrimEnd('/')) }
                    );
                }
            }
            else
            {
                if (options.Type == BrowseType.All || options.Type == BrowseType.Files)
                {
                    var extension = Path.GetExtension(blob.FullPath);
                    files.Add(
                        new FileInfoDto
                        {
                            Path = blob.FullPath,
                            Name = Path.GetFileName(blob.FullPath),
                            Size = options.IncludeField("size") ? blob.Size : null,
                            LastModified = options.IncludeField("lastModified") ? blob.LastModificationTime : null,
                            Extension = options.IncludeField("extension") ? extension : null,
                            Category = options.IncludeField("category")
                                ? FileCategoryMapper.GetCategory(extension)
                                : null,
                        }
                    );
                }
            }
        }

        await LogAuditEventAsync(context, true, $"Browsed path '{path ?? "root"}'").ConfigureAwait(false);

        return new BrowseResponseDto(folders, files);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"File not found: {filePath}");
        }

        var stream = await connection.DownloadAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Downloaded file '{filePath}'").ConfigureAwait(false);

        return stream;
    }

    /// <inheritdoc />
    public async Task<string> UploadFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        Stream stream,
        bool overwrite,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .NotNull(stream, nameof(stream))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        if (!overwrite)
        {
            var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                throw new InvalidOperationException($"File already exists: {filePath}");
            }
        }

        await connection.UploadFileAsync(stream, fullPath, overwrite, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Uploaded file '{filePath}'").ConfigureAwait(false);

        return filePath;
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"File not found: {filePath}");
        }

        await connection.DeleteAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Deleted file '{filePath}'").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(filePath, nameof(filePath))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, filePath);

        var exists = await connection.ExistsAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Checked existence of file '{filePath}'").ConfigureAwait(false);

        return exists;
    }

    #endregion // File Methods

    #region File Methods - Private Helpers

    /// <summary>
    /// Converts a wildcard pattern to a regex.
    /// </summary>
    private static Regex WildcardToRegex(string pattern)
    {
        // Escape regex special characters, then convert wildcards
        var escaped = Regex.Escape(pattern);
        var regexPattern = escaped
            .Replace("\\*", ".*", StringComparison.Ordinal)
            .Replace("\\?", ".", StringComparison.Ordinal);
        return new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    #endregion // File Methods - Private Helpers
}
