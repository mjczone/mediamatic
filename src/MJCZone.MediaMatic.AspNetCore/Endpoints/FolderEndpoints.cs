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
using MJCZone.MediaMatic.AspNetCore.Services;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic folder endpoints.
/// </summary>
public static class FolderEndpoints
{
    /// <summary>
    /// Maps all MediaMatic folder endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticFolderEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // FOLDERS - root filesource
        var folderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/folders",
            OperationTags.FilesourceFolders
        );

        RegisterFolderEndpoints(folderGroup, "Folder", useBucket: false);

        // FOLDERS - with bucket support (S3/Azure)
        var bucketFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/folders",
            OperationTags.FilesourceFolders
        );

        RegisterFolderEndpoints(bucketFolderGroup, "BucketFolder", useBucket: true);

        return app;
    }

    private static void RegisterFolderEndpoints(RouteGroupBuilder group, string namePrefix, bool useBucket)
    {
        var bucketText = useBucket ? " in a bucket" : string.Empty;

        // Create folder
        group
            .MapPost("/{*folderPath}", useBucket ? CreateBucketFolderAsync : CreateFolderAsync)
            .WithName($"Create{namePrefix}")
            .WithSummary($"Create a folder{bucketText}")
            .WithDescription(
                $"Creates the specified folder{bucketText}. Parent folders are created automatically if they don't exist."
            )
            .Produces((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.Conflict)
            .Produces((int)HttpStatusCode.Forbidden);

        // Delete folder
        group
            .MapDelete("/{*folderPath}", useBucket ? DeleteBucketFolderAsync : DeleteFolderAsync)
            .WithName($"Delete{namePrefix}")
            .WithSummary($"Delete a folder{bucketText}")
            .WithDescription($"Deletes the specified folder and all its contents recursively{bucketText}.")
            .Produces((int)HttpStatusCode.NoContent)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);
    }

    // Root filesource implementations
    private static Task<IResult> CreateFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => CreateFolderInternalAsync(operationContext, service, filesourceId, null, folderPath, cancellationToken);

    private static Task<IResult> DeleteFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => DeleteFolderInternalAsync(operationContext, service, filesourceId, null, folderPath, cancellationToken);

    // Bucket implementations
    private static Task<IResult> CreateBucketFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => CreateFolderInternalAsync(operationContext, service, filesourceId, bucketName, folderPath, cancellationToken);

    private static Task<IResult> DeleteBucketFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => DeleteFolderInternalAsync(operationContext, service, filesourceId, bucketName, folderPath, cancellationToken);

    // Internal implementations
    private static async Task<IResult> CreateFolderInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken
    )
    {
        var created = await service
            .CreateFolderAsync(operationContext, filesourceId, bucketName, folderPath, cancellationToken)
            .ConfigureAwait(false);

        if (!created)
        {
            return Results.Conflict(new { error = "Folder already exists", path = folderPath });
        }

        return Results.Created(
            $"{operationContext.EndpointPath?.TrimEnd('/')}/{folderPath}",
            new { path = folderPath }
        );
    }

    private static async Task<IResult> DeleteFolderInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken
    )
    {
        // Check if folder exists before attempting deletion
        var files = await service
            .ListFilesAsync(operationContext, filesourceId, bucketName, folderPath, false, cancellationToken)
            .ConfigureAwait(false);

        var folders = await service
            .ListFoldersAsync(operationContext, filesourceId, bucketName, folderPath, cancellationToken)
            .ConfigureAwait(false);

        // If folder doesn't exist (no files or subfolders), return 404
        if (!files.Any() && !folders.Any())
        {
            return Results.NotFound();
        }

        await service
            .DeleteFolderAsync(operationContext, filesourceId, bucketName, folderPath, cancellationToken)
            .ConfigureAwait(false);

        return Results.NoContent();
    }
}
