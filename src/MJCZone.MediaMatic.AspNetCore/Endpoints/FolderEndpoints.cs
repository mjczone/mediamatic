// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MJCZone.MediaMatic.AspNetCore.Extensions;
using MJCZone.MediaMatic.AspNetCore.Security;
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
            "/fs/{filesourceId}/fo",
            OperationTags.FilesourceFolders
        );

        RegisterFolderEndpoints(folderGroup, "Folder", useBucket: false);

        // FOLDERS - with bucket support (S3/Azure)
        var bucketFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/fo",
            OperationTags.FilesourceFolders
        );

        RegisterFolderEndpoints(bucketFolderGroup, "BucketFolder", useBucket: true);

        return app;
    }

    private static void RegisterFolderEndpoints(RouteGroupBuilder group, string namePrefix, bool useBucket)
    {
        var bucketText = useBucket ? " in a bucket" : string.Empty;

        // List folders at root
        group
            .MapGet("/", useBucket ? ListBucketFoldersAsync : ListFoldersAsync)
            .WithName($"List{namePrefix}s")
            .WithSummary($"List folders at root{bucketText}")
            .WithDescription($"Returns a list of all folders in the root directory{bucketText}.")
            .Produces<FolderListResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // List files in folder (use ?type=files query parameter)
        group
            .MapGet("/{*folderPath}", useBucket ? ListInBucketFolderAsync : ListInFolderAsync)
            .WithName($"List{namePrefix}Contents")
            .WithSummary($"List contents in a folder{bucketText}")
            .WithDescription($"Returns a list of files or folders in the specified folder{bucketText}.")
            .WithOpenApi(operation =>
            {
                var typeParam = operation.Parameters?.FirstOrDefault(p => p.Name == "type");
                if (typeParam != null)
                {
                    typeParam.Description =
                        $"Content type to list. Valid values: '{FolderContentTypes.Files}' (default), '{FolderContentTypes.Folders}'";
                    typeParam.Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString(FolderContentTypes.Files),
                            new OpenApiString(FolderContentTypes.Folders),
                        },
                        Default = new OpenApiString(FolderContentTypes.Files),
                    };
                }

                return operation;
            })
            .Produces<FileListResponse>((int)HttpStatusCode.OK)
            .Produces<FolderListResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
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
    private static Task<IResult> ListFoldersAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromQuery] string? path = null,
        CancellationToken cancellationToken = default
    ) => ListFoldersInternalAsync(operationContext, service, filesourceId, null, path, cancellationToken);

    private static Task<IResult> ListInFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        [FromQuery] string? type = null,
        [FromQuery] bool recursive = false,
        CancellationToken cancellationToken = default
    )
    {
        // Normalize type to lowercase for case-insensitive comparison
        var normalizedType = type?.ToLowerInvariant();

        // Validate type parameter
        if (
            normalizedType != null
            && normalizedType != FolderContentTypes.Files
            && normalizedType != FolderContentTypes.Folders
        )
        {
            return Task.FromResult(
                Results.BadRequest(
                    new
                    {
                        error = "Invalid type parameter",
                        details = $"Valid values are '{FolderContentTypes.Files}' or '{FolderContentTypes.Folders}'",
                    }
                )
            );
        }

        // If type is "folders", list folders; otherwise list files (default)
        if (normalizedType == FolderContentTypes.Folders)
        {
            return ListFoldersInternalAsync(
                operationContext,
                service,
                filesourceId,
                null,
                folderPath,
                cancellationToken
            );
        }

        return ListFilesInFolderInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            folderPath,
            recursive,
            cancellationToken
        );
    }

    private static Task<IResult> ListSubfoldersAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => ListFoldersInternalAsync(operationContext, service, filesourceId, null, folderPath, cancellationToken);

    private static Task<IResult> DeleteFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => DeleteFolderInternalAsync(operationContext, service, filesourceId, null, folderPath, cancellationToken);

    // Bucket implementations
    private static Task<IResult> ListBucketFoldersAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromQuery] string? path = null,
        CancellationToken cancellationToken = default
    ) => ListFoldersInternalAsync(operationContext, service, filesourceId, bucketName, path, cancellationToken);

    private static Task<IResult> ListInBucketFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        [FromQuery] string? type = null,
        [FromQuery] bool recursive = false,
        CancellationToken cancellationToken = default
    )
    {
        // Normalize type to lowercase for case-insensitive comparison
        var normalizedType = type?.ToLowerInvariant();

        // Validate type parameter
        if (
            normalizedType != null
            && normalizedType != FolderContentTypes.Files
            && normalizedType != FolderContentTypes.Folders
        )
        {
            return Task.FromResult(
                Results.BadRequest(
                    new
                    {
                        error = "Invalid type parameter",
                        details = $"Valid values are '{FolderContentTypes.Files}' or '{FolderContentTypes.Folders}'",
                    }
                )
            );
        }

        // If type is "folders", list folders; otherwise list files (default)
        if (normalizedType == FolderContentTypes.Folders)
        {
            return ListFoldersInternalAsync(
                operationContext,
                service,
                filesourceId,
                bucketName,
                folderPath,
                cancellationToken
            );
        }

        return ListFilesInFolderInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            folderPath,
            recursive,
            cancellationToken
        );
    }

    private static Task<IResult> ListBucketSubfoldersAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => ListFoldersInternalAsync(operationContext, service, filesourceId, bucketName, folderPath, cancellationToken);

    private static Task<IResult> DeleteBucketFolderAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => DeleteFolderInternalAsync(operationContext, service, filesourceId, bucketName, folderPath, cancellationToken);

    // Internal implementations
    private static async Task<IResult> ListFoldersInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string? path,
        CancellationToken cancellationToken
    )
    {
        var folders = await service
            .ListFoldersAsync(operationContext, filesourceId, bucketName, path, cancellationToken)
            .ConfigureAwait(false);

        // If a specific folder path was requested (not root) and no subfolders found,
        // check if any files exist in that folder. If nothing exists, return 404.
        if (!string.IsNullOrEmpty(path) && !folders.Any())
        {
            var files = await service
                .ListFilesAsync(operationContext, filesourceId, bucketName, path, false, cancellationToken)
                .ConfigureAwait(false);

            if (!files.Any())
            {
                return Results.NotFound();
            }
        }

        return Results.Ok(new FolderListResponse(folders));
    }

    private static async Task<IResult> ListFilesInFolderInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        bool recursive,
        CancellationToken cancellationToken
    )
    {
        var files = await service
            .ListFilesAsync(operationContext, filesourceId, bucketName, folderPath, recursive, cancellationToken)
            .ConfigureAwait(false);

        // If a specific folder path was requested and no files found,
        // check if any subfolders exist. If nothing exists, return 404.
        if (!string.IsNullOrEmpty(folderPath) && !files.Any())
        {
            var folders = await service
                .ListFoldersAsync(operationContext, filesourceId, bucketName, folderPath, cancellationToken)
                .ConfigureAwait(false);

            if (!folders.Any())
            {
                return Results.NotFound();
            }
        }

        return Results.Ok(new FileListResponse(files));
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

/// <summary>
/// Response containing a list of folder paths.
/// </summary>
public class FolderListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderListResponse"/> class.
    /// </summary>
    public FolderListResponse()
    {
        Folders = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderListResponse"/> class.
    /// </summary>
    /// <param name="folders">The list of folder paths.</param>
    public FolderListResponse(IEnumerable<string> folders)
    {
        Folders = folders;
    }

    /// <summary>
    /// Gets or sets the list of folder paths.
    /// </summary>
    public IEnumerable<string> Folders { get; set; }
}
