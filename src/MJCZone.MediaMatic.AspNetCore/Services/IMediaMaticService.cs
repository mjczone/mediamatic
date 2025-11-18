// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

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
}
