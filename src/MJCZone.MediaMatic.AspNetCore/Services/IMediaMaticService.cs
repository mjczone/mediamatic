// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// Service interface for MediaMatic operations in ASP.NET Core applications.
/// </summary>
public interface IMediaMaticService
{
    #region Filesource Methods

    /// <summary>
    /// Gets all registered datasources.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of datasource information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<IEnumerable<FilesourceDto>> GetFilesourcesAsync(
        IOperationContext context,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets datasource information by name.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasourceId">The id of the datasource.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The datasource information.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the datasource is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FilesourceDto> GetFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new datasource.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasource">The datasource to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added datasource if successful.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FilesourceDto> AddFilesourceAsync(
        IOperationContext context,
        FilesourceDto datasource,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates an existing datasource.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasource">The updated datasource information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated datasource.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the datasource is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FilesourceDto> UpdateFilesourceAsync(
        IOperationContext context,
        FilesourceDto datasource,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes a datasource.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasourceId">The id of the datasource to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the datasource is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task RemoveFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if a datasource exists.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasourceId">The id of the datasource to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the datasource exists, false otherwise.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<bool> FilesourceExistsAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Tests the connection to a datasource.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="datasourceId">The id of the datasource to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test result containing connection status and details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the datasource is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FilesourceConnectivityTestDto> TestFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    );

    #endregion // Filesource Methods

    #region File Methods

    /// <summary>
    /// Gets metadata for a file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File metadata.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<MJCZone.MediaMatic.Models.MediaMetadata> GetFileMetadataAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Transforms an image with specified processing options.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The source file path.</param>
    /// <param name="options">Image processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream containing the transformed image.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<Stream> TransformImageAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        MJCZone.MediaMatic.Models.ImageProcessingOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lists files in a directory.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="recursive">Whether to list files recursively.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of file paths.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<IEnumerable<string>> ListFilesAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string? path,
        bool recursive,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The file path to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream containing the file data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<Stream> DownloadFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The target file path.</param>
    /// <param name="stream">The file stream to upload.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the uploaded file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file exists and overwrite is false.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<string> UploadFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        Stream stream,
        bool overwrite,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The file path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task DeleteFileAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<bool> FileExistsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken = default
    );

    #endregion // File Methods

    #region Folder Methods

    /// <summary>
    /// Lists folders in a directory.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="path">The directory path (null for root).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of folder paths.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<IEnumerable<string>> ListFoldersAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string? path,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a folder and all its contents.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="folderPath">The folder path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the folder is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task DeleteFolderAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken = default
    );

    #endregion // Folder Methods

    #region Archive Methods

    /// <summary>
    /// Creates an archive of a folder.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="folderPath">The folder path to archive.</param>
    /// <param name="request">Archive creation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Archive response containing job ID and archive information.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the folder is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<ArchiveResponse> CreateFolderArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        ArchiveRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates an archive from a list of files and folders.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="request">Archive request containing the list of paths.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Archive response containing job ID and archive information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<ArchiveResponse> CreateFileListArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        ArchiveRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lists all archives in a folder.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="folderPath">The folder path containing archives.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of archive information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<IEnumerable<ArchiveInfo>> ListArchivesAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Downloads an archive file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="archiveId">The archive identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream containing the archive data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the archive is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<Stream> DownloadArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes an archive file.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="archiveId">The archive identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the archive is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task DeleteArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the status of an archive creation job.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Archive job status.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the job is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<ArchiveJobStatus> GetArchiveJobStatusAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string jobId,
        CancellationToken cancellationToken = default
    );

    #endregion // Archive Methods

    #region Statistics Methods

    /// <summary>
    /// Gets statistics for a folder.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="folderPath">The folder path.</param>
    /// <param name="recursive">Whether to include subdirectories recursively.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Folder statistics.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the folder is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FolderStatsResponse> GetFolderStatsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        bool recursive,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets statistics for an entire filesource or bucket.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="filesourceId">The filesource identifier.</param>
    /// <param name="bucketName">Optional bucket name (for S3/Azure).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filesource/bucket statistics.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the filesource is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    Task<FilesourceStatsResponse> GetFilesourceStatsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        CancellationToken cancellationToken = default
    );

    #endregion // Statistics Methods
}
