// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using MJCZone.MediaMatic.Models;
using SkiaSharp;

namespace MJCZone.MediaMatic.Tests.TestHelpers;

/// <summary>
/// Helper class for generating test data (images, videos, etc.).
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Creates a simple test JPEG image.
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="quality">JPEG quality (1-100).</param>
    /// <returns>A MemoryStream containing the JPEG image.</returns>
    public static MemoryStream CreateTestJpeg(int width = 800, int height = 600, int quality = 85)
    {
        return CreateTestImage(width, height, SKEncodedImageFormat.Jpeg, quality);
    }

    /// <summary>
    /// Creates a simple test PNG image.
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>A MemoryStream containing the PNG image.</returns>
    public static MemoryStream CreateTestPng(int width = 800, int height = 600)
    {
        return CreateTestImage(width, height, SKEncodedImageFormat.Png, 100);
    }

    /// <summary>
    /// Creates a simple test WebP image.
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="quality">WebP quality (1-100).</param>
    /// <returns>A MemoryStream containing the WebP image.</returns>
    public static MemoryStream CreateTestWebP(int width = 800, int height = 600, int quality = 80)
    {
        return CreateTestImage(width, height, SKEncodedImageFormat.Webp, quality);
    }

    private static MemoryStream CreateTestImage(int width, int height, SKEncodedImageFormat format, int quality)
    {
        // Create a bitmap with a gradient pattern
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Fill with gradient background
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [SKColors.Blue, SKColors.Green, SKColors.Yellow],
                SKShaderTileMode.Clamp
            ),
        };
        canvas.DrawRect(0, 0, width, height, paint);

        // Draw some text to make it identifiable
        using var font = new SKFont { Size = 48 };
        using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText($"{width}x{height}", width / 2, height / 2, SKTextAlign.Center, font, textPaint);

        // Encode to stream
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);

        var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Creates a test image with EXIF metadata.
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>A MemoryStream containing a JPEG with EXIF data.</returns>
    public static MemoryStream CreateTestImageWithExif(int width = 1920, int height = 1080)
    {
        // For now, just return a standard JPEG
        // In the future, we could use a library to embed EXIF data
        return CreateTestJpeg(width, height);
    }

    /// <summary>
    /// Gets a sample image format for testing.
    /// </summary>
    public static ImageFormat GetSampleFormat(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".webp" => ImageFormat.WebP,
            ".gif" => ImageFormat.Gif,
            ".bmp" => ImageFormat.Bmp,
            ".avif" => ImageFormat.Avif,
            _ => ImageFormat.Jpeg,
        };
    }

    private static bool? _ffmpegAvailable;

    /// <summary>
    /// Checks if FFmpeg is available on the system.
    /// </summary>
    /// <returns>True if FFmpeg is installed and accessible, false otherwise.</returns>
    public static bool IsFfmpegAvailable()
    {
        if (_ffmpegAvailable.HasValue)
        {
            return _ffmpegAvailable.Value;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(5000);
            _ffmpegAvailable = process?.ExitCode == 0;
        }
        catch
        {
            _ffmpegAvailable = false;
        }

        return _ffmpegAvailable.Value;
    }

    /// <summary>
    /// Creates a test MP4 video file using FFmpeg.
    /// </summary>
    /// <param name="outputPath">The output file path for the video.</param>
    /// <param name="width">Video width in pixels.</param>
    /// <param name="height">Video height in pixels.</param>
    /// <param name="durationSeconds">Duration of the video in seconds.</param>
    /// <param name="frameRate">Frame rate of the video.</param>
    /// <returns>The path to the created video file.</returns>
    public static async Task<string> CreateTestMp4Async(
        string outputPath,
        int width = 320,
        int height = 240,
        int durationSeconds = 2,
        double frameRate = 30
    )
    {
        return await CreateTestVideoAsync(outputPath, width, height, durationSeconds, frameRate, "mp4");
    }

    /// <summary>
    /// Creates a test WebM video file using FFmpeg.
    /// </summary>
    /// <param name="outputPath">The output file path for the video.</param>
    /// <param name="width">Video width in pixels.</param>
    /// <param name="height">Video height in pixels.</param>
    /// <param name="durationSeconds">Duration of the video in seconds.</param>
    /// <param name="frameRate">Frame rate of the video.</param>
    /// <returns>The path to the created video file.</returns>
    public static async Task<string> CreateTestWebMAsync(
        string outputPath,
        int width = 320,
        int height = 240,
        int durationSeconds = 2,
        double frameRate = 30
    )
    {
        return await CreateTestVideoAsync(outputPath, width, height, durationSeconds, frameRate, "webm");
    }

    /// <summary>
    /// Creates a test video file using FFmpeg with a test pattern.
    /// </summary>
    /// <param name="outputPath">The output file path for the video.</param>
    /// <param name="width">Video width in pixels.</param>
    /// <param name="height">Video height in pixels.</param>
    /// <param name="durationSeconds">Duration of the video in seconds.</param>
    /// <param name="frameRate">Frame rate of the video.</param>
    /// <param name="format">Output format (mp4, webm, etc.).</param>
    /// <returns>The path to the created video file.</returns>
    public static async Task<string> CreateTestVideoAsync(
        string outputPath,
        int width = 320,
        int height = 240,
        int durationSeconds = 2,
        double frameRate = 30,
        string format = "mp4"
    )
    {
        if (!IsFfmpegAvailable())
        {
            throw new InvalidOperationException("FFmpeg is not available on this system.");
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Delete existing file if present
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Use FFmpeg to generate a test video with color bars pattern
        // testsrc2 creates a more visually interesting test pattern
        var arguments = format.ToLowerInvariant() switch
        {
            "webm" =>
                $"-f lavfi -i testsrc2=size={width}x{height}:rate={frameRate}:duration={durationSeconds} -c:v libvpx -b:v 1M -y \"{outputPath}\"",
            _ =>
                $"-f lavfi -i testsrc2=size={width}x{height}:rate={frameRate}:duration={durationSeconds} -c:v libx264 -pix_fmt yuv420p -y \"{outputPath}\"",
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process =
            Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start FFmpeg process.");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
        }

        if (!File.Exists(outputPath))
        {
            throw new InvalidOperationException($"FFmpeg did not create the output file: {outputPath}");
        }

        return outputPath;
    }

    /// <summary>
    /// Creates a test video file with audio track using FFmpeg.
    /// </summary>
    /// <param name="outputPath">The output file path for the video.</param>
    /// <param name="width">Video width in pixels.</param>
    /// <param name="height">Video height in pixels.</param>
    /// <param name="durationSeconds">Duration of the video in seconds.</param>
    /// <param name="frameRate">Frame rate of the video.</param>
    /// <returns>The path to the created video file.</returns>
    public static async Task<string> CreateTestVideoWithAudioAsync(
        string outputPath,
        int width = 320,
        int height = 240,
        int durationSeconds = 2,
        double frameRate = 30
    )
    {
        if (!IsFfmpegAvailable())
        {
            throw new InvalidOperationException("FFmpeg is not available on this system.");
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Delete existing file if present
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Generate video with test pattern and sine wave audio
        var arguments =
            $"-f lavfi -i testsrc2=size={width}x{height}:rate={frameRate}:duration={durationSeconds} "
            + $"-f lavfi -i sine=frequency=440:duration={durationSeconds} "
            + $"-c:v libx264 -pix_fmt yuv420p -c:a aac -b:a 128k -y \"{outputPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process =
            Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start FFmpeg process.");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
        }

        if (!File.Exists(outputPath))
        {
            throw new InvalidOperationException($"FFmpeg did not create the output file: {outputPath}");
        }

        return outputPath;
    }
}
