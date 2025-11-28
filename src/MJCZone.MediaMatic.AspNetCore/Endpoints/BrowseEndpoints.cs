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
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Services;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic browse endpoints.
/// </summary>
public static class BrowseEndpoints
{
    /// <summary>
    /// Maps all MediaMatic browse endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticBrowseEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // BROWSE - root filesource
        var browseGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/browse",
            OperationTags.FilesourceBrowse
        );

        RegisterBrowseEndpoints(browseGroup, "Browse", useBucket: false);

        // BROWSE - with bucket support (S3/Azure)
        var bucketBrowseGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/browse",
            OperationTags.FilesourceBrowse
        );

        RegisterBrowseEndpoints(bucketBrowseGroup, "BucketBrowse", useBucket: true);

        return app;
    }

    private static void RegisterBrowseEndpoints(RouteGroupBuilder group, string namePrefix, bool useBucket)
    {
        var bucketText = useBucket ? " in a bucket" : string.Empty;

        // Browse root
        group
            .MapGet("/", useBucket ? BrowseBucketRootAsync : BrowseRootAsync)
            .WithName($"{namePrefix}Root")
            .WithSummary($"Browse root directory{bucketText}")
            .WithDescription(
                $"Lists files and folders in the root directory{bucketText}. "
                    + "Use 'type' to filter by files/folders/all, 'filter' for wildcard patterns (e.g., '*.pdf'), "
                    + "'recursive' to include subdirectories, and 'fields' to select which properties to return."
            )
            .Produces<BrowseResponseDto>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Browse path
        group
            .MapGet("/{*folderPath}", useBucket ? BrowseBucketPathAsync : BrowsePathAsync)
            .WithName($"{namePrefix}Path")
            .WithSummary($"Browse a directory{bucketText}")
            .WithDescription(
                $"Lists files and folders in the specified directory{bucketText}. "
                    + "Use 'type' to filter by files/folders/all, 'filter' for wildcard patterns (e.g., '*.pdf'), "
                    + "'recursive' to include subdirectories, and 'fields' to select which properties to return."
            )
            .Produces<BrowseResponseDto>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);
    }

    // Root filesource implementations
    private static Task<IResult> BrowseRootAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromQuery] string? type = null,
        [FromQuery] string? filter = null,
        [FromQuery] bool recursive = false,
        [FromQuery] string? fields = null,
        CancellationToken cancellationToken = default
    ) =>
        BrowseInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            null,
            type,
            filter,
            recursive,
            fields,
            cancellationToken
        );

    private static Task<IResult> BrowsePathAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        [FromQuery] string? type = null,
        [FromQuery] string? filter = null,
        [FromQuery] bool recursive = false,
        [FromQuery] string? fields = null,
        CancellationToken cancellationToken = default
    ) =>
        BrowseInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            folderPath,
            type,
            filter,
            recursive,
            fields,
            cancellationToken
        );

    // Bucket implementations
    private static Task<IResult> BrowseBucketRootAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromQuery] string? type = null,
        [FromQuery] string? filter = null,
        [FromQuery] bool recursive = false,
        [FromQuery] string? fields = null,
        CancellationToken cancellationToken = default
    ) =>
        BrowseInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            null,
            type,
            filter,
            recursive,
            fields,
            cancellationToken
        );

    private static Task<IResult> BrowseBucketPathAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        [FromQuery] string? type = null,
        [FromQuery] string? filter = null,
        [FromQuery] bool recursive = false,
        [FromQuery] string? fields = null,
        CancellationToken cancellationToken = default
    ) =>
        BrowseInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            folderPath,
            type,
            filter,
            recursive,
            fields,
            cancellationToken
        );

    // Internal implementation
    private static async Task<IResult> BrowseInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string? path,
        string? type,
        string? filter,
        bool recursive,
        string? fields,
        CancellationToken cancellationToken
    )
    {
        // Parse type parameter
        var browseType = BrowseType.All;
        if (!string.IsNullOrEmpty(type))
        {
            browseType = type.ToLowerInvariant() switch
            {
                "files" => BrowseType.Files,
                "folders" => BrowseType.Folders,
                "all" => BrowseType.All,
                _ => throw new ArgumentException($"Invalid type '{type}'. Valid values are: files, folders, all"),
            };
        }

        // Parse fields parameter
        IEnumerable<string>? fieldList = null;
        if (!string.IsNullOrEmpty(fields))
        {
            fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        var options = new BrowseOptions
        {
            Type = browseType,
            Filter = filter,
            Recursive = recursive,
            Fields = fieldList,
        };

        var result = await service
            .ListAsync(operationContext, filesourceId, bucketName, path, options, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(result);
    }
}
