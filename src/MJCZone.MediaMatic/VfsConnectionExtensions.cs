// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic;

/// <summary>
/// Extension methods for <see cref="IVfsConnection"/> providing media storage and processing operations.
/// </summary>
public static class VfsConnectionExtensions
{
    #region Upload operations

    /// <summary>
    /// Uploads a file to the virtual file system.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="stream">The file stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the uploaded file.</returns>
    public static Task<string> UploadFileAsync(
        this IVfsConnection connection,
        Stream stream,
        string path,
        bool overwrite = false,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().UploadFileAsync(connection, stream, path, overwrite, cancellationToken);
    }

    /// <summary>
    /// Uploads and processes an image.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="stream">The image stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="options">Image upload options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result including generated variants.</returns>
    public static Task<ImageUploadResult> UploadImageAsync(
        this IVfsConnection connection,
        Stream stream,
        string path,
        ImageUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().UploadImageAsync(connection, stream, path, options, cancellationToken);
    }

    /// <summary>
    /// Uploads and processes a video.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="stream">The video stream to upload.</param>
    /// <param name="path">The target path.</param>
    /// <param name="options">Video upload options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result including thumbnails and metadata.</returns>
    public static Task<VideoUploadResult> UploadVideoAsync(
        this IVfsConnection connection,
        Stream stream,
        string path,
        VideoUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().UploadVideoAsync(connection, stream, path, options, cancellationToken);
    }

    #endregion

    #region Download operations

    /// <summary>
    /// Downloads a file from the virtual file system.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The file path to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream containing the file data.</returns>
    public static Task<Stream> DownloadAsync(
        this IVfsConnection connection,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().DownloadAsync(connection, path, cancellationToken);
    }

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The file path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists.</returns>
    public static Task<bool> ExistsAsync(
        this IVfsConnection connection,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().ExistsAsync(connection, path, cancellationToken);
    }

    #endregion

    #region List operations

    /// <summary>
    /// Lists files in a directory.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of file paths.</returns>
    public static Task<IEnumerable<string>> ListFilesAsync(
        this IVfsConnection connection,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().ListFilesAsync(connection, path, cancellationToken);
    }

    /// <summary>
    /// Lists directories in a directory.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of directory paths.</returns>
    public static Task<IEnumerable<string>> ListFoldersAsync(
        this IVfsConnection connection,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().ListFoldersAsync(connection, path, cancellationToken);
    }

    #endregion

    #region Delete operations

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The file path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task DeleteAsync(
        this IVfsConnection connection,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().DeleteAsync(connection, path, cancellationToken);
    }

    /// <summary>
    /// Deletes a folder and all its contents.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The folder path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task DeleteFolderAsync(
        this IVfsConnection connection,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().DeleteFolderAsync(connection, path, cancellationToken);
    }

    #endregion

    #region Transform operations

    /// <summary>
    /// Processes an existing image.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="sourcePath">The source image path.</param>
    /// <param name="destinationPath">The destination path for processed image.</param>
    /// <param name="options">Processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processing result.</returns>
    public static Task<ImageProcessingResult> ProcessImageAsync(
        this IVfsConnection connection,
        string sourcePath,
        string destinationPath,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().ProcessImageAsync(connection, sourcePath, destinationPath, options, cancellationToken);
    }

    /// <summary>
    /// Generates thumbnails from a video.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="sourcePath">The source video path.</param>
    /// <param name="options">Thumbnail options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of generated thumbnails.</returns>
    public static Task<List<VideoThumbnail>> GenerateThumbnailsAsync(
        this IVfsConnection connection,
        string sourcePath,
        ThumbnailOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().GenerateThumbnailsAsync(connection, sourcePath, options, cancellationToken);
    }

    /// <summary>
    /// Transcodes a video to a different format.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="sourcePath">The source video path.</param>
    /// <param name="destinationPath">The destination path for transcoded video.</param>
    /// <param name="options">Transcode options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processing result.</returns>
    public static Task<VideoProcessingResult> TranscodeVideoAsync(
        this IVfsConnection connection,
        string sourcePath,
        string destinationPath,
        TranscodeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return connection
            .Vfs()
            .TranscodeVideoAsync(connection, sourcePath, destinationPath, options, cancellationToken);
    }

    #endregion

    #region Metadata operations

    /// <summary>
    /// Extracts metadata from a media file.
    /// </summary>
    /// <param name="connection">The VFS connection.</param>
    /// <param name="path">The file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted metadata.</returns>
    public static Task<MediaMetadata> GetMetadataAsync(
        this IVfsConnection connection,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return connection.Vfs().GetMetadataAsync(connection, path, cancellationToken);
    }

    #endregion

    #region Private helper methods

    private static IVfsMethods Vfs(this IVfsConnection connection)
    {
        return VfsMethodsProvider.GetMethods(connection);
    }

    #endregion
}
