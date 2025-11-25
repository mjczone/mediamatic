// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for statistics operations.
/// </summary>
public partial class MediaMaticService
{
    #region Statistics Methods

    /// <inheritdoc />
    public async Task<FolderStatsResponse> GetFolderStatsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string folderPath,
        bool recursive,
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

        // TODO: Implement folder stats calculation in IVfsMethods and add extension method
        // This would involve listing files, getting their sizes and types, and aggregating
        var stats = new FolderStatsResponse
        {
            Path = folderPath,
            TotalSize = 0,
            FileCount = 0,
            FolderCount = 0,
            ByType = new Dictionary<string, TypeStats>(),
        };

        await LogAuditEventAsync(context, true, $"Retrieved stats for folder '{folderPath}'").ConfigureAwait(false);

        return stats;
    }

    /// <inheritdoc />
    public async Task<FilesourceStatsResponse> GetFilesourceStatsAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
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

        // TODO: Implement filesource stats calculation in IVfsMethods and add extension method
        // This would involve recursively analyzing the entire filesource or bucket
        var stats = new FilesourceStatsResponse
        {
            FilesourceId = filesourceId,
            BucketName = bucketName,
            TotalSize = 0,
            FileCount = 0,
            FolderCount = 0,
            ByType = new Dictionary<string, TypeStats>(),
            TopFolders = new List<FolderSizeInfo>(),
        };

        var bucketDisplay =
            bucketName != null ? $" (bucket '{bucketName}')" : string.Empty;
        await LogAuditEventAsync(context, true, $"Retrieved stats for filesource '{filesourceId}'{bucketDisplay}")
            .ConfigureAwait(false);

        return stats;
    }

    #endregion // Statistics Methods
}
