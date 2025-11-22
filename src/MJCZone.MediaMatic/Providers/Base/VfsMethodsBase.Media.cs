// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Partial class containing stream-based media processing operations.
/// These operations work for ALL VFS providers (Local, S3, GCP, SFTP, etc.).
/// </summary>
public abstract partial class VfsMethodsBase
{
    #region Image processing methods

    /// <inheritdoc/>
    public virtual async Task<ImageUploadResult> UploadImageAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        ImageUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        options ??= new ImageUploadOptions();

        try
        {
            // Copy stream to MemoryStream to allow multiple reads
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;

            // Detect MIME type
            var mimeType = await MimeDetector
                .DetectMimeTypeAsync(memoryStream, cancellationToken)
                .ConfigureAwait(false);
            memoryStream.Position = 0;

            // Extract metadata
            var metadata = await MetadataReader
                .ExtractImageMetadataAsync(memoryStream, cancellationToken)
                .ConfigureAwait(false);
            memoryStream.Position = 0;

            // Determine original image format
            var originalFormat = DetermineImageFormat(mimeType);

            // Process original image (resize if MaxWidth/MaxHeight specified)
            ProcessedImage originalImage;
            if (options.MaxWidth.HasValue || options.MaxHeight.HasValue)
            {
                memoryStream.Position = 0;
                originalImage = await ImageProcessor
                    .ResizeAsync(
                        memoryStream,
                        options.MaxWidth,
                        options.MaxHeight,
                        new ImageProcessingOptions { Format = originalFormat, Quality = options.JpegQuality },
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                // Upload resized image (which has its own stream)
                originalImage.stream.Position = 0;
                await UploadFileAsync(vfs, originalImage.stream, path, options.Overwrite, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                // No resizing needed - use memoryStream directly for all operations
                // Upload a COPY so memoryStream stays available for variants
                memoryStream.Position = 0;
                var uploadCopy = new MemoryStream();
                await memoryStream.CopyToAsync(uploadCopy, cancellationToken).ConfigureAwait(false);
                uploadCopy.Position = 0;
                await UploadFileAsync(vfs, uploadCopy, path, options.Overwrite, cancellationToken)
                    .ConfigureAwait(false);
                await uploadCopy.DisposeAsync().ConfigureAwait(false);

                // Create ProcessedImage record pointing to memoryStream (which we keep for further processing)
                originalImage = new ProcessedImage(
                    memoryStream,
                    metadata.Width ?? 0,
                    metadata.Height ?? 0,
                    memoryStream.Length,
                    originalFormat
                );
            }

            var result = new ImageUploadResult
            {
                Path = path,
                Width = originalImage.width,
                Height = originalImage.height,
                FileSize = originalImage.fileSize,
                MimeType = mimeType,
                Format = originalImage.format,
                Metadata = metadata,
                Success = true,
            };

            // Generate format variants (WebP, AVIF) if requested
            if (options.GenerateFormats)
            {
                var formats = options.Formats ?? [ImageFormat.WebP, ImageFormat.Avif];
                var baseDir = Path.GetDirectoryName(path) ?? string.Empty;
                var baseName = Path.GetFileNameWithoutExtension(path);

                foreach (var format in formats)
                {
                    if (format == originalFormat)
                    {
                        continue; // Skip if same as original
                    }

                    try
                    {
                        var quality = format switch
                        {
                            ImageFormat.WebP => options.WebPQuality,
                            ImageFormat.Avif => options.AvifQuality,
                            ImageFormat.Jpeg => options.JpegQuality,
                            _ => 85,
                        };

                        // Create a copy for format conversion to avoid stream issues
                        var convertStream = new MemoryStream();
                        originalImage.stream.Position = 0;
                        await originalImage.stream.CopyToAsync(convertStream, cancellationToken).ConfigureAwait(false);
                        convertStream.Position = 0;

                        var convertedImage = await ImageProcessor
                            .ConvertFormatAsync(convertStream, format, quality, cancellationToken)
                            .ConfigureAwait(false);

                        await convertStream.DisposeAsync().ConfigureAwait(false);

                        try
                        {
                            var extension = GetImageExtension(format);
                            var variantPath = Path.Combine(baseDir, $"{baseName}{extension}");

                            // Create a copy for upload (FluentStorage may dispose the stream)
                            convertedImage.stream.Position = 0;
                            var variantUploadCopy = new MemoryStream();
                            await convertedImage
                                .stream.CopyToAsync(variantUploadCopy, cancellationToken)
                                .ConfigureAwait(false);
                            variantUploadCopy.Position = 0;

                            await UploadFileAsync(
                                    vfs,
                                    variantUploadCopy,
                                    variantPath,
                                    options.Overwrite,
                                    cancellationToken
                                )
                                .ConfigureAwait(false);
                            await variantUploadCopy.DisposeAsync().ConfigureAwait(false);

                            result.Variants.Add(
                                new ImageVariant
                                {
                                    Path = variantPath,
                                    Format = format,
                                    Width = convertedImage.width,
                                    Height = convertedImage.height,
                                    FileSize = convertedImage.fileSize,
                                    VariantType = "format",
                                }
                            );
                        }
                        finally
                        {
                            await convertedImage.stream.DisposeAsync().ConfigureAwait(false);
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception)
                    {
                        // Skip this format variant if encoding fails (e.g., AVIF not supported on this platform)
                        // The original image upload should still succeed
                        continue;
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            }

            // Generate thumbnail variants if requested
            if (options.GenerateThumbnails && options.ThumbnailSizes != null)
            {
                var baseDir = Path.GetDirectoryName(path) ?? string.Empty;
                var baseName = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);

                foreach (var size in options.ThumbnailSizes)
                {
                    if (size >= originalImage.width)
                    {
                        continue; // Skip if thumbnail is larger than original
                    }

                    // Create a copy for thumbnail generation to avoid stream issues
                    var thumbnailStream = new MemoryStream();
                    originalImage.stream.Position = 0;
                    await originalImage.stream.CopyToAsync(thumbnailStream, cancellationToken).ConfigureAwait(false);
                    thumbnailStream.Position = 0;

                    var thumbnail = await ImageProcessor
                        .ResizeAsync(
                            thumbnailStream,
                            size,
                            null,
                            new ImageProcessingOptions { Format = originalFormat, Quality = options.JpegQuality },
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    await thumbnailStream.DisposeAsync().ConfigureAwait(false);

                    try
                    {
                        var thumbnailPath = Path.Combine(baseDir, $"{baseName}_{size}w{extension}");

                        // Create a copy for upload (FluentStorage may dispose the stream)
                        thumbnail.stream.Position = 0;
                        var thumbnailUploadCopy = new MemoryStream();
                        await thumbnail
                            .stream.CopyToAsync(thumbnailUploadCopy, cancellationToken)
                            .ConfigureAwait(false);
                        thumbnailUploadCopy.Position = 0;

                        await UploadFileAsync(
                                vfs,
                                thumbnailUploadCopy,
                                thumbnailPath,
                                options.Overwrite,
                                cancellationToken
                            )
                            .ConfigureAwait(false);
                        await thumbnailUploadCopy.DisposeAsync().ConfigureAwait(false);

                        result.Variants.Add(
                            new ImageVariant
                            {
                                Path = thumbnailPath,
                                Format = thumbnail.format,
                                Width = thumbnail.width,
                                Height = thumbnail.height,
                                FileSize = thumbnail.fileSize,
                                VariantType = $"thumbnail-{size}w",
                            }
                        );
                    }
                    finally
                    {
                        await thumbnail.stream.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }

            // Dispose original image stream
            if (originalImage.stream != memoryStream)
            {
                await originalImage.stream.DisposeAsync().ConfigureAwait(false);
            }

            await memoryStream.DisposeAsync().ConfigureAwait(false);

            return result;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            return new ImageUploadResult
            {
                Path = path,
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    /// <inheritdoc/>
    public virtual async Task<ImageProcessingResult> ProcessImageAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        options ??= new ImageProcessingOptions();

        var startTime = DateTime.UtcNow;

        try
        {
            // Download source image
            var sourceStream = await DownloadAsync(vfs, sourcePath, cancellationToken).ConfigureAwait(false);

            try
            {
                // Process image (resize or convert format)
                ProcessedImage processedImage;

                if (options.Width.HasValue || options.Height.HasValue)
                {
                    // Resize operation
                    processedImage = await ImageProcessor
                        .ResizeAsync(sourceStream, options.Width, options.Height, options, cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (options.Format.HasValue)
                {
                    // Format conversion
                    processedImage = await ImageProcessor
                        .ConvertFormatAsync(sourceStream, options.Format.Value, options.Quality, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException(
                        "Either dimensions (Width/Height) or Format must be specified for image processing"
                    );
                }

                try
                {
                    // Upload processed image to VFS
                    processedImage.stream.Position = 0; // Reset stream position
                    await UploadFileAsync(vfs, processedImage.stream, destinationPath, true, cancellationToken)
                        .ConfigureAwait(false);

                    var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                    return new ImageProcessingResult
                    {
                        Path = destinationPath,
                        Width = processedImage.width,
                        Height = processedImage.height,
                        FileSize = processedImage.fileSize,
                        Format = processedImage.format,
                        Success = true,
                        ProcessingTimeMs = (long)processingTime,
                    };
                }
                finally
                {
                    await processedImage.stream.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await sourceStream.DisposeAsync().ConfigureAwait(false);
            }
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            return new ImageProcessingResult
            {
                Path = destinationPath,
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    #endregion

    #region Helper methods

    private static ImageFormat DetermineImageFormat(string? mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => ImageFormat.Jpeg,
            "image/png" => ImageFormat.Png,
            "image/webp" => ImageFormat.WebP,
            "image/gif" => ImageFormat.Gif,
            "image/bmp" => ImageFormat.Bmp,
            "image/avif" => ImageFormat.Avif,
            _ => ImageFormat.Jpeg, // Default to JPEG
        };
    }

    private static string GetImageExtension(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            ImageFormat.WebP => ".webp",
            ImageFormat.Gif => ".gif",
            ImageFormat.Bmp => ".bmp",
            ImageFormat.Avif => ".avif",
            _ => ".jpg",
        };
    }

    #endregion

    #region Media processor initialization

    /// <summary>
    /// Initializes media processor instances.
    /// </summary>
    partial void InitializeMediaProcessors()
    {
        _mimeDetector = new MimeTypeDetector();
        _metadataReader = new MetadataReader();
        _imageProcessor = new ImageProcessor();
        _videoProcessor = new VideoProcessor();
    }

    #endregion
}
