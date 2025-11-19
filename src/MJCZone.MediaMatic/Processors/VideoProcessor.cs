// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FFMpegCore;
using FFMpegCore.Enums;
using MJCZone.MediaMatic.Models;
using FFCodec = FFMpegCore.Enums.VideoCodec;
using ModelCodec = MJCZone.MediaMatic.Models.VideoCodec;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Processes videos using FFMpegCore library (thumbnails, transcoding).
/// </summary>
public class VideoProcessor : IVideoProcessor
{
    /// <inheritdoc/>
    public async Task<List<string>> GenerateThumbnailsAsync(
        string videoPath,
        ThumbnailOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentNullException.ThrowIfNull(options);

        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Video file not found", videoPath);
        }

        var outputPath = options.OutputPath ?? Path.GetDirectoryName(videoPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(outputPath);

        var videoInfo = await FFProbe.AnalyseAsync(videoPath, cancellationToken: cancellationToken).ConfigureAwait(false);
        var duration = videoInfo.Duration.TotalSeconds;

        // Determine timestamps for thumbnails
        var timestamps = options.Timestamps ?? GenerateEvenlySpacedTimestamps(duration, options.Count);

        var generatedThumbnails = new List<string>();

        for (int i = 0; i < timestamps.Count; i++)
        {
            var timestamp = TimeSpan.FromSeconds(timestamps[i]);
            var filename = string.Format(options.FilePattern, i, timestamps[i]);
            var thumbnailPath = Path.Combine(outputPath, filename);

            // FFMpegCore SnapshotAsync doesn't support CancellationToken
#pragma warning disable CA2016 // Forward the CancellationToken parameter to methods
            await FFMpeg.SnapshotAsync(
                videoPath,
                thumbnailPath,
                new System.Drawing.Size(options.Width, options.Height ?? -1), // -1 maintains aspect ratio
                timestamp
            ).ConfigureAwait(false);
#pragma warning restore CA2016 // Forward the CancellationToken parameter to methods

            cancellationToken.ThrowIfCancellationRequested();

            generatedThumbnails.Add(thumbnailPath);
        }

        return generatedThumbnails;
    }

    /// <inheritdoc/>
    public async Task<string> TranscodeAsync(
        string sourcePath,
        string destinationPath,
        TranscodeOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(options);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source video file not found", sourcePath);
        }

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Build FFMpeg arguments
        var arguments = FFMpegArguments
            .FromFileInput(sourcePath)
            .OutputToFile(destinationPath, true, ffOptions =>
            {
                // Set video codec
                if (options.Codec.HasValue)
                {
                    var videoCodec = options.Codec.Value switch
                    {
                        ModelCodec.H264 => FFCodec.LibX264,
                        ModelCodec.H265 => FFCodec.LibX265,
                        ModelCodec.VP8 => FFCodec.LibVpx,
                        ModelCodec.VP9 => FFCodec.LibVpx,  // VP9 uses LibVpx
                        _ => FFCodec.LibX264,
                    };
                    ffOptions.WithVideoCodec(videoCodec);
                }
                else
                {
                    // Use default codec based on format
                    var defaultCodec = options.Format switch
                    {
                        VideoFormat.Mp4 => FFCodec.LibX264,
                        VideoFormat.WebM => FFCodec.LibVpx,  // VP9 for WebM
                        _ => FFCodec.LibX264,
                    };
                    ffOptions.WithVideoCodec(defaultCodec);
                }

                // Set video bitrate
                if (options.VideoBitrate.HasValue)
                {
                    ffOptions.WithVideoBitrate(options.VideoBitrate.Value);
                }

                // Set frame rate
                if (options.FrameRate.HasValue)
                {
                    ffOptions.WithFramerate(options.FrameRate.Value);
                }

                // Set resolution
                if (options.Width.HasValue || options.Height.HasValue)
                {
                    var width = options.Width ?? -1;   // -1 means maintain aspect ratio
                    var height = options.Height ?? -1;
                    ffOptions.WithVideoFilters(filterOptions => filterOptions.Scale(width, height));
                }

                // Set CRF (Constant Rate Factor) for quality-based encoding
                if (options.Crf.HasValue)
                {
                    ffOptions.WithConstantRateFactor(options.Crf.Value);
                }

                // Set audio bitrate or strip audio
                if (options.StripAudio)
                {
                    ffOptions.WithCustomArgument("-an");
                }
                else if (options.AudioBitrate.HasValue)
                {
                    ffOptions.WithAudioBitrate(options.AudioBitrate.Value);
                }

                // Set encoding preset
                ffOptions.WithSpeedPreset(options.Preset switch
                {
                    "ultrafast" => Speed.UltraFast,
                    "fast" => Speed.Fast,
                    "medium" => Speed.Medium,
                    "slow" => Speed.Slow,
                    "veryslow" => Speed.VerySlow,
                    _ => Speed.Medium,
                });

                // Hardware acceleration
                if (options.UseHardwareAcceleration)
                {
                    ffOptions.WithHardwareAcceleration();
                }

                // Strip metadata
                if (options.StripMetadata)
                {
                    ffOptions.WithCustomArgument("-map_metadata -1");
                }
            });

        await arguments.ProcessAsynchronously().ConfigureAwait(false);

        return destinationPath;
    }

    private static List<double> GenerateEvenlySpacedTimestamps(double duration, int count)
    {
        if (count <= 0)
        {
            return [];
        }

        if (count == 1)
        {
            return [duration / 2]; // Middle of video
        }

        var timestamps = new List<double>();
        var interval = duration / (count + 1);

        for (int i = 1; i <= count; i++)
        {
            timestamps.Add(interval * i);
        }

        return timestamps;
    }
}
