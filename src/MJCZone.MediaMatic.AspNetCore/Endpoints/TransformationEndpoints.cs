// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MJCZone.MediaMatic.AspNetCore.Extensions;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Services;
using MJCZone.MediaMatic.AspNetCore.Transformations;
using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic image transformation endpoints.
/// </summary>
public static class TransformationEndpoints
{
    /// <summary>
    /// Maps all MediaMatic transformation endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticTransformationEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // Register transformation endpoint for root files
        var fileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/transform",
            OperationTags.FilesourceTransformations
        );

        fileGroup
            .MapGet("/{transformations}/{*filePath}", TransformImageAsync)
            .WithName("TransformImage")
            .WithSummary("Transform an image using URL parameters")
            .WithDescription(
                "Apply transformations to an image (resize, crop, format conversion, quality optimization) using URL parameters. "
                    + "Supports parameters like w_400 (width), h_300 (height), c_fill (crop mode), q_80 (quality), f_auto (format), and more. "
                    + "Use 'download=true' to force download instead of inline display. "
                    + "Use 'saveTo' to cache the transformed result to disk for future requests."
            )
            .Produces((int)HttpStatusCode.OK, contentType: "image/jpeg")
            .Produces((int)HttpStatusCode.OK, contentType: "image/png")
            .Produces((int)HttpStatusCode.OK, contentType: "image/webp")
            .Produces((int)HttpStatusCode.OK, contentType: "image/avif")
            .Produces((int)HttpStatusCode.OK, contentType: "image/gif")
            .Produces((int)HttpStatusCode.OK, contentType: "image/bmp")
            .Produces((int)HttpStatusCode.NotModified)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        fileGroup
            .MapPost("/{transformations}/{*filePath}", GenerateTransformAsync)
            .WithName("GenerateTransform")
            .WithSummary("Generate and save a transformed image")
            .WithDescription(
                "Transform an image and save it to a specified path. Returns metadata about the transformed image instead of the image itself. "
                    + "Ideal for CMS pre-generation workflows where thumbnails are generated ahead of time."
            )
            .Produces<TransformResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        // Register transformation endpoint for bucket files
        var bucketFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/transform",
            OperationTags.FilesourceTransformations
        );

        bucketFileGroup
            .MapGet("/{transformations}/{*filePath}", TransformImageFromBucketAsync)
            .WithName("TransformImageFromBucket")
            .WithSummary("Transform an image from a bucket using URL parameters")
            .WithDescription(
                "Apply transformations to an image in a storage bucket (S3, Azure, etc.). "
                    + "Same transformation parameters as the root endpoint. "
                    + "Use 'download=true' to force download instead of inline display. "
                    + "Use 'saveTo' to cache the transformed result to disk for future requests."
            )
            .Produces((int)HttpStatusCode.OK, contentType: "image/jpeg")
            .Produces((int)HttpStatusCode.OK, contentType: "image/png")
            .Produces((int)HttpStatusCode.OK, contentType: "image/webp")
            .Produces((int)HttpStatusCode.OK, contentType: "image/avif")
            .Produces((int)HttpStatusCode.OK, contentType: "image/gif")
            .Produces((int)HttpStatusCode.OK, contentType: "image/bmp")
            .Produces((int)HttpStatusCode.NotModified)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        bucketFileGroup
            .MapPost("/{transformations}/{*filePath}", GenerateTransformFromBucketAsync)
            .WithName("GenerateTransformFromBucket")
            .WithSummary("Generate and save a transformed image from a bucket")
            .WithDescription(
                "Transform an image from a bucket and save it to a specified path. Returns metadata about the transformed image. "
                    + "Ideal for CMS pre-generation workflows."
            )
            .Produces<TransformResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        // Register batch transformation endpoint
        var batchGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/transform-batch",
            OperationTags.FilesourceTransformations
        );

        batchGroup
            .MapPost("/", GenerateBatchTransformAsync)
            .WithName("GenerateBatchTransform")
            .WithSummary("Generate multiple transformed images in one request")
            .WithDescription(
                "Transform a single source image into multiple variants (e.g., different sizes, formats). "
                    + "Each variant is saved to its specified path. Returns metadata for all generated images. "
                    + "Ideal for generating all thumbnail sizes at once."
            )
            .Produces<TransformBatchResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        // Register batch transformation endpoint for bucket
        var bucketBatchGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/transform-batch",
            OperationTags.FilesourceTransformations
        );

        bucketBatchGroup
            .MapPost("/", GenerateBatchTransformFromBucketAsync)
            .WithName("GenerateBatchTransformFromBucket")
            .WithSummary("Generate multiple transformed images from a bucket in one request")
            .WithDescription(
                "Transform a single source image from a bucket into multiple variants. "
                    + "Each variant is saved to its specified path within the bucket. Returns metadata for all generated images."
            )
            .Produces<TransformBatchResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.Forbidden);

        return app;
    }

    private static Task<IResult> TransformImageAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpContext httpContext,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        [FromRoute] string transformations,
        [FromQuery] bool download = false,
        [FromQuery] string? saveTo = null,
        CancellationToken cancellationToken = default
    ) =>
        TransformImageInternalAsync(
            operationContext,
            service,
            httpContext,
            filesourceId,
            null,
            filePath,
            transformations,
            download,
            saveTo,
            cancellationToken
        );

    private static Task<IResult> TransformImageFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpContext httpContext,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        [FromRoute] string transformations,
        [FromQuery] bool download = false,
        [FromQuery] string? saveTo = null,
        CancellationToken cancellationToken = default
    ) =>
        TransformImageInternalAsync(
            operationContext,
            service,
            httpContext,
            filesourceId,
            bucketName,
            filePath,
            transformations,
            download,
            saveTo,
            cancellationToken
        );

    private static Task<IResult> GenerateTransformAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        [FromRoute] string transformations,
        [FromBody] TransformRequestDto request,
        CancellationToken cancellationToken = default
    ) =>
        GenerateTransformInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            filePath,
            transformations,
            request.SaveTo,
            cancellationToken
        );

    private static Task<IResult> GenerateTransformFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        [FromRoute] string transformations,
        [FromBody] TransformRequestDto request,
        CancellationToken cancellationToken = default
    ) =>
        GenerateTransformInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            filePath,
            transformations,
            request.SaveTo,
            cancellationToken
        );

    private static Task<IResult> GenerateBatchTransformAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromBody] TransformBatchRequestDto request,
        CancellationToken cancellationToken = default
    ) => GenerateBatchTransformInternalAsync(operationContext, service, filesourceId, null, request, cancellationToken);

    private static Task<IResult> GenerateBatchTransformFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromBody] TransformBatchRequestDto request,
        CancellationToken cancellationToken = default
    ) =>
        GenerateBatchTransformInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            request,
            cancellationToken
        );

    private static async Task<IResult> GenerateTransformInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string filePath,
        string transformations,
        string saveTo,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(saveTo))
        {
            return Results.BadRequest(new { error = "saveTo is required for POST transform" });
        }

        // Parse transformation parameters
        TransformationOptions options;
        try
        {
            options = TransformationParser.Parse(transformations);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = "Invalid transformation parameters", details = ex.Message });
        }

        // Resolve format from options (f_auto defaults to WebP for POST since no browser context)
        var resolvedFormat =
            options.Format == ImageFormatOption.Auto ? ImageFormat.WebP : ToImageFormat(options.Format);

        // Apply transformations
        var processingOptions = options.ToImageProcessingOptions(resolvedFormat);

        var transformedStream = await service
            .TransformImageAsync(
                operationContext,
                filesourceId,
                bucketName,
                filePath,
                processingOptions,
                cancellationToken
            )
            .ConfigureAwait(false);

        // Copy stream to memory to get size and save
        using var memoryStream = new MemoryStream();
        await transformedStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;

        // Save to the specified path
        await service
            .UploadFileAsync(
                operationContext,
                filesourceId,
                bucketName,
                saveTo,
                memoryStream,
                overwrite: true,
                cancellationToken
            )
            .ConfigureAwait(false);

        // Get the dimensions from processing options (if specified) or return 0
        var result = new TransformResultDto
        {
            Path = saveTo,
            Size = memoryStream.Length,
            Width = processingOptions.Width ?? 0,
            Height = processingOptions.Height ?? 0,
            Format = GetFormatName(resolvedFormat),
            Success = true,
        };

        return Results.Created($"/{saveTo}", new TransformResponse { Result = result });
    }

    private static async Task<IResult> GenerateBatchTransformInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        TransformBatchRequestDto request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return Results.BadRequest(new { error = "source is required" });
        }

        if (request.Variants == null || request.Variants.Count == 0)
        {
            return Results.BadRequest(new { error = "At least one variant is required" });
        }

        var results = new List<TransformResultDto>();

        foreach (var variant in request.Variants)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(variant.SaveTo))
                {
                    results.Add(
                        new TransformResultDto
                        {
                            Path = variant.SaveTo ?? string.Empty,
                            Success = false,
                            ErrorMessage = "saveTo is required for each variant",
                        }
                    );
                    continue;
                }

                // Parse transformation parameters
                TransformationOptions options;
                try
                {
                    options = TransformationParser.Parse(variant.Transformations);
                }
                catch (ArgumentException ex)
                {
                    results.Add(
                        new TransformResultDto
                        {
                            Path = variant.SaveTo,
                            Success = false,
                            ErrorMessage = $"Invalid transformation parameters: {ex.Message}",
                        }
                    );
                    continue;
                }

                // Resolve format (f_auto defaults to WebP for batch)
                var resolvedFormat =
                    options.Format == ImageFormatOption.Auto ? ImageFormat.WebP : ToImageFormat(options.Format);

                // Apply transformations
                var processingOptions = options.ToImageProcessingOptions(resolvedFormat);

                var transformedStream = await service
                    .TransformImageAsync(
                        operationContext,
                        filesourceId,
                        bucketName,
                        request.Source,
                        processingOptions,
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                // Copy stream to memory to get size and save
                using var memoryStream = new MemoryStream();
                await transformedStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;

                // Save to the specified path
                await service
                    .UploadFileAsync(
                        operationContext,
                        filesourceId,
                        bucketName,
                        variant.SaveTo,
                        memoryStream,
                        overwrite: true,
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                results.Add(
                    new TransformResultDto
                    {
                        Path = variant.SaveTo,
                        Size = memoryStream.Length,
                        Width = processingOptions.Width ?? 0,
                        Height = processingOptions.Height ?? 0,
                        Format = GetFormatName(resolvedFormat),
                        Success = true,
                    }
                );
            }
            catch (Exception ex)
            {
                results.Add(
                    new TransformResultDto
                    {
                        Path = variant.SaveTo ?? string.Empty,
                        Success = false,
                        ErrorMessage = ex.Message,
                    }
                );
            }
        }

        var response = new TransformBatchResponse { Result = results };
        return Results.Created(string.Empty, response);
    }

    private static async Task<IResult> TransformImageInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpContext httpContext,
        string filesourceId,
        string? bucketName,
        string filePath,
        string transformations,
        bool download,
        string? saveTo,
        CancellationToken cancellationToken
    )
    {
        // Parse transformation parameters
        TransformationOptions options;
        try
        {
            options = TransformationParser.Parse(transformations);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = "Invalid transformation parameters", details = ex.Message });
        }

        // Handle f_auto: Select optimal format based on browser capabilities
        ImageFormat? resolvedFormat = null;
        if (options.Format == ImageFormatOption.Auto)
        {
            var acceptHeader = httpContext.Request.Headers.Accept.ToString();
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();

            // For f_auto, we need to know the original format
            // We'll get it from the file metadata
            var metadata = await service
                .GetFileMetadataAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
                .ConfigureAwait(false);

            var originalFormat = DetermineImageFormatFromMimeType(metadata.MimeType);
            resolvedFormat = BrowserFormatSelector.SelectOptimalFormat(userAgent, acceptHeader, originalFormat);
        }
        else
        {
            resolvedFormat = ToImageFormat(options.Format);
        }

        // Generate cache key for this transformation
        var cacheKey = GenerateCacheKey(filesourceId, bucketName, filePath, transformations, resolvedFormat);
        var etag = $"\"{cacheKey}\"";

        // Check if client has cached version (ETag)
        var requestEtag = httpContext.Request.Headers.IfNoneMatch.ToString();
        if (!string.IsNullOrEmpty(requestEtag) && requestEtag == etag)
        {
            return Results.StatusCode((int)HttpStatusCode.NotModified);
        }

        // Get original file metadata for Last-Modified check
        var fileMetadata = await service
            .GetFileMetadataAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        // Check if client's cached version is still valid (Last-Modified)
        var ifModifiedSinceHeader = httpContext.Request.Headers.IfModifiedSince.ToString();
        if (
            !string.IsNullOrEmpty(ifModifiedSinceHeader)
            && DateTime.TryParse(ifModifiedSinceHeader, out var ifModifiedSince)
        )
        {
            if (fileMetadata.ModifiedAt <= ifModifiedSince)
            {
                return Results.StatusCode((int)HttpStatusCode.NotModified);
            }
        }

        // Apply transformations
        var processingOptions = options.ToImageProcessingOptions(resolvedFormat);

        var transformedStream = await service
            .TransformImageAsync(
                operationContext,
                filesourceId,
                bucketName,
                filePath,
                processingOptions,
                cancellationToken
            )
            .ConfigureAwait(false);

        // Save to cache if saveTo is specified
        if (!string.IsNullOrEmpty(saveTo))
        {
            // Copy stream to memory so we can both save and return it
            using var memoryStream = new MemoryStream();
            await transformedStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;

            // Save to the specified path
            await service
                .UploadFileAsync(
                    operationContext,
                    filesourceId,
                    bucketName,
                    saveTo,
                    memoryStream,
                    overwrite: true,
                    cancellationToken
                )
                .ConfigureAwait(false);

            // Reset stream for response
            memoryStream.Position = 0;
            transformedStream = new MemoryStream(memoryStream.ToArray());
        }

        // Set cache headers
        httpContext.Response.Headers.ETag = etag;
        httpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

        if (fileMetadata.ModifiedAt != default)
        {
            httpContext.Response.Headers.LastModified = fileMetadata.ModifiedAt.ToString("R");
        }

        // Set content type based on resolved format
        var contentType = GetContentType(resolvedFormat.Value);

        // Set Content-Disposition for download
        if (download)
        {
            var downloadFileName = !string.IsNullOrEmpty(saveTo)
                ? Path.GetFileName(saveTo)
                : $"{Path.GetFileNameWithoutExtension(filePath)}{GetFileExtension(resolvedFormat.Value)}";
            httpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{downloadFileName}\"";
        }

        // Return transformed image
        return Results.Stream(transformedStream, contentType: contentType);
    }

    private static string GenerateCacheKey(
        string filesourceId,
        string? bucketName,
        string filePath,
        string transformations,
        ImageFormat? format
    )
    {
        var key = $"{filesourceId}/{bucketName ?? "root"}/{filePath}/{transformations}/{format}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hash);
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

    private static ImageFormat DetermineImageFormatFromMimeType(string? mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
        {
            return ImageFormat.Jpeg;
        }

        return mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ImageFormat.Jpeg,
            "image/png" => ImageFormat.Png,
            "image/webp" => ImageFormat.WebP,
            "image/avif" => ImageFormat.Avif,
            "image/bmp" => ImageFormat.Bmp,
            "image/gif" => ImageFormat.Gif,
            "image/tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Jpeg,
        };
    }

    private static string GetContentType(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Png => "image/png",
            ImageFormat.WebP => "image/webp",
            ImageFormat.Avif => "image/avif",
            ImageFormat.Bmp => "image/bmp",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Tiff => "image/tiff",
            _ => "image/jpeg",
        };
    }

    private static string GetFileExtension(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            ImageFormat.WebP => ".webp",
            ImageFormat.Avif => ".avif",
            ImageFormat.Bmp => ".bmp",
            ImageFormat.Gif => ".gif",
            ImageFormat.Tiff => ".tiff",
            _ => ".jpg",
        };
    }

    private static string GetFormatName(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => "jpeg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            ImageFormat.Avif => "avif",
            ImageFormat.Bmp => "bmp",
            ImageFormat.Gif => "gif",
            ImageFormat.Tiff => "tiff",
            _ => "jpeg",
        };
    }
}
