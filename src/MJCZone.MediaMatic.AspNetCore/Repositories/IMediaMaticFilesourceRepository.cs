// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Repositories;

/// <summary>
/// Repository interface for managing file storage source configurations.
/// </summary>
public interface IMediaMaticFilesourceRepository
{
    /// <summary>
    /// Initializes the repository, creating necessary storage structures if they don't exist.
    /// This method should be called once during application startup.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Adds a new filesource registration.
    /// </summary>
    /// <param name="filesource">The filesource to add.</param>
    /// <returns>True if added successfully, false if a filesource with the same ID already exists.</returns>
    Task<bool> AddFilesourceAsync(FilesourceDto filesource);

    /// <summary>
    /// Updates an existing filesource registration. Behaves like a patch. All NULL properties are ignored.
    /// </summary>
    /// <param name="filesource">The updated filesource information.</param>
    /// <returns>True if updated successfully, false if the filesource doesn't exist.</returns>
    Task<bool> UpdateFilesourceAsync(FilesourceDto filesource);

    /// <summary>
    /// Removes a filesource registration by ID.
    /// </summary>
    /// <param name="id">The ID of the filesource to remove.</param>
    /// <returns>True if removed successfully, false if the filesource doesn't exist.</returns>
    Task<bool> RemoveFilesourceAsync(string id);

    /// <summary>
    /// Gets a list of all registered filesources and their metadata (excluding connection strings).
    /// </summary>
    /// <param name="tag">Optional tag to filter filesources by. If null, returns all filesources.</param>
    /// <returns>A collection of filesource information without connection strings.</returns>
    Task<List<FilesourceDto>> GetFilesourcesAsync(string? tag = null);

    /// <summary>
    /// Gets filesource information by ID (excluding connection string).
    /// </summary>
    /// <param name="id">The ID of the filesource.</param>
    /// <returns>The filesource information without connection string, or null if not found.</returns>
    Task<FilesourceDto?> GetFilesourceAsync(string id);

    /// <summary>
    /// Checks if a filesource with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the filesource to check.</param>
    /// <returns>True if the filesource exists, false otherwise.</returns>
    Task<bool> FilesourceExistsAsync(string id);

    /// <summary>
    /// Gets a connection string for internal use by MediaMatic services.
    /// This method is for internal use only and should not be exposed through APIs.
    /// </summary>
    /// <param name="id">The ID of the filesource.</param>
    /// <returns>The connection string if found, null otherwise.</returns>
    Task<string?> GetConnectionStringAsync(string id);
}
