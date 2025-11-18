// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for uploading and processing images.
/// </summary>
public class ImageUploadOptions
{
    /// <summary>
    /// Gets or sets the target path for the uploaded image.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate additional format variants (WebP, AVIF).
    /// Default is true.
    /// </summary>
    public bool GenerateFormats { get; set; } = true;

    /// <summary>
    /// Gets or sets the specific formats to generate (if null, generates WebP and AVIF if supported).
    /// </summary>
    public List<ImageFormat>? Formats { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate responsive image thumbnails.
    /// Default is true.
    /// </summary>
    public bool GenerateThumbnails { get; set; } = true;

    /// <summary>
    /// Gets or sets the thumbnail sizes to generate (width in pixels).
    /// Default is [320, 640, 1024, 1920].
    /// </summary>
    public List<int>? ThumbnailSizes { get; set; } = [320, 640, 1024, 1920];

    /// <summary>
    /// Gets or sets the maximum width for the original image (will resize if larger).
    /// Null means no maximum.
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height for the original image (will resize if larger).
    /// Null means no maximum.
    /// </summary>
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets the JPEG quality (1-100). Default is 85.
    /// </summary>
    public int JpegQuality { get; set; } = 85;

    /// <summary>
    /// Gets or sets the WebP quality (1-100). Default is 80.
    /// </summary>
    public int WebPQuality { get; set; } = 80;

    /// <summary>
    /// Gets or sets the AVIF quality (1-100). Default is 75.
    /// </summary>
    public int AvifQuality { get; set; } = 75;

    /// <summary>
    /// Gets or sets a value indicating whether to preserve EXIF metadata.
    /// Default is true.
    /// </summary>
    public bool PreserveExif { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-orient images based on EXIF orientation.
    /// Default is true.
    /// </summary>
    public bool AutoOrient { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to strip all metadata (overrides PreserveExif).
    /// Default is false.
    /// </summary>
    public bool StripMetadata { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing files.
    /// Default is false.
    /// </summary>
    public bool Overwrite { get; set; }
}
