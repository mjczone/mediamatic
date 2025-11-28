// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Response for archive creation request.
/// </summary>
public class ArchiveResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveResponse"/> class.
    /// </summary>
    public ArchiveResponse()
    {
        ArchiveId = string.Empty;
        ArchivePath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveResponse"/> class.
    /// </summary>
    /// <param name="archiveId">The archive ID.</param>
    /// <param name="archivePath">The path to the archive file.</param>
    /// <param name="fileCount">The number of files in the archive.</param>
    public ArchiveResponse(string archiveId, string archivePath, int fileCount)
    {
        ArchiveId = archiveId;
        ArchivePath = archivePath;
        FileCount = fileCount;
    }

    /// <summary>
    /// Gets or sets the unique archive ID (filename).
    /// </summary>
    public string ArchiveId { get; set; }

    /// <summary>
    /// Gets or sets the path to the archive file within the filesource.
    /// </summary>
    public string ArchivePath { get; set; }

    /// <summary>
    /// Gets or sets the number of files included in the archive.
    /// </summary>
    public int FileCount { get; set; }
}
