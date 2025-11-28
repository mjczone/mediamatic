// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Response containing files and folders from a browse operation.
/// </summary>
public sealed class BrowseResponseDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseResponseDto"/> class.
    /// </summary>
    public BrowseResponseDto()
    {
        Folders = [];
        Files = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseResponseDto"/> class.
    /// </summary>
    /// <param name="folders">The list of folders.</param>
    /// <param name="files">The list of files.</param>
    public BrowseResponseDto(IEnumerable<FolderInfoDto> folders, IEnumerable<FileInfoDto> files)
    {
        Folders = folders;
        Files = files;
    }

    /// <summary>
    /// Gets or sets the list of folders.
    /// </summary>
    public IEnumerable<FolderInfoDto> Folders { get; set; }

    /// <summary>
    /// Gets or sets the list of files.
    /// </summary>
    public IEnumerable<FileInfoDto> Files { get; set; }
}
