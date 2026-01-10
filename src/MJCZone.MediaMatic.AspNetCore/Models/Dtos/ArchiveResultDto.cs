// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Result of an archive operation.
/// </summary>
public class ArchiveResultDto
{
    /// <summary>
    /// Gets or sets the unique archive ID (filename).
    /// </summary>
    public required string ArchiveId { get; set; }

    /// <summary>
    /// Gets or sets the path to the archive file within the filesource.
    /// </summary>
    public required string ArchivePath { get; set; }

    /// <summary>
    /// Gets or sets the number of files included in the archive.
    /// </summary>
    public int FileCount { get; set; }
}
