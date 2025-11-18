// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Result of a video upload operation.
/// </summary>
public class VideoUploadResult
{
    /// <summary>
    /// Gets or sets the path to the uploaded video.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the video width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the video height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the detected video format.
    /// </summary>
    public VideoFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the video codec.
    /// </summary>
    public string? VideoCodec { get; set; }

    /// <summary>
    /// Gets or sets the audio codec.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Gets or sets the bitrate in kbps.
    /// </summary>
    public int Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the frame rate (fps).
    /// </summary>
    public double FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the extracted metadata.
    /// </summary>
    public MediaMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the generated thumbnails.
    /// </summary>
    public List<VideoThumbnail> Thumbnails { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the upload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets error message if upload failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
