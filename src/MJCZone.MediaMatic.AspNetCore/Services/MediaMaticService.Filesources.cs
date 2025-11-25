// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// MediaMaticService implementation for filesource management operations.
/// </summary>
public partial class MediaMaticService
{
    #region Filesource Methods

    /// <inheritdoc />
    public async Task<IEnumerable<FilesourceDto>> GetFilesourcesAsync(
        IOperationContext context,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory.Arguments().NotNull(context, nameof(context)).Assert();

        var result = await _filesourceRepository.GetFilesourcesAsync(null).ConfigureAwait(false);
        await LogAuditEventAsync(context, true, "Retrieved all filesources").ConfigureAwait(false);
        return result ?? [];
    }

    /// <inheritdoc />
    public async Task<FilesourceDto> GetFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(datasourceId, nameof(datasourceId))
            .Assert();

        var filesource =
            await _filesourceRepository.GetFilesourceAsync(datasourceId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Filesource '{datasourceId}' not found");

        await LogAuditEventAsync(context, true, $"Retrieved filesource '{datasourceId}'").ConfigureAwait(false);
        return filesource;
    }

    /// <inheritdoc />
    public async Task<FilesourceDto> AddFilesourceAsync(
        IOperationContext context,
        FilesourceDto datasource,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNull(datasource, nameof(datasource))
            .Assert();

        if (
            !string.IsNullOrWhiteSpace(datasource.Id)
            && await _filesourceRepository.FilesourceExistsAsync(datasource.Id!).ConfigureAwait(false)
        )
        {
            throw new DuplicateKeyException($"Filesource '{datasource.Id}' already exists");
        }

        var created = await _filesourceRepository.AddFilesourceAsync(datasource).ConfigureAwait(false);

        if (!created)
        {
            throw new InvalidOperationException("Failed to create filesource");
        }

        var newFilesource =
            await _filesourceRepository.GetFilesourceAsync(datasource.Id!).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Failed to retrieve newly created filesource");

        await LogAuditEventAsync(context, true, $"Filesource '{newFilesource.Id}' created").ConfigureAwait(false);
        return newFilesource;
    }

    /// <inheritdoc />
    public async Task<FilesourceDto> UpdateFilesourceAsync(
        IOperationContext context,
        FilesourceDto datasource,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNull(datasource, nameof(datasource))
            .Object(
                datasource,
                nameof(datasource),
                builder => builder.NotNullOrWhiteSpace(d => d.Id, nameof(datasource.Id))
            )
            .Assert();

        var existing =
            await _filesourceRepository.GetFilesourceAsync(datasource.Id!).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Filesource '{datasource.Id}' not found");

        var updated = await _filesourceRepository.UpdateFilesourceAsync(datasource!).ConfigureAwait(false);

        if (!updated)
        {
            throw new InvalidOperationException("Failed to update filesource");
        }

        existing =
            await _filesourceRepository.GetFilesourceAsync(datasource.Id!).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Filesource '{datasource.Id}' not found after update");

        await LogAuditEventAsync(context, true, $"Updated filesource '{datasource.Id}'").ConfigureAwait(false);
        return existing;
    }

    /// <inheritdoc />
    public async Task RemoveFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(datasourceId, nameof(datasourceId))
            .Assert();

        var existing =
            await _filesourceRepository.GetFilesourceAsync(datasourceId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Filesource '{datasourceId}' not found");

        var deleted = await _filesourceRepository.RemoveFilesourceAsync(datasourceId).ConfigureAwait(false);

        if (!deleted)
        {
            throw new InvalidOperationException("Failed to delete filesource");
        }

        await LogAuditEventAsync(context, true, $"Removed filesource '{datasourceId}'").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> FilesourceExistsAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(datasourceId, nameof(datasourceId))
            .Assert();

        var exists = await _filesourceRepository.FilesourceExistsAsync(datasourceId).ConfigureAwait(false);

        await LogAuditEventAsync(context, true, $"Checked existence of filesource '{datasourceId}'")
            .ConfigureAwait(false);
        return exists;
    }

    /// <inheritdoc />
    public async Task<FilesourceConnectivityTestDto> TestFilesourceAsync(
        IOperationContext context,
        string datasourceId,
        CancellationToken cancellationToken = default
    )
    {
        await AssertPermissionsAsync(context).ConfigureAwait(false);

        ValidationFactory
            .Arguments()
            .NotNull(context, nameof(context))
            .NotNullOrWhiteSpace(datasourceId, nameof(datasourceId))
            .Assert();

        var result = new FilesourceConnectivityTestDto { FilesourceId = datasourceId };
        var startTime = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var connection = await GetVfsConnectionAsync(datasourceId).ConfigureAwait(false);

            // Get filesource for provider info
            var filesource = await _filesourceRepository.GetFilesourceAsync(datasourceId).ConfigureAwait(false);

            // Test by attempting to list root directory
            await connection.ListFoldersAsync(null, cancellationToken).ConfigureAwait(false);

            result.Connected = true;
            result.Provider = filesource?.Provider;
            result.FilesourceName = filesource?.DisplayName;
            result.ResponseTimeMs = startTime.ElapsedMilliseconds;
            await LogAuditEventAsync(context, true, $"Tested filesource '{datasourceId}' connectivity")
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            result.Connected = false;
            result.ErrorMessage = ex.Message;
            result.ResponseTimeMs = startTime.ElapsedMilliseconds;
            await LogAuditEventAsync(context, false, ex.Message).ConfigureAwait(false);
        }

        return result;
    }

    #endregion // Filesource Methods
}
