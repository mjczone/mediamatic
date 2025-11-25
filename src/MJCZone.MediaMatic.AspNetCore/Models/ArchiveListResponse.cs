// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Response containing a list of archives.
/// </summary>
public class ArchiveListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveListResponse"/> class.
    /// </summary>
    public ArchiveListResponse()
    {
        Archives = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveListResponse"/> class.
    /// </summary>
    /// <param name="archives">The list of archives.</param>
    public ArchiveListResponse(IEnumerable<ArchiveInfo> archives)
    {
        Archives = archives;
    }

    /// <summary>
    /// Gets or sets the list of archives.
    /// </summary>
    public IEnumerable<ArchiveInfo> Archives { get; set; }
}

/// <summary>
/// Information about an archive file.
/// </summary>
public class ArchiveInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveInfo"/> class.
    /// </summary>
    public ArchiveInfo()
    {
        ArchiveId = string.Empty;
        FileName = string.Empty;
        Path = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveInfo"/> class.
    /// </summary>
    /// <param name="archiveId">The archive ID.</param>
    /// <param name="fileName">The archive file name.</param>
    /// <param name="path">The archive file path.</param>
    /// <param name="size">The archive size in bytes.</param>
    /// <param name="createdAt">When the archive was created.</param>
    public ArchiveInfo(string archiveId, string fileName, string path, long size, DateTime createdAt)
    {
        ArchiveId = archiveId;
        FileName = fileName;
        Path = path;
        Size = size;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets or sets the unique archive ID.
    /// </summary>
    public string ArchiveId { get; set; }

    /// <summary>
    /// Gets or sets the archive file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the archive file path within the filesource.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the archive size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets when the archive was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the archive will be automatically deleted (if configured).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
