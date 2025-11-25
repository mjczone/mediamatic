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

    /// <summary>
    /// Gets or sets a value indicating whether to include files recursively from folders.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the duration after which the archive should be automatically deleted.
    /// Format: "7d" (7 days), "24h" (24 hours), "30m" (30 minutes).
    /// If null, archive is never automatically deleted.
    /// </summary>
    public string? DeleteAfter { get; set; }
}
