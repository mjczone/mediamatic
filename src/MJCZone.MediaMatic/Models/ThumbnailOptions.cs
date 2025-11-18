// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for generating thumbnails from a video.
/// </summary>
public class ThumbnailOptions
{
    /// <summary>
    /// Gets or sets the number of thumbnails to generate.
    /// Default is 3.
    /// </summary>
    public int Count { get; set; } = 3;

    /// <summary>
    /// Gets or sets specific timestamps (in seconds) for thumbnail generation.
    /// Null means evenly spaced throughout video.
    /// </summary>
    public List<double>? Timestamps { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail width in pixels.
    /// Default is 320. Height will maintain aspect ratio.
    /// </summary>
    public int Width { get; set; } = 320;

    /// <summary>
    /// Gets or sets the thumbnail height in pixels.
    /// Null means maintain aspect ratio based on width.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the output format for thumbnails.
    /// Default is JPEG.
    /// </summary>
    public ImageFormat Format { get; set; } = ImageFormat.Jpeg;

    /// <summary>
    /// Gets or sets the quality (1-100) for lossy formats.
    /// Default is 85.
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Gets or sets the output directory for thumbnails.
    /// Null means same directory as video.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the filename pattern for thumbnails.
    /// Use {0} for thumbnail index, {1} for timestamp.
    /// Default is "thumb_{0}.jpg".
    /// </summary>
    public string FilePattern { get; set; } = "thumb_{0}.jpg";
}
