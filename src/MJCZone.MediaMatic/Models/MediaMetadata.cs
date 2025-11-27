// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents  metadata for a media file.
/// </summary>
public class MediaMetadata
{
    // Basic file properties

    /// <summary>
    /// Gets or sets the name of the media file.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the media file.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the file.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification date and time of the file.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the metadata provider name.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets custom metadata key-value pairs.
    /// </summary>
    public Dictionary<string, string>? CustomMetadata { get; set; }

    // Image-specific

    /// <summary>
    /// Gets or sets the width of the image in pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the image in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the color space of the image.
    /// </summary>
    public string? ColorSpace { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the image.
    /// </summary>
    public int? Orientation { get; set; }

    /// <summary>
    /// Gets or sets the bit depth of the image.
    /// </summary>
    public int? BitDepth { get; set; }

    // EXIF - Camera

    /// <summary>
    /// Gets or sets the camera manufacturer.
    /// </summary>
    public string? CameraMake { get; set; }

    /// <summary>
    /// Gets or sets the camera model.
    /// </summary>
    public string? CameraModel { get; set; }

    /// <summary>
    /// Gets or sets the lens manufacturer.
    /// </summary>
    public string? LensMake { get; set; }

    /// <summary>
    /// Gets or sets the lens model.
    /// </summary>
    public string? LensModel { get; set; }

    /// <summary>
    /// Gets or sets the software used to process the image.
    /// </summary>
    public string? Software { get; set; }

    // EXIF - Exposure settings

    /// <summary>
    /// Gets or sets the ISO sensitivity.
    /// </summary>
    public double? ISO { get; set; }

    /// <summary>
    /// Gets or sets the aperture value (f-number).
    /// </summary>
    public double? Aperture { get; set; }

    /// <summary>
    /// Gets or sets the shutter speed in seconds.
    /// </summary>
    public double? ShutterSpeed { get; set; }

    /// <summary>
    /// Gets or sets the focal length in millimeters.
    /// </summary>
    public double? FocalLength { get; set; }

    /// <summary>
    /// Gets or sets the focal length in 35mm equivalent.
    /// </summary>
    public double? FocalLengthIn35mm { get; set; }

    /// <summary>
    /// Gets or sets the exposure mode.
    /// </summary>
    public string? ExposureMode { get; set; }

    /// <summary>
    /// Gets or sets the exposure program.
    /// </summary>
    public string? ExposureProgram { get; set; }

    /// <summary>
    /// Gets or sets the exposure bias value.
    /// </summary>
    public double? ExposureBias { get; set; }

    /// <summary>
    /// Gets or sets the metering mode.
    /// </summary>
    public string? MeteringMode { get; set; }

    /// <summary>
    /// Gets or sets the flash mode or status.
    /// </summary>
    public string? Flash { get; set; }

    /// <summary>
    /// Gets or sets the white balance mode.
    /// </summary>
    public string? WhiteBalance { get; set; }

    // EXIF - GPS

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the altitude in meters.
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Gets or sets the original date and time when the image was taken.
    /// </summary>
    public DateTime? DateTimeOriginal { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the image was digitized.
    /// </summary>
    public DateTime? DateTimeDigitized { get; set; }

    // Video-specific

    /// <summary>
    /// Gets or sets the duration of the video or audio.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets the video codec.
    /// </summary>
    public string? VideoCodec { get; set; }

    /// <summary>
    /// Gets or sets the audio codec.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Gets or sets the frame rate in frames per second.
    /// </summary>
    public double? FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the video bitrate in bits per second.
    /// </summary>
    public int? VideoBitrate { get; set; }

    /// <summary>
    /// Gets or sets the audio bitrate in bits per second.
    /// </summary>
    public int? AudioBitrate { get; set; }

    /// <summary>
    /// Gets or sets the container format.
    /// </summary>
    public string? ContainerFormat { get; set; }

    /// <summary>
    /// Gets or sets the total bitrate in bits per second.
    /// </summary>
    public int? TotalBitrate { get; set; }

    // Audio-specific

    /// <summary>
    /// Gets or sets the audio sample rate in Hz.
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the number of audio channels.
    /// </summary>
    public int? Channels { get; set; }

    /// <summary>
    /// Gets or sets the title of the media.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the artist name.
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// Gets or sets the album name.
    /// </summary>
    public string? Album { get; set; }

    /// <summary>
    /// Gets or sets the album artist name.
    /// </summary>
    public string? AlbumArtist { get; set; }

    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets the year of release.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the track number.
    /// </summary>
    public int? TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the copyright information.
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>
    /// Gets or sets the description of the media.
    /// </summary>
    public string? Description { get; set; }
}
