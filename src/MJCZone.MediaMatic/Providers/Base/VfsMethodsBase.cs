// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage.Blobs;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Represents the base class for VFS methods.
/// </summary>
public abstract partial class VfsMethodsBase : IVfsMethods
{
    #region Media processor fields (used by VfsMethodsBase.Media.cs)

    private IMimeTypeDetector? _mimeDetector;
    private IMetadataReader? _metadataReader;
    private IImageProcessor? _imageProcessor;
    private IVideoProcessor? _videoProcessor;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VfsMethodsBase"/> class.
    /// </summary>
    /// <param name="providerType">The VFS provider type.</param>
    internal VfsMethodsBase(VfsProviderType providerType)
    {
        ProviderType = providerType;
        InitializeMediaProcessors();
    }

    /// <summary>
    /// Gets the type of the VFS provider.
    /// </summary>
    public VfsProviderType ProviderType { get; }

    /// <summary>
    /// Gets a value indicating whether the provider supports buckets.
    /// </summary>
    public virtual bool SupportsBuckets => false;

    /// <summary>
    /// Gets a value indicating whether the provider supports encrypted storage.
    /// </summary>
    public virtual bool SupportsNativeEncryptedStorage => false;

    /// <summary>
    /// Gets a value indicating whether the provider supports streaming.
    /// </summary>
    public virtual bool SupportsStreaming => false;

    #region Media processor properties (used by VfsMethodsBase.Media.cs)

    private IMimeTypeDetector MimeDetector =>
        _mimeDetector ?? throw new InvalidOperationException("Media processors not initialized");

    private IMetadataReader MetadataReader =>
        _metadataReader ?? throw new InvalidOperationException("Media processors not initialized");

    private IImageProcessor ImageProcessor =>
        _imageProcessor ?? throw new InvalidOperationException("Media processors not initialized");

    private IVideoProcessor VideoProcessor =>
        _videoProcessor ?? throw new InvalidOperationException("Media processors not initialized");

    #endregion

    #region Basic file operations - Generic implementations using IBlobStorage

