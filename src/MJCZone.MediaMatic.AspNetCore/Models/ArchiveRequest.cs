// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Request to create an archive of files and folders.
/// </summary>
public class ArchiveRequest
{
    /// <summary>
    /// Gets or sets the name of the archive file (without extension).
    /// If not specified, defaults to timestamp-based name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the list of file and folder paths to include in the archive.
    /// Used for file list archives. Folders should end with '/'.
    /// </summary>
    public List<string>? Paths { get; set; }

    /// <summary>
    /// Gets or sets the compression format.
    /// Valid values: "zip" (default), "tar", "tar.gz".
    /// </summary>
    public string Compression { get; set; } = "zip";
}
