// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Options for processing existing images.
/// </summary>
public class ImageProcessingOptions
{
    /// <summary>
    /// Gets or sets the target width (null to maintain aspect ratio).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the target height (null to maintain aspect ratio).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the target format (null to keep original format).
    /// </summary>
    public ImageFormat? Format { get; set; }

    /// <summary>
    /// Gets or sets the quality (1-100). Default is 85.
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Gets or sets the resize mode for fitting content to target dimensions.
    /// Default is Fit (maintain aspect ratio, scale to fit within bounds).
    /// </summary>
    public ResizeMode ResizeMode { get; set; } = ResizeMode.Fit;

    /// <summary>
    /// Gets or sets the focal point for smart cropping (x, y as percentage 0-1).
    /// Null means center crop.
    /// </summary>
    public FocalPoint? FocalPoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to preserve EXIF metadata.
    /// Default is true.
    /// </summary>
    public bool PreserveExif { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-orient based on EXIF.
    /// Default is true.
    /// </summary>
    public bool AutoOrient { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to strip all metadata.
    /// Default is false.
    /// </summary>
    public bool StripMetadata { get; set; }

    /// <summary>
    /// Gets or sets the background color for padding (hex format, e.g., "#FFFFFF").
    /// Default is white.
    /// </summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets a value indicating whether to apply sharpening after resize.
    /// Default is false.
    /// </summary>
    public bool Sharpen { get; set; }
}
