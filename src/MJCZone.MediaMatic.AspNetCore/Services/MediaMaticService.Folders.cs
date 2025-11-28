// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for folder operations.
/// </summary>
public partial class MediaMaticService
{
    #region Folder Methods

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListFoldersAsync(
        IOperationContext context,
        string filesourceId,
        string? bucketName,
        string? path,
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

        var folders = await connection.ListFoldersAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Listed folders in path '{path ?? "root"}'").ConfigureAwait(false);

        return folders;
    }

    /// <inheritdoc />
    public async Task<bool> CreateFolderAsync(
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

        // Check if folder already exists by checking for any contents
        var existingFolders = await connection.ListFoldersAsync(fullPath, cancellationToken).ConfigureAwait(false);
        var existingFiles = await connection.ListFilesAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (existingFolders.Any() || existingFiles.Any())
        {
            // Folder already exists (has contents)
            return false;
        }

        // Create the folder by creating a placeholder file and then deleting it
        // This is a common pattern for blob storage that doesn't have true folders
        await connection.CreateFolderAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Created folder '{folderPath}'").ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task DeleteFolderAsync(
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

        await connection.DeleteFolderAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Deleted folder '{folderPath}'").ConfigureAwait(false);
    }

    #endregion // Folder Methods
}
