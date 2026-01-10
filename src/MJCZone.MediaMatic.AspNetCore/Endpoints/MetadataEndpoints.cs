// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MJCZone.MediaMatic.AspNetCore.Extensions;
using MJCZone.MediaMatic.AspNetCore.Models.Responses;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Services;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic metadata endpoints.
/// </summary>
public static class MetadataEndpoints
{
    /// <summary>
    /// Maps all MediaMatic metadata endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticMetadataEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // Register metadata endpoint for root files
        var metadataGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/metadata",
            OperationTags.FilesourceFiles
        );

        metadataGroup
            .MapGet("/{*filePath}", GetFileMetadataAsync)
            .WithName("GetFileMetadata")
            .WithSummary("Get file metadata")
            .WithDescription("Returns metadata for the specified file (MIME type, size, dimensions, EXIF, etc.).")
            .Produces<FileMetadataResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Register metadata endpoint for bucket files
        var bucketMetadataGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/metadata",
            OperationTags.FilesourceFiles
        );

        bucketMetadataGroup
            .MapGet("/{*filePath}", GetBucketFileMetadataAsync)
            .WithName("GetBucketFileMetadata")
            .WithSummary("Get file metadata from a bucket")
            .WithDescription(
                "Returns metadata for the specified file in a storage bucket (MIME type, size, dimensions, EXIF, etc.)."
            )
            .Produces<FileMetadataResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        return app;
    }

    private static Task<IResult> GetFileMetadataAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => GetFileMetadataInternalAsync(operationContext, service, filesourceId, null, filePath, cancellationToken);

    private static Task<IResult> GetBucketFileMetadataAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => GetFileMetadataInternalAsync(operationContext, service, filesourceId, bucketName, filePath, cancellationToken);

    private static async Task<IResult> GetFileMetadataInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        var metadata = await service
            .GetFileMetadataAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new FileMetadataResponse(metadata));
    }
}
