// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Status of an archive creation job.
/// </summary>
public class ArchiveJobStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveJobStatus"/> class.
    /// </summary>
    public ArchiveJobStatus()
    {
        JobId = string.Empty;
        Status = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveJobStatus"/> class.
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <param name="status">The job status.</param>
    public ArchiveJobStatus(string jobId, string status)
    {
        JobId = jobId;
        Status = status;
    }

    /// <summary>
    /// Gets or sets the job ID.
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Gets or sets the current status.
    /// Valid values: "pending", "processing", "completed", "failed".
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// Gets or sets the number of files processed.
    /// </summary>
    public int? FilesProcessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of files to process.
    /// </summary>
    public int? TotalFiles { get; set; }

    /// <summary>
    /// Gets or sets the error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the job was completed (successfully or failed).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the path to the completed archive file.
    /// </summary>
    public string? ArchivePath { get; set; }

    /// <summary>
    /// Gets or sets the size of the completed archive in bytes.
    /// </summary>
    public long? ArchiveSize { get; set; }
}
