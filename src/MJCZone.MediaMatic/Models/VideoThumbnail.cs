// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a thumbnail extracted from a video.
/// </summary>
public class VideoThumbnail
{
    /// <summary>
    /// Gets or sets the path to the thumbnail file.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the timestamp in the video (seconds).
    /// </summary>
    public double Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the image format of the thumbnail.
    /// </summary>
    public ImageFormat Format { get; set; }
}
