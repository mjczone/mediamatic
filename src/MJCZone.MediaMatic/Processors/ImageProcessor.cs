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

        // Calculate target dimensions maintaining aspect ratio
        var (targetWidth, targetHeight) = CalculateTargetDimensions(
            inputBitmap.Width,
            inputBitmap.Height,
            width,
            height
        );

        // Create resized image with high-quality sampling (SKFilterQuality.High deprecated, use SKSamplingOptions)
        var imageInfo = new SKImageInfo(targetWidth, targetHeight);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
        using var resizedBitmap = inputBitmap.Resize(imageInfo, samplingOptions);

        if (resizedBitmap == null)
        {
            throw new InvalidOperationException("Failed to resize image");
        }

        // Encode to output format
        var format = options.Format ?? ImageFormat.Jpeg;
        var outputStream = new MemoryStream();
        using var image = SKImage.FromBitmap(resizedBitmap);
        using var encodedData = EncodeImage(image, format, options.Quality);
        encodedData.SaveTo(outputStream);

        outputStream.Position = 0;
        return Task.FromResult(new ProcessedImage(
            outputStream,
            targetWidth,
            targetHeight,
            outputStream.Length,
            format
        ));
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
        return Task.FromResult(new ProcessedImage(
            outputStream,
            inputBitmap.Width,
            inputBitmap.Height,
            outputStream.Length,
            targetFormat
        ));
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

    private static (int width, int height) CalculateTargetDimensions(
        int originalWidth,
        int originalHeight,
        int? targetWidth,
        int? targetHeight
    )
    {
        if (targetWidth.HasValue && targetHeight.HasValue)
        {
            return (targetWidth.Value, targetHeight.Value);
        }

        if (targetWidth.HasValue)
        {
            var aspectRatio = (double)originalHeight / originalWidth;
            return (targetWidth.Value, (int)(targetWidth.Value * aspectRatio));
        }

        if (targetHeight.HasValue)
        {
            var aspectRatio = (double)originalWidth / originalHeight;
            return ((int)(targetHeight.Value * aspectRatio), targetHeight.Value);
        }

        return (originalWidth, originalHeight);
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
            throw new InvalidOperationException($"Failed to encode image to {format} format. The format may not be supported by the current SkiaSharp build.");
        }

        return encodedData;
    }
}
