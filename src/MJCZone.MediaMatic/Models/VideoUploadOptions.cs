// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for uploading and processing videos.
/// </summary>
public class VideoUploadOptions
{
    /// <summary>
    /// Gets or sets the target path for the uploaded video.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate video thumbnails.
    /// Default is true.
    /// </summary>
    public bool GenerateThumbnails { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of thumbnails to generate.
    /// Default is 3.
    /// </summary>
    public int ThumbnailCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets specific timestamps (in seconds) for thumbnail generation.
    /// Null means evenly spaced throughout video.
    /// </summary>
    public List<double>? ThumbnailTimestamps { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail width in pixels.
    /// Default is 320.
    /// </summary>
    public int ThumbnailWidth { get; set; } = 320;

    /// <summary>
    /// Gets or sets a value indicating whether to transcode the video to standard formats.
    /// Default is false.
    /// </summary>
    public bool Transcode { get; set; }

    /// <summary>
    /// Gets or sets the target video format for transcoding.
    /// Null means keep original format.
    /// </summary>
    public VideoFormat? TargetFormat { get; set; }

    /// <summary>
    /// Gets or sets the target video codec.
    /// Null means use default for format.
    /// </summary>
    public VideoCodec? TargetCodec { get; set; }

    /// <summary>
    /// Gets or sets the maximum width for the video (will resize if larger).
    /// Null means no maximum.
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height for the video (will resize if larger).
    /// Null means no maximum.
    /// </summary>
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets the target bitrate in kbps.
    /// Null means automatic.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to extract metadata (duration, codec, dimensions).
    /// Default is true.
    /// </summary>
    public bool ExtractMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to strip all metadata.
    /// Default is false.
    /// </summary>
    public bool StripMetadata { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing files.
    /// Default is false.
    /// </summary>
    public bool Overwrite { get; set; }
}
