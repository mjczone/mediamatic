// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.AspNetCore.Transformations;

/// <summary>
/// Options for URL-based image transformations.
/// </summary>
public class TransformationOptions
{
    /// <summary>
    /// Gets or sets the target width in pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the target height in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio (width/height).
    /// Used to calculate missing dimension when only width or height is specified.
    /// </summary>
    public float? AspectRatio { get; set; }

    /// <summary>
    /// Gets or sets the device pixel ratio for high-density displays.
    /// Applied to width and height (e.g., dpr_2 doubles dimensions).
    /// </summary>
    public float? DevicePixelRatio { get; set; }

    /// <summary>
    /// Gets or sets the resize mode (fit, fill, cover, pad, stretch).
    /// </summary>
    public ResizeMode CropMode { get; set; } = ResizeMode.Fit;

    /// <summary>
    /// Gets or sets the gravity mode for cropping.
    /// </summary>
    public GravityMode Gravity { get; set; } = GravityMode.Center;

    /// <summary>
    /// Gets or sets the focal point X coordinate (0.0-1.0).
    /// Used when Gravity is Custom.
    /// </summary>
    public float? FocalPointX { get; set; }

    /// <summary>
    /// Gets or sets the focal point Y coordinate (0.0-1.0).
    /// Used when Gravity is Custom.
    /// </summary>
    public float? FocalPointY { get; set; }

    /// <summary>
    /// Gets or sets the focal point for smart cropping.
    /// Built from FocalPointX and FocalPointY when Gravity is Custom.
    /// </summary>
    public FocalPoint? FocalPoint { get; set; }

    /// <summary>
    /// Gets or sets the quality (0-100). Null means auto-optimize.
    /// </summary>
    public int? Quality { get; set; }

    /// <summary>
    /// Gets or sets the output format (auto, jpeg, png, webp, avif, etc.).
    /// </summary>
    public ImageFormatOption Format { get; set; } = ImageFormatOption.Auto;

    /// <summary>
    /// Gets or sets the background color for padding (hex format).
    /// Default is white (#FFFFFF).
    /// </summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets a value indicating whether to preserve metadata.
    /// </summary>
    public bool PreserveMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets the version identifier for cache busting.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Converts transformation options to ImageProcessingOptions for the core library.
    /// </summary>
    /// <param name="resolvedFormat">The resolved image format (if Auto was selected).</param>
    /// <returns>Image processing options.</returns>
    public ImageProcessingOptions ToImageProcessingOptions(ImageFormat? resolvedFormat = null)
    {
        return new ImageProcessingOptions
        {
            Width = Width,
            Height = Height,
            Format = resolvedFormat ?? (Format == ImageFormatOption.Auto ? null : ToImageFormat(Format)),
            Quality = Quality ?? GetDefaultQuality(resolvedFormat),
            ResizeMode = CropMode,
            FocalPoint = FocalPoint,
            PreserveExif = PreserveMetadata,
            StripMetadata = !PreserveMetadata,
            BackgroundColor = BackgroundColor,
            AutoOrient = true,
            Sharpen = false,
        };
    }

    private static ImageFormat ToImageFormat(ImageFormatOption option)
    {
        return option switch
        {
            ImageFormatOption.Jpeg => ImageFormat.Jpeg,
            ImageFormatOption.Png => ImageFormat.Png,
            ImageFormatOption.WebP => ImageFormat.WebP,
            ImageFormatOption.Avif => ImageFormat.Avif,
            ImageFormatOption.Bmp => ImageFormat.Bmp,
            ImageFormatOption.Gif => ImageFormat.Gif,
            _ => ImageFormat.Jpeg,
        };
    }

    private static int GetDefaultQuality(ImageFormat? format)
    {
        return format switch
        {
            ImageFormat.Jpeg => 85,
            ImageFormat.WebP => 90,
            ImageFormat.Avif => 85,
            ImageFormat.Png => 100, // Lossless, quality doesn't apply
            ImageFormat.Bmp => 100, // Uncompressed
            ImageFormat.Gif => 100,
            _ => 85,
        };
    }
}
