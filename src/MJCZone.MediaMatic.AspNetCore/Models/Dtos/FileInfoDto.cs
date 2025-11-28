// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Represents file information returned by browse operations.
/// </summary>
public sealed class FileInfoDto
{
    /// <summary>
    /// Gets or sets the full path of the file.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name (without directory path).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>
    /// Gets or sets the file extension (e.g., ".jpg", ".pdf").
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the file category based on extension (e.g., "image", "video", "document").
    /// </summary>
    public string? Category { get; set; }
}
