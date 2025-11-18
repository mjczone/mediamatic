// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for processing existing videos.
/// </summary>
public class VideoProcessingOptions
{
    /// <summary>
    /// Gets or sets the target video format.
    /// Null means keep original format.
    /// </summary>
    public VideoFormat? Format { get; set; }

    /// <summary>
    /// Gets or sets the target video codec.
    /// Null means use default for format.
    /// </summary>
    public VideoCodec? Codec { get; set; }

    /// <summary>
    /// Gets or sets the target width (null to maintain aspect ratio).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the target height (null to maintain aspect ratio).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the target bitrate in kbps.
    /// Null means automatic.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the target frame rate (fps).
    /// Null means keep original.
    /// </summary>
    public double? FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the start time for trimming (seconds).
    /// Null means start from beginning.
    /// </summary>
    public double? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for trimming (seconds).
    /// Null means to end of video.
    /// </summary>
    public double? EndTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to strip audio.
    /// Default is false.
    /// </summary>
    public bool StripAudio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to strip metadata.
    /// Default is false.
    /// </summary>
    public bool StripMetadata { get; set; }

    /// <summary>
    /// Gets or sets the audio bitrate in kbps.
    /// Null means automatic.
    /// </summary>
    public int? AudioBitrate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use hardware acceleration.
    /// Default is false.
    /// </summary>
    public bool UseHardwareAcceleration { get; set; }
}
