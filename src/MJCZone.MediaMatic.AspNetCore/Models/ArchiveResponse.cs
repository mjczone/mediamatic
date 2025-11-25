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
        JobId = string.Empty;
        ArchiveId = string.Empty;
        ArchivePath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveResponse"/> class.
    /// </summary>
    /// <param name="jobId">The background job ID.</param>
    /// <param name="archiveId">The archive ID.</param>
    /// <param name="archivePath">The path to the archive file.</param>
    public ArchiveResponse(string jobId, string archiveId, string archivePath)
    {
        JobId = jobId;
        ArchiveId = archiveId;
        ArchivePath = archivePath;
    }

    /// <summary>
    /// Gets or sets the background job ID for tracking archive creation progress.
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Gets or sets the unique archive ID.
    /// </summary>
    public string ArchiveId { get; set; }

    /// <summary>
    /// Gets or sets the path to the archive file within the filesource.
    /// </summary>
    public string ArchivePath { get; set; }

    /// <summary>
    /// Gets or sets the download URL for the archive (if available).
    /// </summary>
    public Uri? DownloadUrl { get; set; }

    /// <summary>
    /// Gets or sets the status of the archive creation.
    /// </summary>
    public ArchiveJobStatus? Status { get; set; }
}
