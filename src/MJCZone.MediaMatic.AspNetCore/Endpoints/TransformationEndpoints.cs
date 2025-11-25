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
                    + "Results are cached for performance."
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
                    + "Same transformation parameters as the root endpoint."
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

        return app;
    }

    private static Task<IResult> TransformImageAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpContext httpContext,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        [FromRoute] string transformations,
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
            cancellationToken
        );

    private static async Task<IResult> TransformImageInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpContext httpContext,
        string filesourceId,
        string? bucketName,
        string filePath,
        string transformations,
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

        // Set cache headers
        httpContext.Response.Headers.ETag = etag;
        httpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

        if (fileMetadata.ModifiedAt != default)
        {
            httpContext.Response.Headers.LastModified = fileMetadata.ModifiedAt.ToString("R");
        }

        // Set content type based on resolved format
        var contentType = GetContentType(resolvedFormat.Value);

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
}
