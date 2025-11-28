// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic.Interfaces;

/// <summary>
/// Defines methods for interacting with a virtual file system.
/// </summary>
public interface IVfsMethods
{
    /// <summary>
    /// Gets the type of the vfs provider.
    /// </summary>
    VfsProviderType ProviderType { get; }

    /// <summary>
    /// Gets a value indicating whether the vfs supports buckets.
    /// </summary>
    bool SupportsBuckets { get; }

    /// <summary>
    /// Gets a value indicating whether the vfs supports encrypted storage.
    /// </summary>
    bool SupportsNativeEncryptedStorage { get; }

    /// <summary>
    /// Gets a value indicating whether the vfs supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }

    // Upload operations

    /// <summary>
    /// Uploads a file to the virtual file system.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="stream">The file stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the uploaded file.</returns>
    Task<string> UploadFileAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        bool overwrite = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads and processes an image.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="stream">The image stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="options">Image upload options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result including generated variants.</returns>
    Task<ImageUploadResult> UploadImageAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        ImageUploadOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads and processes a video.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="stream">The video stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="options">Video upload options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result including thumbnails and metadata.</returns>
    Task<VideoUploadResult> UploadVideoAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        VideoUploadOptions? options = null,
        CancellationToken cancellationToken = default
    );

    // Download operations

    /// <summary>
    /// Downloads a file from the virtual file system.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The file path to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream containing the file data.</returns>
    Task<Stream> DownloadAsync(IVfsConnection vfs, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The file path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists.</returns>
    Task<bool> ExistsAsync(IVfsConnection vfs, string path, CancellationToken cancellationToken = default);

    // List operations

    /// <summary>
    /// Lists files in a directory.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of file paths.</returns>
    Task<IEnumerable<string>> ListFilesAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lists directories in a directory.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of directory paths.</returns>
    Task<IEnumerable<string>> ListFoldersAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    );

    // Delete operations

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The file path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(IVfsConnection vfs, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a folder and all its contents.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The folder path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFolderAsync(IVfsConnection vfs, string path, CancellationToken cancellationToken = default);

    // Folder operations

    /// <summary>
    /// Creates a folder.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The folder path to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateFolderAsync(IVfsConnection vfs, string path, CancellationToken cancellationToken = default);

    // Transform operations

    /// <summary>
    /// Processes an existing image.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="sourcePath">The source image path.</param>
    /// <param name="destinationPath">The destination path for processed image.</param>
    /// <param name="options">Processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processing result.</returns>
    Task<ImageProcessingResult> ProcessImageAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates thumbnails from a video.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="sourcePath">The source video path.</param>
    /// <param name="options">Thumbnail options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of generated thumbnails.</returns>
    Task<List<VideoThumbnail>> GenerateThumbnailsAsync(
        IVfsConnection vfs,
        string sourcePath,
        ThumbnailOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Transcodes a video to a different format.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="sourcePath">The source video path.</param>
    /// <param name="destinationPath">The destination path for transcoded video.</param>
    /// <param name="options">Transcode options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processing result.</returns>
    Task<VideoProcessingResult> TranscodeVideoAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        TranscodeOptions? options = null,
        CancellationToken cancellationToken = default
    );

    // Metadata operations

    /// <summary>
    /// Extracts metadata from a media file.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <param name="path">The file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted metadata.</returns>
    Task<MediaMetadata> GetMetadataAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    );
}
