// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for archive operations.
/// </summary>
public partial class MediaMaticService
{
    #region Archive Methods

    /// <inheritdoc />
    public async Task<ArchiveResponse> CreateFolderArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        ArchiveRequest request,
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

        // TODO: Implement archive creation in IVfsMethods and add extension method
        // For now, return a stub response
        var response = new ArchiveResponse
        {
            JobId = Guid.NewGuid().ToString(),
            ArchiveId =
                $"{folderPath.Replace("/", "_", StringComparison.Ordinal)}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip",
        };

        await LogAuditEventAsync(context, true, $"Created archive for folder '{folderPath}'").ConfigureAwait(false);

        return response;
    }

    /// <inheritdoc />
    public async Task<ArchiveResponse> CreateFileListArchiveAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        ArchiveRequest request,
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

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);

        // TODO: Implement file list archive creation in IVfsMethods and add extension method
        var response = new ArchiveResponse
        {
            JobId = Guid.NewGuid().ToString(),
            ArchiveId = $"archive_{DateTime.UtcNow:yyyyMMddHHmmss}.zip",
        };

        await LogAuditEventAsync(context, true, $"Created archive from file list ({request.Paths?.Count ?? 0} items)")
            .ConfigureAwait(false);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArchiveInfo>> ListArchivesAsync(
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
            .NotNullOrWhiteSpace(folderPath, nameof(folderPath))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);
        var fullPath = CombineBucketAndPath(bucketName, folderPath);

        // TODO: Implement archive listing in IVfsMethods and add extension method
        var archives = new List<ArchiveInfo>();

        await LogAuditEventAsync(context, true, $"Listed archives in folder '{folderPath}'").ConfigureAwait(false);

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

        // TODO: Archives are stored in a special __archives folder, need to implement proper path resolution
        var archivePath = CombineBucketAndPath(bucketName, $"__archives/{archiveId}");
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

        // TODO: Archives are stored in a special __archives folder
        var archivePath = CombineBucketAndPath(bucketName, $"__archives/{archiveId}");
        await connection.DeleteAsync(archivePath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Deleted archive '{archiveId}'").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ArchiveJobStatus> GetArchiveJobStatusAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string jobId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(filesourceId, nameof(filesourceId))
            .NotNullOrWhiteSpace(jobId, nameof(jobId))
            .Assert();

        var connection = await GetVfsConnectionAsync(filesourceId).ConfigureAwait(false);

        // TODO: Implement job status tracking (probably needs a separate job tracking system)
        var status = new ArchiveJobStatus
        {
            JobId = jobId,
            Status = "completed",
            Progress = 100,
        };

        await LogAuditEventAsync(context, true, $"Retrieved status for archive job '{jobId}'").ConfigureAwait(false);

        return status;
    }

    #endregion // Archive Methods
}
