// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;
using SkiaSharp;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Processes images using SkiaSharp library (resize, convert, optimize).
/// </summary>
public class ImageProcessor : IImageProcessor
{
    /// <inheritdoc/>
    public Task<ProcessedImage> ResizeAsync(
        Stream inputStream,
        int? width,
        int? height,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        if (!width.HasValue && !height.HasValue)
        {
            throw new ArgumentException("At least one dimension (width or height) must be specified");
        }

        options ??= new ImageProcessingOptions();

        using var inputBitmap = SKBitmap.Decode(inputStream);
        if (inputBitmap == null)
        {
            throw new InvalidOperationException("Failed to decode image");
        }

        // Determine final output dimensions
        var outputWidth = width ?? inputBitmap.Width;
        var outputHeight = height ?? inputBitmap.Height;

        // Apply resize mode
        SKBitmap resultBitmap;
        switch (options.ResizeMode)
        {
            case ResizeMode.Cover:
                resultBitmap = ApplyCoverMode(inputBitmap, outputWidth, outputHeight, options.FocalPoint);
                break;

            case ResizeMode.Pad:
                resultBitmap = ApplyPadMode(inputBitmap, outputWidth, outputHeight, options.BackgroundColor);
                break;

            case ResizeMode.Stretch:
                resultBitmap = ApplyStretchMode(inputBitmap, outputWidth, outputHeight);
                break;

            case ResizeMode.Fit:
            default:
                resultBitmap = ApplyFitMode(inputBitmap, width, height, out outputWidth, out outputHeight);
                break;
        }

        using (resultBitmap)
        {
            // Encode to output format
            var format = options.Format ?? ImageFormat.Jpeg;
            var outputStream = new MemoryStream();
            using var image = SKImage.FromBitmap(resultBitmap);
            using var encodedData = EncodeImage(image, format, options.Quality);
            encodedData.SaveTo(outputStream);

            outputStream.Position = 0;
            return Task.FromResult(
                new ProcessedImage(outputStream, outputWidth, outputHeight, outputStream.Length, format)
            );
        }
    }

    /// <inheritdoc/>
    public Task<ProcessedImage> ConvertFormatAsync(
        Stream inputStream,
        ImageFormat targetFormat,
        int quality,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        using var inputBitmap = SKBitmap.Decode(inputStream);
        if (inputBitmap == null)
        {
            throw new InvalidOperationException("Failed to decode image");
        }

        var outputStream = new MemoryStream();
        using var image = SKImage.FromBitmap(inputBitmap);
        using var encodedData = EncodeImage(image, targetFormat, quality);
        encodedData.SaveTo(outputStream);

        outputStream.Position = 0;
        return Task.FromResult(
            new ProcessedImage(outputStream, inputBitmap.Width, inputBitmap.Height, outputStream.Length, targetFormat)
        );
    }

    /// <inheritdoc/>
    public Task<List<ProcessedImage>> GenerateVariantsAsync(
        Stream inputStream,
        ImageUploadOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNull(options);

        // For now, GenerateVariantsAsync is not meant to be called directly
        // It will be orchestrated by VfsMethodsBase.UploadImageAsync
        throw new NotImplementedException(
            "GenerateVariantsAsync should be called through VfsMethodsBase.UploadImageAsync orchestration"
        );
    }

    private static SKBitmap ApplyFitMode(
        SKBitmap input,
        int? targetWidth,
        int? targetHeight,
        out int outputWidth,
        out int outputHeight
    )
    {
        // Calculate dimensions maintaining aspect ratio (fit within bounds)
        if (targetWidth.HasValue && targetHeight.HasValue)
        {
            var scaleX = (double)targetWidth.Value / input.Width;
            var scaleY = (double)targetHeight.Value / input.Height;
            var scale = Math.Min(scaleX, scaleY);
            outputWidth = (int)(input.Width * scale);
            outputHeight = (int)(input.Height * scale);
        }
        else if (targetWidth.HasValue)
        {
            var aspectRatio = (double)input.Height / input.Width;
            outputWidth = targetWidth.Value;
            outputHeight = (int)(targetWidth.Value * aspectRatio);
        }
        else if (targetHeight.HasValue)
        {
            var aspectRatio = (double)input.Width / input.Height;
            outputWidth = (int)(targetHeight.Value * aspectRatio);
            outputHeight = targetHeight.Value;
        }
        else
        {
            outputWidth = input.Width;
            outputHeight = input.Height;
        }

        var imageInfo = new SKImageInfo(outputWidth, outputHeight);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
        var resized = input.Resize(imageInfo, samplingOptions);

        if (resized == null)
        {
            throw new InvalidOperationException("Failed to resize image in Fit mode");
        }

        return resized;
    }

