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
/// Extension methods for registering MediaMatic archive endpoints.
/// </summary>
public static class ArchiveEndpoints
{
    /// <summary>
    /// Maps all MediaMatic archive endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticArchiveEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // Archive folder endpoint - root filesource
        var folderArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/archive/folders",
            OperationTags.FilesourceUtilities
        );

        folderArchiveGroup
            .MapPost("/{*folderPath}", CreateFolderArchiveAsync)
            .WithName("CreateFolderArchive")
            .WithSummary("Create an archive of a folder")
            .WithDescription(
                "Creates a compressed zip archive of the specified folder and stores it in the __archives folder."
            )
            .Produces<ArchiveResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Archive file list endpoint - root filesource
        var fileArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/archive/files",
            OperationTags.FilesourceUtilities
        );

        fileArchiveGroup
            .MapPost("/", CreateFileListArchiveAsync)
            .WithName("CreateFileListArchive")
            .WithSummary("Create an archive from a list of files")
            .WithDescription(
                "Creates a compressed zip archive from a list of specified files and folders."
            )
            .Produces<ArchiveResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // List archives endpoint - root filesource
        var listArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/archives/folders",
            OperationTags.FilesourceUtilities
        );

        listArchiveGroup
            .MapGet("/{*folderPath}", ListArchivesAsync)
            .WithName("ListArchives")
            .WithSummary("List all archives in a folder")
            .WithDescription("Returns a list of all archives stored in the __archives subfolder.")
            .Produces<ArchiveListResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Download archive endpoint - root filesource
        var downloadArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/archives",
            OperationTags.FilesourceUtilities
        );

        downloadArchiveGroup
            .MapGet("/{archiveId}", DownloadArchiveAsync)
            .WithName("DownloadArchive")
            .WithSummary("Download a specific archive")
            .WithDescription("Downloads the specified archive file.")
            .Produces((int)HttpStatusCode.OK, contentType: "application/zip")
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Delete archive endpoint - root filesource
        downloadArchiveGroup
            .MapDelete("/{archiveId}", DeleteArchiveAsync)
            .WithName("DeleteArchive")
            .WithSummary("Delete a specific archive")
            .WithDescription("Deletes the specified archive file.")
            .Produces((int)HttpStatusCode.NoContent)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Bucket variants
        RegisterBucketArchiveEndpoints(app, basePath);

        return app;
    }

    private static void RegisterBucketArchiveEndpoints(IEndpointRouteBuilder app, string? basePath)
    {
        // Archive folder endpoint - bucket
        var bucketFolderArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/archive/folders",
            OperationTags.FilesourceUtilities
        );

        bucketFolderArchiveGroup
            .MapPost("/{*folderPath}", CreateFolderArchiveFromBucketAsync)
            .WithName("CreateBucketFolderArchive")
            .WithSummary("Create an archive of a folder in a bucket")
            .WithDescription("Creates a compressed archive of the specified folder in a storage bucket.")
            .Produces<ArchiveResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Archive file list endpoint - bucket
        var bucketFileArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/archive/files",
            OperationTags.FilesourceUtilities
        );

        bucketFileArchiveGroup
            .MapPost("/", CreateFileListArchiveFromBucketAsync)
            .WithName("CreateBucketFileListArchive")
            .WithSummary("Create an archive from a list of files in a bucket")
            .WithDescription("Creates a compressed archive from files and folders in a storage bucket.")
            .Produces<ArchiveResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // List archives endpoint - bucket
        var bucketListArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/archives/folders",
            OperationTags.FilesourceUtilities
        );

        bucketListArchiveGroup
            .MapGet("/{*folderPath}", ListArchivesFromBucketAsync)
            .WithName("ListBucketArchives")
            .WithSummary("List all archives in a bucket folder")
            .WithDescription("Returns archives stored in the __archives subfolder within a bucket.")
            .Produces<ArchiveListResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Download archive endpoint - bucket
        var downloadBucketArchiveGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/archives",
            OperationTags.FilesourceUtilities
        );

        downloadBucketArchiveGroup
            .MapGet("/{archiveId}", DownloadArchiveFromBucketAsync)
            .WithName("DownloadBucketArchive")
            .WithSummary("Download a specific archive from a bucket")
            .WithDescription("Downloads an archive file from a storage bucket.")
            .Produces((int)HttpStatusCode.OK, contentType: "application/zip")
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Delete archive endpoint - bucket
        downloadBucketArchiveGroup
            .MapDelete("/{archiveId}", DeleteArchiveFromBucketAsync)
            .WithName("DeleteBucketArchive")
            .WithSummary("Delete a specific archive from a bucket")
            .WithDescription("Deletes an archive file from a storage bucket.")
            .Produces((int)HttpStatusCode.NoContent)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);
    }

    // Root filesource implementations
    private static Task<IResult> CreateFolderArchiveAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        [FromBody] ArchiveRequest? request,
        CancellationToken cancellationToken = default
    ) =>
        CreateFolderArchiveInternalAsync(
            operationContext,
            service,
            filesourceId,
            null,
            folderPath,
            request,
            cancellationToken
        );

    private static Task<IResult> CreateFileListArchiveAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromBody] ArchiveRequest request,
        CancellationToken cancellationToken = default
    ) => CreateFileListArchiveInternalAsync(operationContext, service, filesourceId, null, request, cancellationToken);

    private static Task<IResult> ListArchivesAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => ListArchivesInternalAsync(operationContext, service, filesourceId, null, folderPath, cancellationToken);

    private static Task<IResult> DownloadArchiveAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string archiveId,
        CancellationToken cancellationToken = default
    ) => DownloadArchiveInternalAsync(operationContext, service, filesourceId, null, archiveId, cancellationToken);

    private static Task<IResult> DeleteArchiveAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string archiveId,
        CancellationToken cancellationToken = default
    ) => DeleteArchiveInternalAsync(operationContext, service, filesourceId, null, archiveId, cancellationToken);

    // Bucket implementations
    private static Task<IResult> CreateFolderArchiveFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        [FromBody] ArchiveRequest? request,
        CancellationToken cancellationToken = default
    ) =>
        CreateFolderArchiveInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            folderPath,
            request,
            cancellationToken
        );

    private static Task<IResult> CreateFileListArchiveFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromBody] ArchiveRequest request,
        CancellationToken cancellationToken = default
    ) =>
        CreateFileListArchiveInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            request,
            cancellationToken
        );

    private static Task<IResult> ListArchivesFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string folderPath,
        CancellationToken cancellationToken = default
    ) => ListArchivesInternalAsync(operationContext, service, filesourceId, bucketName, folderPath, cancellationToken);

    private static Task<IResult> DownloadArchiveFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string archiveId,
        CancellationToken cancellationToken = default
    ) =>
        DownloadArchiveInternalAsync(operationContext, service, filesourceId, bucketName, archiveId, cancellationToken);

    private static Task<IResult> DeleteArchiveFromBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string archiveId,
        CancellationToken cancellationToken = default
    ) => DeleteArchiveInternalAsync(operationContext, service, filesourceId, bucketName, archiveId, cancellationToken);

    // Internal implementations
    private static async Task<IResult> CreateFolderArchiveInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        ArchiveRequest? request,
        CancellationToken cancellationToken
    )
    {
        request ??= new ArchiveRequest();

        var response = await service
            .CreateFolderArchiveAsync(
                operationContext,
                filesourceId,
                bucketName,
                folderPath,
                request,
                cancellationToken
            )
            .ConfigureAwait(false);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateFileListArchiveInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        ArchiveRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.Paths == null || request.Paths.Count == 0)
        {
            return Results.BadRequest(new { error = "Paths array is required and cannot be empty" });
        }

        var response = await service
            .CreateFileListArchiveAsync(operationContext, filesourceId, bucketName, request, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(response);
    }

    private static async Task<IResult> ListArchivesInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string folderPath,
        CancellationToken cancellationToken
    )
    {
        var archives = await service
            .ListArchivesAsync(operationContext, filesourceId, bucketName, folderPath, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ArchiveListResponse(archives));
    }

    private static async Task<IResult> DownloadArchiveInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken
    )
    {
        var archiveStream = await service
            .DownloadArchiveAsync(operationContext, filesourceId, bucketName, archiveId, cancellationToken)
            .ConfigureAwait(false);

        var contentType =
            archiveId.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ? "application/gzip"
            : archiveId.EndsWith(".tar", StringComparison.OrdinalIgnoreCase) ? "application/x-tar"
            : "application/zip";

        return Results.Stream(archiveStream, contentType: contentType, fileDownloadName: archiveId);
    }

    private static async Task<IResult> DeleteArchiveInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string archiveId,
        CancellationToken cancellationToken
    )
    {
        await service
            .DeleteArchiveAsync(operationContext, filesourceId, bucketName, archiveId, cancellationToken)
            .ConfigureAwait(false);

        return Results.NoContent();
    }
}
