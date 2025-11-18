// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Represents a filesource registration for MediaMatic operations.
/// Contains connection information and metadata for filesource access.
/// </summary>
public sealed class FilesourceDto
{
    /// <summary>
    /// Gets or sets the unique name identifier for this filesource.
    /// </summary>
    [StringLength(64)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the file system provider type.
    /// </summary>
    [StringLength(10)]
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets the connection string for file system access.
    /// </summary>
    [StringLength(2000)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the display name for this filesource.
    /// </summary>
    [StringLength(128)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a description of this filesource.
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets additional tags for categorizing this filesource.
    /// </summary>
    public ICollection<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this filesource is enabled for use.
    /// </summary>
    public bool? IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets the date and time when this filesource was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the date and time when this filesource was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; internal set; } = DateTimeOffset.UtcNow;
}
