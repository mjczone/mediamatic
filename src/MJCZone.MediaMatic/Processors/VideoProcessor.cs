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

        var videoInfo = await FFProbe
            .AnalyseAsync(videoPath, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var duration = videoInfo.Duration.TotalSeconds;

        // Determine timestamps for thumbnails
        var timestamps = options.Timestamps ?? GenerateEvenlySpacedTimestamps(duration, options.Count);

        var generatedThumbnails = new List<string>();

        // Get video dimensions for calculating resize
        var videoWidth = videoInfo.PrimaryVideoStream?.Width ?? 1920;
        var videoHeight = videoInfo.PrimaryVideoStream?.Height ?? 1080;

        for (int i = 0; i < timestamps.Count; i++)
        {
            var timestamp = TimeSpan.FromSeconds(timestamps[i]);
            var filename = string.Format(options.FilePattern, i, timestamps[i]);
            var thumbnailPath = Path.Combine(outputPath, filename);

            // Build FFMpeg filter based on resize mode
            var targetWidth = options.Width;
            var targetHeight = options.Height ?? (int)(options.Width * ((double)videoHeight / videoWidth));

            var filterString = BuildResizeFilter(
                options.ResizeMode,
                targetWidth,
                targetHeight,
                options.BackgroundColor,
                options.FocalPoint
            );

            await FFMpegArguments
                .FromFileInput(videoPath, true, inputOptions => inputOptions.Seek(timestamp))
                .OutputToFile(
                    thumbnailPath,
                    true,
                    outputOptions =>
                    {
                        outputOptions
                            .WithCustomArgument($"-vf \"{filterString}\"")
                            .WithFrameOutputCount(1)
                            .WithCustomArgument($"-q:v {Math.Max(1, 100 - options.Quality)}"); // Quality for JPEG
                    }
                )
                .ProcessAsynchronously()
                .ConfigureAwait(false);

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
            .OutputToFile(
                destinationPath,
                true,
                ffOptions =>
                {
                    // Set video codec
                    if (options.Codec.HasValue)
                    {
                        var videoCodec = options.Codec.Value switch
                        {
                            ModelCodec.H264 => FFCodec.LibX264,
                            ModelCodec.H265 => FFCodec.LibX265,
                            ModelCodec.VP8 => FFCodec.LibVpx,
                            ModelCodec.VP9 => FFCodec.LibVpx, // VP9 uses LibVpx
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
                            VideoFormat.WebM => FFCodec.LibVpx, // VP9 for WebM
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
                        var width = options.Width ?? -1; // -1 means maintain aspect ratio
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
                    ffOptions.WithSpeedPreset(
                        options.Preset switch
                        {
                            "ultrafast" => Speed.UltraFast,
                            "fast" => Speed.Fast,
                            "medium" => Speed.Medium,
                            "slow" => Speed.Slow,
                            "veryslow" => Speed.VerySlow,
                            _ => Speed.Medium,
                        }
                    );

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
                }
            );

        await arguments.ProcessAsynchronously().ConfigureAwait(false);

        return destinationPath;
    }

    private static string BuildResizeFilter(
        Models.ResizeMode resizeMode,
        int targetWidth,
        int targetHeight,
        string backgroundColor,
        FocalPoint? focalPoint
    )
    {
        // Parse background color for pad filter
        var bgColor = backgroundColor.TrimStart('#');
        if (bgColor.Length == 6)
        {
            bgColor = "0x" + bgColor;
        }

        return resizeMode switch
        {
            Models.ResizeMode.Cover => BuildCoverFilter(targetWidth, targetHeight, focalPoint),

            Models.ResizeMode.Pad =>
            // Scale to fit, then pad with background color
            $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=decrease,pad={targetWidth}:{targetHeight}:(ow-iw)/2:(oh-ih)/2:{bgColor}",

            Models.ResizeMode.Stretch =>
            // Stretch to exact dimensions
            $"scale={targetWidth}:{targetHeight}",

            Models.ResizeMode.Fit or _ =>
            // Scale to fit within bounds
            $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=decrease",
        };
    }

    private static string BuildCoverFilter(int targetWidth, int targetHeight, FocalPoint? focalPoint)
    {
        // Scale to cover, then crop
        if (focalPoint != null)
        {
            // Crop around focal point
            // FFMpeg crop: crop=w:h:x:y
            // For focal point, we calculate x and y based on the focal point position
            // The expressions use 'iw' (input width) and 'ih' (input height) after scaling
            var focalX = focalPoint.X;
            var focalY = focalPoint.Y;

            // Calculate crop position centered on focal point, clamped to bounds
            // Using FFMpeg expressions: min(max(...), ...) for clamping
            // Escape commas with backslash for FFMpeg expression parser
            var cropX = $"min(max(iw*{focalX:F2}-{targetWidth / 2}\\,0)\\,iw-{targetWidth})";
            var cropY = $"min(max(ih*{focalY:F2}-{targetHeight / 2}\\,0)\\,ih-{targetHeight})";

            return $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=increase,crop={targetWidth}:{targetHeight}:{cropX}:{cropY}";
        }

        // Default: center crop
        return $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=increase,crop={targetWidth}:{targetHeight}";
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
