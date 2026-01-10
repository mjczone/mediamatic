// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Result of a browse operation.
/// </summary>
public class BrowseResultDto
{
    /// <summary>
    /// Gets or sets the list of folders.
    /// </summary>
    public IEnumerable<FolderInfoDto> Folders { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of files.
    /// </summary>
    public IEnumerable<FileInfoDto> Files { get; set; } = [];
}
