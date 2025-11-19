// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

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
        // TODO: Implement using SkiaSharp 3.119.1 API (use SKSamplingOptions instead of deprecated SKFilterQuality)
        throw new NotImplementedException("Image resize not yet implemented");
    }

    /// <inheritdoc/>
    public Task<ProcessedImage> ConvertFormatAsync(
        Stream inputStream,
        ImageFormat targetFormat,
        int quality,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using SkiaSharp 3.119.1 API
        throw new NotImplementedException("Image format conversion not yet implemented");
    }

    /// <inheritdoc/>
    public Task<List<ProcessedImage>> GenerateVariantsAsync(
        Stream inputStream,
        ImageUploadOptions options,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using SkiaSharp 3.119.1 API
        throw new NotImplementedException("Image variant generation not yet implemented");
    }
}
