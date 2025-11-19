// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Provides image processing operations (resize, convert, optimize).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Resizes an image to the specified dimensions.
    /// </summary>
    /// <param name="inputStream">The input image stream.</param>
    /// <param name="width">Target width (optional).</param>
    /// <param name="height">Target height (optional).</param>
    /// <param name="options">Processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed image with metadata.</returns>
    Task<ProcessedImage> ResizeAsync(
        Stream inputStream,
        int? width,
        int? height,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Converts an image to a different format.
    /// </summary>
    /// <param name="inputStream">The input image stream.</param>
    /// <param name="targetFormat">Target image format.</param>
    /// <param name="quality">Quality setting (0-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed image with metadata.</returns>
    Task<ProcessedImage> ConvertFormatAsync(
        Stream inputStream,
        ImageFormat targetFormat,
        int quality,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates multiple image variants (formats and sizes).
    /// </summary>
    /// <param name="inputStream">The input image stream.</param>
    /// <param name="options">Upload options specifying desired variants.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of processed images.</returns>
    Task<List<ProcessedImage>> GenerateVariantsAsync(
        Stream inputStream,
        ImageUploadOptions options,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Represents a processed image with metadata.
/// </summary>
/// <param name="stream">The processed image stream.</param>
/// <param name="width">Image width.</param>
/// <param name="height">Image height.</param>
/// <param name="fileSize">File size in bytes.</param>
/// <param name="format">Image format.</param>
public record ProcessedImage(Stream stream, int width, int height, long fileSize, ImageFormat format);
