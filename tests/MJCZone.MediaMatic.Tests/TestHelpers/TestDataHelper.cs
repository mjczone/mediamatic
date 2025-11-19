// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.IO;
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
                new[] { SKColors.Blue, SKColors.Green, SKColors.Yellow },
                SKShaderTileMode.Clamp
            ),
        };
        canvas.DrawRect(0, 0, width, height, paint);

        // Draw some text to make it identifiable
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 48,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
        };
        canvas.DrawText($"{width}x{height}", width / 2, height / 2, textPaint);

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
}
