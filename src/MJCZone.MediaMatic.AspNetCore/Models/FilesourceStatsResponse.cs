// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Statistics for an entire filesource.
/// </summary>
public class FilesourceStatsResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceStatsResponse"/> class.
    /// </summary>
    public FilesourceStatsResponse()
    {
        FilesourceId = string.Empty;
        ByType = new Dictionary<string, TypeStats>();
        TopFolders = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceStatsResponse"/> class.
    /// </summary>
    /// <param name="filesourceId">The filesource ID.</param>
    public FilesourceStatsResponse(string filesourceId)
    {
        FilesourceId = filesourceId;
        ByType = new Dictionary<string, TypeStats>();
        TopFolders = [];
    }

    /// <summary>
    /// Gets or sets the filesource ID.
    /// </summary>
    public string FilesourceId { get; set; }

    /// <summary>
    /// Gets or sets the bucket name (if applicable).
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the total size of all files in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Gets or sets the formatted total size (e.g., "15.2 GB").
    /// </summary>
    public string? TotalSizeFormatted { get; set; }

    /// <summary>
    /// Gets or sets the total number of files.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of folders.
    /// </summary>
    public int FolderCount { get; set; }

    /// <summary>
    /// Gets or sets the oldest file modification date.
    /// </summary>
    public DateTime? OldestFile { get; set; }

    /// <summary>
    /// Gets or sets the newest file modification date.
    /// </summary>
    public DateTime? NewestFile { get; set; }

    /// <summary>
    /// Gets or sets statistics broken down by MIME type.
    /// </summary>
    public Dictionary<string, TypeStats> ByType { get; set; }

    /// <summary>
    /// Gets or sets the top folders by size.
    /// </summary>
    public IEnumerable<FolderSizeInfo> TopFolders { get; set; }

    /// <summary>
    /// Gets or sets when the statistics were calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the statistics were retrieved from cache.
    /// </summary>
    public bool Cached { get; set; }
}

/// <summary>
/// Information about folder size for top folder ranking.
/// </summary>
public class FolderSizeInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderSizeInfo"/> class.
    /// </summary>
    public FolderSizeInfo()
    {
        Path = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderSizeInfo"/> class.
    /// </summary>
    /// <param name="path">The folder path.</param>
    /// <param name="size">The folder size in bytes.</param>
    /// <param name="fileCount">The number of files.</param>
    public FolderSizeInfo(string path, long size, int fileCount)
    {
        Path = path;
        Size = size;
        FileCount = fileCount;
    }

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the folder size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the formatted size (e.g., "2.5 GB").
    /// </summary>
    public string? SizeFormatted { get; set; }

    /// <summary>
    /// Gets or sets the number of files in the folder.
    /// </summary>
    public int FileCount { get; set; }
}
