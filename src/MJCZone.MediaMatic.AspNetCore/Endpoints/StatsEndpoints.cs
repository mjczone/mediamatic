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
using MJCZone.MediaMatic.AspNetCore.Models;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Services;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic statistics endpoints.
/// </summary>
public static class StatsEndpoints
{
    /// <summary>
    /// Maps all MediaMatic statistics endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticStatsEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // Folder stats endpoint - root filesource
        var folderStatsGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/stats/fo",
            OperationTags.FilesourceUtilities
        );

        folderStatsGroup
            .MapGet("/{*folderPath}", GetFolderStatsAsync)
            .WithName("GetFolderStats")
            .WithSummary("Get folder statistics")
            .WithDescription(
                "Returns statistics for a folder including total size, file count, folder count, and breakdown by MIME type. "
                    + "Use ?recursive=true to include subdirectories."
            )
            .Produces<FolderStatsResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Filesource stats endpoint - root filesource
        var filesourceStatsGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}",
            OperationTags.FilesourceUtilities
        );

        filesourceStatsGroup
            .MapGet("/stats", GetFilesourceStatsAsync)
            .WithName("GetFilesourceStats")
            .WithSummary("Get filesource statistics")
            .WithDescription(
                "Returns  statistics for the entire filesource including total size, file count, "
                    + "top folders by size, and breakdown by MIME type."
            )
            .Produces<FilesourceStatsResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Bucket variants
        RegisterBucketStatsEndpoints(app, basePath);

        return app;
    }

    private static void RegisterBucketStatsEndpoints(IEndpointRouteBuilder app, string? basePath)
    {
        // Folder stats endpoint - bucket
        var bucketFolderStatsGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/stats/fo",
            OperationTags.FilesourceUtilities
        );

        bucketFolderStatsGroup
            .MapGet("/{*folderPath}", GetFolderStatsFromBucketAsync)
            .WithName("GetBucketFolderStats")
            .WithSummary("Get folder statistics from a bucket")
            .WithDescription(
                "Returns statistics for a folder in a storage bucket including total size, file count, and breakdown by MIME type."
            )
            .Produces<FolderStatsResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Bucket stats endpoint
        var bucketStatsGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}",
            OperationTags.FilesourceUtilities
        );

        bucketStatsGroup
            .MapGet("/stats", GetBucketStatsAsync)
            .WithName("GetBucketStats")
            .WithSummary("Get bucket statistics")
            .WithDescription(
                "Returns  statistics for an entire storage bucket including total size, file count, "
                    + "top folders by size, and breakdown by MIME type."
            )
            .Produces<FilesourceStatsResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);
    }

    // Root filesource implementations
    private static Task<IResult> GetFolderStatsAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        [FromQuery] bool recursive = false,
        CancellationToken cancellationToken = default
    ) =>
        GetFolderStatsInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            folderPath,
            recursive,
            cancellationToken
        );

    private static Task<IResult> GetFilesourceStatsAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        CancellationToken cancellationToken = default
    ) => GetFilesourceStatsInternalAsync(operationContext, service, filesourceId, null, cancellationToken);

    // Bucket implementations
    private static Task<IResult> GetFolderStatsFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        [FromQuery] bool recursive = false,
        CancellationToken cancellationToken = default
    ) =>
        GetFolderStatsInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            folderPath,
            recursive,
            cancellationToken
        );

    private static Task<IResult> GetBucketStatsAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        CancellationToken cancellationToken = default
    ) => GetFilesourceStatsInternalAsync(operationContext, service, filesourceId, bucketName, cancellationToken);

    // Internal implementations
    private static async Task<IResult> GetFolderStatsInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        bool recursive,
        CancellationToken cancellationToken
    )
    {
        var stats = await service
            .GetFolderStatsAsync(operationContext, filesourceId, bucketName, folderPath, recursive, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetFilesourceStatsInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        CancellationToken cancellationToken
    )
    {
        var stats = await service
            .GetFilesourceStatsAsync(operationContext, filesourceId, bucketName, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(stats);
    }
}
