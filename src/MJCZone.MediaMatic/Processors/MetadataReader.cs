// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Extracts metadata from images and videos using MetadataExtractor and FFMpegCore.
/// </summary>
public class MetadataReader : IMetadataReader
{
    /// <inheritdoc/>
    public Task<MediaMetadata> ExtractImageMetadataAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must be readable", nameof(stream));
        }

        var metadata = new MediaMetadata
        {
            Provider = "MetadataExtractor",
        };

        try
        {
            // Read all metadata directories from the image
            var directories = ImageMetadataReader.ReadMetadata(stream);

            // Extract basic image properties from any directory that has them
            foreach (var directory in directories)
            {
                if (directory.HasError)
                {
                    continue;
                }

                // Try to get width and height from various possible tag locations
                // Different formats store these in different places
                foreach (var tag in directory.Tags)
                {
                    var tagName = tag.Name?.ToLowerInvariant() ?? string.Empty;

                    if (tagName.Contains("width", StringComparison.Ordinal) && metadata.Width == null)
                    {
                        if (directory.TryGetInt32(tag.Type, out var width) && width > 0)
                        {
                            metadata.Width = width;
                        }
                    }

                    if (tagName.Contains("height", StringComparison.Ordinal) && metadata.Height == null)
                    {
                        if (directory.TryGetInt32(tag.Type, out var height) && height > 0)
                        {
                            metadata.Height = height;
                        }
                    }
                }
            }

            // Extract EXIF IFD0 Directory (camera info)
            var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (exifIfd0 != null)
            {
                metadata.CameraMake = exifIfd0.GetDescription(ExifDirectoryBase.TagMake);
                metadata.CameraModel = exifIfd0.GetDescription(ExifDirectoryBase.TagModel);
                metadata.Software = exifIfd0.GetDescription(ExifDirectoryBase.TagSoftware);
                metadata.DateTimeOriginal = exifIfd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dt) ? dt : null;

                // Orientation
                if (exifIfd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                {
                    metadata.Orientation = orientation;
                }
            }

            // Extract EXIF SubIFD Directory (exposure settings)
            var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSubIfd != null)
            {
                // ISO
                if (exifSubIfd.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                {
                    metadata.ISO = iso;
                }

                // Aperture (F-number)
                if (exifSubIfd.TryGetDouble(ExifDirectoryBase.TagFNumber, out var fNumber))
                {
                    metadata.Aperture = fNumber;
                }

                // Shutter speed (exposure time)
                if (exifSubIfd.TryGetDouble(ExifDirectoryBase.TagExposureTime, out var exposureTime))
                {
                    metadata.ShutterSpeed = exposureTime;
                }

                // Focal length
                if (exifSubIfd.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focalLength))
                {
                    metadata.FocalLength = focalLength;
                }

                // Focal length in 35mm
                if (exifSubIfd.TryGetInt32(ExifDirectoryBase.Tag35MMFilmEquivFocalLength, out var focalLength35))
                {
                    metadata.FocalLengthIn35mm = focalLength35;
                }

                // Exposure mode
                metadata.ExposureMode = exifSubIfd.GetDescription(ExifDirectoryBase.TagExposureMode);
                metadata.ExposureProgram = exifSubIfd.GetDescription(ExifDirectoryBase.TagExposureProgram);

                // Exposure bias
                if (exifSubIfd.TryGetDouble(ExifDirectoryBase.TagExposureBias, out var exposureBias))
                {
                    metadata.ExposureBias = exposureBias;
                }

                // Metering mode
                metadata.MeteringMode = exifSubIfd.GetDescription(ExifDirectoryBase.TagMeteringMode);

                // Flash
                metadata.Flash = exifSubIfd.GetDescription(ExifDirectoryBase.TagFlash);

                // White balance
                metadata.WhiteBalance = exifSubIfd.GetDescription(ExifDirectoryBase.TagWhiteBalance);

                // Lens info
                metadata.LensModel = exifSubIfd.GetDescription(ExifDirectoryBase.TagLensModel);
                metadata.LensMake = exifSubIfd.GetDescription(ExifDirectoryBase.TagLensMake);

                // Timestamps
                metadata.DateTimeOriginal = exifSubIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dtOriginal)
                    ? dtOriginal
                    : metadata.DateTimeOriginal;

                metadata.DateTimeDigitized = exifSubIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var dtDigitized)
                    ? dtDigitized
                    : null;
            }

            // Extract GPS Directory
            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsDirectory != null)
            {
                // Try to get GPS location
                try
                {
                    var latArray = gpsDirectory.GetRationalArray(GpsDirectory.TagLatitude);
                    if (latArray != null && latArray.Length >= 3)
                    {
                        var latDegrees = latArray[0].ToDouble();
                        var latMinutes = latArray[1].ToDouble();
                        var latSeconds = latArray[2].ToDouble();
                        var latRef = gpsDirectory.GetString(GpsDirectory.TagLatitudeRef);

                        var latitude = latDegrees + (latMinutes / 60.0) + (latSeconds / 3600.0);
                        if (latRef == "S")
                        {
                            latitude = -latitude;
                        }

                        metadata.Latitude = latitude;
                    }

                    var lonArray = gpsDirectory.GetRationalArray(GpsDirectory.TagLongitude);
                    if (lonArray != null && lonArray.Length >= 3)
                    {
                        var lonDegrees = lonArray[0].ToDouble();
                        var lonMinutes = lonArray[1].ToDouble();
                        var lonSeconds = lonArray[2].ToDouble();
                        var lonRef = gpsDirectory.GetString(GpsDirectory.TagLongitudeRef);

                        var longitude = lonDegrees + (lonMinutes / 60.0) + (lonSeconds / 3600.0);
                        if (lonRef == "W")
                        {
                            longitude = -longitude;
                        }

                        metadata.Longitude = longitude;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
                {
                    // GPS parsing failed, continue without GPS data
                }
#pragma warning restore CA1031 // Do not catch general exception types

                // Altitude
                try
                {
                    var altitudeRational = gpsDirectory.GetRational(GpsDirectory.TagAltitude);
                    metadata.Altitude = altitudeRational.ToDouble();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
                {
                    // Altitude parsing failed, continue without altitude data
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception)
        {
            // If metadata extraction fails, return partial metadata
            // Don't throw - allow the upload to continue with whatever metadata we could extract
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return Task.FromResult(metadata);
    }

    /// <inheritdoc/>
    public async Task<MediaMetadata> ExtractVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Video file not found", filePath);
        }

        var metadata = new MediaMetadata
        {
            Provider = "FFMpegCore",
            FileName = Path.GetFileName(filePath),
        };

        try
        {
            var mediaInfo = await FFMpegCore.FFProbe.AnalyseAsync(filePath, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Basic file properties
            metadata.Size = new FileInfo(filePath).Length;

            // Video duration
            metadata.Duration = mediaInfo.Duration;

            // Container format
            metadata.ContainerFormat = mediaInfo.Format.FormatName;

            // Video stream info
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            if (videoStream != null)
            {
                metadata.Width = videoStream.Width;
                metadata.Height = videoStream.Height;
                metadata.VideoCodec = videoStream.CodecName;
                metadata.FrameRate = videoStream.FrameRate;
                metadata.VideoBitrate = (int?)videoStream.BitRate;
            }

            // Audio stream info
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            if (audioStream != null)
            {
                metadata.AudioCodec = audioStream.CodecName;
                metadata.AudioBitrate = (int?)audioStream.BitRate;
                metadata.SampleRate = audioStream.SampleRateHz;
                metadata.Channels = audioStream.Channels;
            }

            // Total bitrate
            if (mediaInfo.PrimaryVideoStream?.BitRate != null)
            {
                var totalBitrate = mediaInfo.PrimaryVideoStream.BitRate;
                if (mediaInfo.PrimaryAudioStream?.BitRate != null)
                {
                    totalBitrate += mediaInfo.PrimaryAudioStream.BitRate;
                }

                metadata.TotalBitrate = (int)totalBitrate;
            }
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception)
        {
            // If metadata extraction fails, return partial metadata
            // Don't throw - allow the upload to continue with whatever metadata we could extract
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return metadata;
    }
}
