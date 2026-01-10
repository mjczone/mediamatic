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
using MJCZone.MediaMatic.AspNetCore.Services;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic file endpoints.
/// </summary>
public static class FileEndpoints
{
    /// <summary>
    /// Maps all MediaMatic file endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/mm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticFileEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        // FILES - root filesource
        var fileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/files",
            OperationTags.FilesourceFiles
        );

        RegisterFileEndpoints(fileGroup, "File", useBucket: false);

        // FILES - with bucket support (S3/Azure)
        var bucketFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/files",
            OperationTags.FilesourceFiles
        );

        RegisterFileEndpoints(bucketFileGroup, "BucketFile", useBucket: true);

        return app;
    }

    private static void RegisterFileEndpoints(RouteGroupBuilder group, string namePrefix, bool useBucket)
    {
        var bucketText = useBucket ? " in a bucket" : string.Empty;

        // Download/view file
        group
            .MapGet("/{*filePath}", useBucket ? DownloadBucketFileAsync : DownloadFileAsync)
            .WithName($"Download{namePrefix}")
            .WithSummary($"Get a file{bucketText}")
            .WithDescription(
                $"Gets the specified file{bucketText} and returns it with the appropriate content type. "
                    + "Use 'download=true' query parameter to force download instead of inline display."
            )
            .Produces((int)HttpStatusCode.OK, contentType: "application/octet-stream")
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Upload file
        group
            .MapPost("/{*filePath}", useBucket ? UploadBucketFileAsync : UploadFileAsync)
            .WithName($"Upload{namePrefix}")
            .WithSummary($"Upload a file{bucketText}")
            .WithDescription(
                $"Uploads a file to the specified path{bucketText}. Use multipart/form-data or send binary data directly."
            )
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.Conflict)
            .Produces((int)HttpStatusCode.Forbidden);

        // Overwrite file
        group
            .MapPut("/{*filePath}", useBucket ? OverwriteBucketFileAsync : OverwriteFileAsync)
            .WithName($"Overwrite{namePrefix}")
            .WithSummary($"Overwrite a file{bucketText}")
            .WithDescription($"Overwrites an existing file{bucketText} or creates it if it doesn't exist.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.Forbidden);

        // Delete file
        group
            .MapDelete("/{*filePath}", useBucket ? DeleteBucketFileAsync : DeleteFileAsync)
            .WithName($"Delete{namePrefix}")
            .WithSummary($"Delete a file{bucketText}")
            .WithDescription($"Deletes the specified file{bucketText}.")
            .Produces((int)HttpStatusCode.NoContent)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Check if file exists
        group
            .MapMethods("/{*filePath}", ["HEAD"], useBucket ? FileExistsInBucketAsync : FileExistsAsync)
            .WithName($"{namePrefix}Exists")
            .WithSummary($"Check if file exists{bucketText}")
            .WithDescription($"Returns 200 if file exists{bucketText}, 404 if not.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);
    }

    // Root filesource implementations
    private static Task<IResult> DownloadFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default
    ) =>
        DownloadFileInternalAsync(operationContext, service, filesourceId, null, filePath, download, cancellationToken);

    private static Task<IResult> UploadFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpRequest request,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) =>
        UploadFileInternalAsync(
            operationContext,
            service,
            request,
            filesourceId,
            null,
            filePath,
            overwrite: false,
            cancellationToken
        );

    private static Task<IResult> OverwriteFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpRequest request,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) =>
        UploadFileInternalAsync(
            operationContext,
            service,
            request,
            filesourceId,
            null,
            filePath,
            overwrite: true,
            cancellationToken
        );

    private static Task<IResult> DeleteFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => DeleteFileInternalAsync(operationContext, service, filesourceId, null, filePath, cancellationToken);

    private static Task<IResult> FileExistsAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => FileExistsInternalAsync(operationContext, service, filesourceId, null, filePath, cancellationToken);

    // Bucket implementations
    private static Task<IResult> DownloadBucketFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default
    ) =>
        DownloadFileInternalAsync(
            operationContext,
            service,
            filesourceId,
            bucketName,
            filePath,
            download,
            cancellationToken
        );

    private static Task<IResult> UploadBucketFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpRequest request,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) =>
        UploadFileInternalAsync(
            operationContext,
            service,
            request,
            filesourceId,
            bucketName,
            filePath,
            overwrite: false,
            cancellationToken
        );

    private static Task<IResult> OverwriteBucketFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpRequest request,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) =>
        UploadFileInternalAsync(
            operationContext,
            service,
            request,
            filesourceId,
            bucketName,
            filePath,
            overwrite: true,
            cancellationToken
        );

    private static Task<IResult> DeleteBucketFileAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => DeleteFileInternalAsync(operationContext, service, filesourceId, bucketName, filePath, cancellationToken);

    private static Task<IResult> FileExistsInBucketAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromRoute] string bucketName,
        [FromRoute] string filePath,
        CancellationToken cancellationToken = default
    ) => FileExistsInternalAsync(operationContext, service, filesourceId, bucketName, filePath, cancellationToken);

    // Internal implementations
    private static async Task<IResult> DownloadFileInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string filePath,
        bool download,
        CancellationToken cancellationToken
    )
    {
        var fileStream = await service
            .DownloadFileAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        var metadata = await service
            .GetFileMetadataAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        var contentType = metadata.MimeType ?? "application/octet-stream";
        var fileName = Path.GetFileName(filePath);

        // Use fileDownloadName only when download=true to force Content-Disposition: attachment
        return Results.Stream(fileStream, contentType: contentType, fileDownloadName: download ? fileName : null);
    }

    private static async Task<IResult> UploadFileInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        HttpRequest request,
        string filesourceId,
        string? bucketName,
        string filePath,
        bool overwrite,
        CancellationToken cancellationToken
    )
    {
        Stream stream;

        // Handle multipart/form-data (file upload from form)
        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
            if (form.Files.Count > 0)
            {
                var file = form.Files[0];
                // Copy form file stream to memory to avoid sync I/O
                var memoryStream = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;
                stream = memoryStream;
            }
            else
            {
                // Copy request body to memory to avoid sync I/O
                var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;
                stream = memoryStream;
            }
        }
        // Handle direct binary upload
        else
        {
            // Copy request body to memory to avoid sync I/O
            var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            stream = memoryStream;
        }

        var uploadedPath = await service
            .UploadFileAsync(operationContext, filesourceId, bucketName, filePath, stream, overwrite, cancellationToken)
            .ConfigureAwait(false);

        var response = new FileUploadResponse(uploadedPath);

        return overwrite
            ? Results.Ok(response)
            : Results.Created($"{operationContext.EndpointPath?.TrimEnd('/')}/{uploadedPath}", response);
    }

    private static async Task<IResult> DeleteFileInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        await service
            .DeleteFileAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        return Results.NoContent();
    }

    private static async Task<IResult> FileExistsInternalAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        string filesourceId,
        string? bucketName,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        var exists = await service
            .FileExistsAsync(operationContext, filesourceId, bucketName, filePath, cancellationToken)
            .ConfigureAwait(false);

        return exists ? Results.Ok() : Results.NotFound();
    }
}
