// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for transcoding a video to a different format or codec.
/// </summary>
public class TranscodeOptions
{
    /// <summary>
    /// Gets or sets the target video format.
    /// Default is MP4.
    /// </summary>
    public VideoFormat Format { get; set; } = VideoFormat.Mp4;

    /// <summary>
    /// Gets or sets the target video codec.
    /// Null means use default for format (H.264 for MP4, VP9 for WebM).
    /// </summary>
    public VideoCodec? Codec { get; set; }

    /// <summary>
    /// Gets or sets the target width (null to maintain original or aspect ratio).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the target height (null to maintain original or aspect ratio).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the target video bitrate in kbps.
    /// Null means automatic based on resolution.
    /// </summary>
    public int? VideoBitrate { get; set; }

    /// <summary>
    /// Gets or sets the target audio bitrate in kbps.
    /// Null means automatic (typically 128 kbps).
    /// </summary>
    public int? AudioBitrate { get; set; }

    /// <summary>
    /// Gets or sets the target frame rate (fps).
    /// Null means keep original.
    /// </summary>
    public double? FrameRate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use hardware acceleration.
    /// Default is false.
    /// </summary>
    public bool UseHardwareAcceleration { get; set; }

    /// <summary>
    /// Gets or sets the preset for encoding speed vs quality tradeoff.
    /// Options: "ultrafast", "fast", "medium", "slow", "veryslow".
    /// Default is "medium".
    /// </summary>
    public string Preset { get; set; } = "medium";

    /// <summary>
    /// Gets or sets the Constant Rate Factor (CRF) for quality (0-51, lower is better).
    /// 23 is default for H.264, 28 for H.265. Null means use bitrate mode.
    /// </summary>
    public int? Crf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use two-pass encoding for better quality.
    /// Default is false (single pass).
    /// </summary>
    public bool TwoPassEncoding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to strip audio from the output.
    /// Default is false.
    /// </summary>
    public bool StripAudio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to strip metadata from the output.
    /// Default is false.
    /// </summary>
    public bool StripMetadata { get; set; }

    /// <summary>
    /// Gets or sets the output file path.
    /// Null means same directory with new extension.
    /// </summary>
    public string? OutputPath { get; set; }
}
