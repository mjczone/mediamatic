// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models;

/// <summary>
/// Statistics for a folder.
/// </summary>
public class FolderStatsResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderStatsResponse"/> class.
    /// </summary>
    public FolderStatsResponse()
    {
        Path = string.Empty;
        ByType = new Dictionary<string, TypeStats>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderStatsResponse"/> class.
    /// </summary>
    /// <param name="path">The folder path.</param>
    public FolderStatsResponse(string path)
    {
        Path = path;
        ByType = new Dictionary<string, TypeStats>();
    }

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the total size of all files in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Gets or sets the formatted total size (e.g., "1.50 GB").
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
    /// Gets or sets when the statistics were calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the statistics were retrieved from cache.
    /// </summary>
    public bool Cached { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the statistics include subdirectories recursively.
    /// </summary>
    public bool Recursive { get; set; }
}

/// <summary>
/// Statistics for a specific file type.
/// </summary>
public class TypeStats
{
    /// <summary>
    /// Gets or sets the number of files of this type.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the total size of files of this type in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the formatted size (e.g., "500 MB").
    /// </summary>
    public string? SizeFormatted { get; set; }
}