    /// <inheritdoc/>
    public virtual async Task<string> UploadFileAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        bool overwrite = false,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);

        if (!overwrite && await blobStorage.ExistsAsync(path, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"File already exists at path: {path}");
        }

        await blobStorage.WriteAsync(path, stream, false, cancellationToken).ConfigureAwait(false);
        return path;
    }

    /// <inheritdoc/>
    public virtual async Task<VideoUploadResult> UploadVideoAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        VideoUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        options ??= new VideoUploadOptions();

        var tempVideoFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(path)}");

        try
        {
            // Save stream to temp file (needed for FFMpegCore operations)
            using (var fileStream = File.Create(tempVideoFile))
            {
                await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            // Upload video to VFS
            using (var uploadStream = File.OpenRead(tempVideoFile))
            {
                await UploadFileAsync(vfs, uploadStream, path, options.Overwrite, cancellationToken)
                    .ConfigureAwait(false);
            }

            // Get file size
            var fileInfo = new FileInfo(tempVideoFile);

            // Extract metadata if requested
            MediaMetadata? metadata = null;
            if (options.ExtractMetadata)
            {
                metadata = await MetadataReader
                    .ExtractVideoMetadataAsync(tempVideoFile, cancellationToken)
                    .ConfigureAwait(false);
            }

            var result = new VideoUploadResult
            {
                Path = path,
                Width = metadata?.Width ?? 0,
                Height = metadata?.Height ?? 0,
                Duration = metadata?.Duration?.TotalSeconds ?? 0,
                FileSize = fileInfo.Length,
                MimeType = "video/" + Path.GetExtension(path).TrimStart('.'),
                VideoCodec = metadata?.VideoCodec,
                AudioCodec = metadata?.AudioCodec,
                Bitrate = metadata?.TotalBitrate ?? 0,
                FrameRate = metadata?.FrameRate ?? 0,
                Metadata = metadata,
                Success = true,
            };

            // Generate thumbnails if requested
            if (options.GenerateThumbnails)
            {
                var thumbnailOptions = new ThumbnailOptions
                {
                    Count = options.ThumbnailCount,
                    Timestamps = options.ThumbnailTimestamps,
                    Width = options.ThumbnailWidth,
                };

                var thumbnails = await GenerateThumbnailsAsync(vfs, path, thumbnailOptions, cancellationToken)
                    .ConfigureAwait(false);
                result.Thumbnails = thumbnails;
            }

            return result;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            return new VideoUploadResult
            {
                Path = path,
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
#pragma warning restore CA1031 // Do not catch general exception types
        finally
        {
            // Clean up temp file
            if (File.Exists(tempVideoFile))
            {
                File.Delete(tempVideoFile);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Stream> DownloadAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        return await blobStorage.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        return await blobStorage.ExistsAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<string>> ListFilesAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = false,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return blobs.Where(b => !b.IsFolder).Select(b => b.FullPath);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<string>> ListFoldersAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = false,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return blobs.Where(b => b.IsFolder).Select(b => b.FullPath);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        await blobStorage.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteFolderAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);

        // List all files in the folder recursively
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = true,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // Delete all files
        var filePaths = blobs.Where(b => !b.IsFolder).Select(b => b.FullPath).ToList();
        if (filePaths.Count != 0)
        {
            await blobStorage.DeleteAsync(filePaths, cancellationToken).ConfigureAwait(false);
        }

        // Delete all subfolders (deepest first to avoid issues)
        // Some providers (SFTP) may auto-delete empty parent folders when children are removed,
        // so we catch and ignore "not found" errors during folder deletion.
        var folderPaths = blobs.Where(b => b.IsFolder).Select(b => b.FullPath).OrderByDescending(p => p.Length).ToList();
        foreach (var folder in folderPaths)
        {
            try
            {
                await blobStorage.DeleteAsync(folder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.Message.Contains("No such file", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                // Folder was already deleted (auto-removed when empty), ignore
            }
        }

        // Delete the folder itself (with and without trailing slash)
        var folderPath = path.TrimEnd('/');
        var pathsToDelete = new List<string> { folderPath, folderPath + "/", folderPath + "/.folder" };

        foreach (var pathToDelete in pathsToDelete)
        {
            try
            {
                await blobStorage.DeleteAsync(pathToDelete, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.Message.Contains("No such file", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                // Already deleted, ignore
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task CreateFolderAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        // Most blob storage systems don't have true folders - they use path prefixes.
        // Creating a folder typically means creating a placeholder file or marker.
        // FluentStorage doesn't have a direct CreateFolder method, so we create
        // an empty marker file to ensure the "folder" exists.
        var blobStorage = GetBlobStorage(vfs);

        // Ensure path ends with / to indicate it's a folder
        var folderPath = path.EndsWith('/') ? path : path + "/";
        var markerPath = folderPath + ".folder";

        // Write an empty marker file
        await blobStorage.WriteAsync(markerPath, new MemoryStream(), false, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Media processing operations - Stub implementations (override in provider classes)

    /// <inheritdoc/>
    public virtual async Task<List<VideoThumbnail>> GenerateThumbnailsAsync(
        IVfsConnection vfs,
        string sourcePath,
        ThumbnailOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        options ??= new ThumbnailOptions();

        // Download video to temp file (FFMpegCore requires file path)
        var tempVideoFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}");
        var tempThumbnailDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempThumbnailDir);

            // Download video to temp file
            var videoStream = await DownloadAsync(vfs, sourcePath, cancellationToken).ConfigureAwait(false);
            try
            {
                using var fileStream = File.Create(tempVideoFile);
                await videoStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await videoStream.DisposeAsync().ConfigureAwait(false);
            }

            // Generate thumbnails to temp directory
            var tempOptions = new ThumbnailOptions
            {
                OutputPath = tempThumbnailDir,
                Count = options.Count,
                Timestamps = options.Timestamps,
                Width = options.Width,
                Height = options.Height,
                FilePattern = options.FilePattern,
            };

            var thumbnailPaths = await VideoProcessor
                .GenerateThumbnailsAsync(tempVideoFile, tempOptions, cancellationToken)
                .ConfigureAwait(false);

            // Upload thumbnails to VFS and build result list
            var thumbnails = new List<VideoThumbnail>();
            var baseDir = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            var baseFileName = Path.GetFileNameWithoutExtension(sourcePath);

            for (int i = 0; i < thumbnailPaths.Count; i++)
            {
                var tempThumbnailPath = thumbnailPaths[i];
                var thumbnailFileName = Path.GetFileName(tempThumbnailPath);
                var vfsThumbnailPath = Path.Combine(baseDir, $"{baseFileName}_{thumbnailFileName}");

                // Upload thumbnail to VFS
                using var thumbnailStream = File.OpenRead(tempThumbnailPath);
                await UploadFileAsync(vfs, thumbnailStream, vfsThumbnailPath, true, cancellationToken)
                    .ConfigureAwait(false);

                // Get file info
                var fileInfo = new FileInfo(tempThumbnailPath);
                var timestamp = options.Timestamps?[i] ?? tempOptions.Timestamps?[i] ?? 0;

                // Get thumbnail dimensions using SkiaSharp
                using var skStream = File.OpenRead(tempThumbnailPath);
                using var skBitmap = SkiaSharp.SKBitmap.Decode(skStream);

                thumbnails.Add(
                    new VideoThumbnail
                    {
                        Path = vfsThumbnailPath,
                        Timestamp = timestamp,
                        Width = skBitmap?.Width ?? options.Width,
                        Height = skBitmap?.Height ?? options.Height ?? -1,
                        FileSize = fileInfo.Length,
                        Format = ImageFormat.Jpeg, // FFMpeg generates JPEG by default
                    }
                );
            }

            return thumbnails;
        }
        finally
        {
            // Clean up temp files
            if (File.Exists(tempVideoFile))
            {
                File.Delete(tempVideoFile);
            }

            if (Directory.Exists(tempThumbnailDir))
            {
                Directory.Delete(tempThumbnailDir, true);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task<VideoProcessingResult> TranscodeVideoAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        TranscodeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        options ??= new TranscodeOptions();

        var startTime = DateTime.UtcNow;

        // Temp files for video processing
        var tempSourceFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}");
        var tempDestFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(destinationPath)}");

        try
        {
            // Download source video to temp file
            var sourceStream = await DownloadAsync(vfs, sourcePath, cancellationToken).ConfigureAwait(false);
            try
            {
                using var fileStream = File.Create(tempSourceFile);
                await sourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await sourceStream.DisposeAsync().ConfigureAwait(false);
            }

            // Transcode video
            await VideoProcessor
                .TranscodeAsync(tempSourceFile, tempDestFile, options, cancellationToken)
                .ConfigureAwait(false);

            // Upload transcoded video to VFS
            using (var transcodedStream = File.OpenRead(tempDestFile))
            {
                await UploadFileAsync(vfs, transcodedStream, destinationPath, true, cancellationToken)
                    .ConfigureAwait(false);
            }

            // Get result metadata
            var fileInfo = new FileInfo(tempDestFile);
            var metadata = await MetadataReader
                .ExtractVideoMetadataAsync(tempDestFile, cancellationToken)
                .ConfigureAwait(false);
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new VideoProcessingResult
            {
                Path = destinationPath,
                Width = metadata.Width ?? 0,
                Height = metadata.Height ?? 0,
                Duration = metadata.Duration?.TotalSeconds ?? 0,
                FileSize = fileInfo.Length,
                Format = options.Format,
                Bitrate = metadata.TotalBitrate ?? 0,
                Success = true,
                ProcessingTimeMs = (long)processingTime,
            };
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            return new VideoProcessingResult
            {
                Path = destinationPath,
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
#pragma warning restore CA1031 // Do not catch general exception types
        finally
        {
            // Clean up temp files
            if (File.Exists(tempSourceFile))
            {
                File.Delete(tempSourceFile);
            }

            if (File.Exists(tempDestFile))
            {
                File.Delete(tempDestFile);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task<MediaMetadata> GetMetadataAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Download file to check if it's image or video
        var stream = await DownloadAsync(vfs, path, cancellationToken).ConfigureAwait(false);

        try
        {
            // Detect MIME type to determine if it's image or video
            var mimeType = await MimeDetector.DetectMimeTypeAsync(stream, cancellationToken).ConfigureAwait(false);

            if (mimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Reset stream position for metadata extraction
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                // Extract image metadata from stream
                var metadata = await MetadataReader
                    .ExtractImageMetadataAsync(stream, cancellationToken)
                    .ConfigureAwait(false);
                metadata.MimeType = mimeType;
                metadata.FileName = Path.GetFileName(path);
                return metadata;
            }
            else if (mimeType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Video metadata extraction requires a file path, so download to temp file
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(path)}");

                try
                {
                    // Download to temp file
                    using (var fileStream = File.Create(tempFile))
                    {
                        await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                    }

                    // Extract video metadata
                    var videoMetadata = await MetadataReader
                        .ExtractVideoMetadataAsync(tempFile, cancellationToken)
                        .ConfigureAwait(false);
                    videoMetadata.MimeType = mimeType;
                    return videoMetadata;
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            else
            {
                // For non-media files, return basic metadata with just MIME type
                // Try to get file size from stream if possible
                long size = 0;
                if (stream.CanSeek)
                {
                    size = stream.Length;
                }

                return new MediaMetadata
                {
                    MimeType = mimeType ?? "application/octet-stream",
                    FileName = Path.GetFileName(path),
                    Size = size,
                };
            }
        }
        finally
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Gets the blob storage from the VFS connection.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <returns>The blob storage instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection type is invalid.</exception>
    protected static IBlobStorage GetBlobStorage(IVfsConnection vfs)
    {
        if (vfs is not VfsConnectionBase connectionBase)
        {
            throw new InvalidOperationException("Invalid VFS connection type");
        }

        return connectionBase.BlobStorage;
    }

    /// <summary>
    /// Initializes media processor instances (implemented in VfsMethodsBase.Media.cs).
    /// </summary>
    partial void InitializeMediaProcessors();

    #endregion
}