    private static SKBitmap ApplyCoverMode(SKBitmap input, int targetWidth, int targetHeight, FocalPoint? focalPoint)
    {
        // Scale to cover the entire target area, then crop around focal point
        var scaleX = (double)targetWidth / input.Width;
        var scaleY = (double)targetHeight / input.Height;
        var scale = Math.Max(scaleX, scaleY); // Use larger scale to cover

        var scaledWidth = (int)(input.Width * scale);
        var scaledHeight = (int)(input.Height * scale);

        // First resize to cover
        var imageInfo = new SKImageInfo(scaledWidth, scaledHeight);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
        using var scaled = input.Resize(imageInfo, samplingOptions);

        if (scaled == null)
        {
            throw new InvalidOperationException("Failed to resize image in Cover mode");
        }

        // Calculate crop position based on focal point (default to center)
        int cropX,
            cropY;
        if (focalPoint != null)
        {
            // Focal point is in normalized coordinates (0-1)
            // Calculate the focal point position in the scaled image
            var focalX = (int)(focalPoint.X * scaledWidth);
            var focalY = (int)(focalPoint.Y * scaledHeight);

            // Calculate crop rectangle centered on focal point, but clamped to image bounds
            cropX = Math.Clamp(focalX - (targetWidth / 2), 0, scaledWidth - targetWidth);
            cropY = Math.Clamp(focalY - (targetHeight / 2), 0, scaledHeight - targetHeight);
        }
        else
        {
            // Default to center crop
            cropX = (scaledWidth - targetWidth) / 2;
            cropY = (scaledHeight - targetHeight) / 2;
        }

        var cropRect = new SKRectI(cropX, cropY, cropX + targetWidth, cropY + targetHeight);

        var result = new SKBitmap(targetWidth, targetHeight);
        using var canvas = new SKCanvas(result);
        canvas.DrawBitmap(scaled, cropRect, new SKRect(0, 0, targetWidth, targetHeight));

        return result;
    }

    private static SKBitmap ApplyPadMode(SKBitmap input, int targetWidth, int targetHeight, string backgroundColor)
    {
        // Scale to fit within bounds, then pad to fill remaining space
        var scaleX = (double)targetWidth / input.Width;
        var scaleY = (double)targetHeight / input.Height;
        var scale = Math.Min(scaleX, scaleY); // Use smaller scale to fit

        var scaledWidth = (int)(input.Width * scale);
        var scaledHeight = (int)(input.Height * scale);

        // First resize to fit
        var imageInfo = new SKImageInfo(scaledWidth, scaledHeight);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
        using var scaled = input.Resize(imageInfo, samplingOptions);

        if (scaled == null)
        {
            throw new InvalidOperationException("Failed to resize image in Pad mode");
        }

        // Create result with padding
        var result = new SKBitmap(targetWidth, targetHeight);
        using var canvas = new SKCanvas(result);

        // Fill with background color
        var bgColor = ParseHexColor(backgroundColor);
        canvas.Clear(bgColor);

        // Center the scaled image
        var offsetX = (targetWidth - scaledWidth) / 2;
        var offsetY = (targetHeight - scaledHeight) / 2;
        canvas.DrawBitmap(scaled, offsetX, offsetY);

        return result;
    }

    private static SKBitmap ApplyStretchMode(SKBitmap input, int targetWidth, int targetHeight)
    {
        // Simply stretch to exact dimensions, ignoring aspect ratio
        var imageInfo = new SKImageInfo(targetWidth, targetHeight);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
        var resized = input.Resize(imageInfo, samplingOptions);

        if (resized == null)
        {
            throw new InvalidOperationException("Failed to resize image in Stretch mode");
        }

        return resized;
    }

    private static SKColor ParseHexColor(string hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return SKColors.White;
        }

        // Remove # prefix if present
        var hex = hexColor.TrimStart('#');

        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new SKColor(r, g, b);
        }

        if (hex.Length == 8)
        {
            var a = Convert.ToByte(hex.Substring(0, 2), 16);
            var r = Convert.ToByte(hex.Substring(2, 2), 16);
            var g = Convert.ToByte(hex.Substring(4, 2), 16);
            var b = Convert.ToByte(hex.Substring(6, 2), 16);
            return new SKColor(r, g, b, a);
        }

        return SKColors.White;
    }

    private static SKData EncodeImage(SKImage image, ImageFormat format, int quality)
    {
        var encodedFormat = format switch
        {
            ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            ImageFormat.Png => SKEncodedImageFormat.Png,
            ImageFormat.WebP => SKEncodedImageFormat.Webp,
            ImageFormat.Gif => SKEncodedImageFormat.Gif,
            ImageFormat.Bmp => SKEncodedImageFormat.Bmp,
            ImageFormat.Avif => SKEncodedImageFormat.Avif,
            _ => SKEncodedImageFormat.Jpeg,
        };

        var encodedData = image.Encode(encodedFormat, quality);
        if (encodedData == null)
        {
            throw new InvalidOperationException(
                $"Failed to encode image to {format} format. The format may not be supported by the current SkiaSharp build."
            );
        }

        return encodedData;
    }
}
