// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Represents folder information returned by browse operations.
/// </summary>
public sealed class FolderInfoDto
{
    /// <summary>
    /// Gets or sets the full path of the folder.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder name (without parent directory path).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
