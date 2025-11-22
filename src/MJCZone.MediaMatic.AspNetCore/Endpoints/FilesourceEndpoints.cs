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
using MJCZone.MediaMatic.AspNetCore.Models.Responses;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Services;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic filesource endpoints.
/// </summary>
public static class FilesourceEndpoints
{
    /// <summary>
    /// Maps all MediaMatic filesource endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/dm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticFilesourceEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        var group = app.MapMediaMaticEndpointGroup(basePath, "/fs", OperationTags.Filesources);

        // List all filesources - GET only since no parameters needed
        group
            .MapGet("/", ListFilesourcesAsync)
            .WithName("ListFilesources")
            .WithSummary("Gets all registered filesources")
            .Produces<FilesourceListResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.Forbidden);

        // Get specific filesource - POST with request body
        group
            .MapGet("/{filesourceId}", GetFilesourceAsync)
            .WithName("GetFilesource")
            .WithSummary("Gets a filesource by ID")
            .Produces<FilesourceResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Add new filesource - POST only
        group
            .MapPost("/", CreateFilesourceAsync)
            .WithName("AddFilesource")
            .WithSummary("Adds a new filesource")
            .Produces<FilesourceResponse>((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.Conflict)
            .Produces((int)HttpStatusCode.Forbidden);

        // Update existing filesource - PUT only
        group
            .MapPut("/{filesourceId}", UpdateFilesourceAsync)
            .WithName("UpdateFilesource")
            .WithSummary("Updates an existing filesource")
            .Produces<FilesourceResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Patch existing filesource - PATCH only
        group
            .MapPatch("/{filesourceId}", UpdateFilesourceAsync)
            .WithName("PatchFilesource")
            .WithSummary("Patches an existing filesource")
            .Produces<FilesourceResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Remove filesource - DELETE only
        group
            .MapDelete("/{filesourceId}", DeleteFilesourceAsync)
            .WithName("RemoveFilesource")
            .WithSummary("Removes a filesource")
            .Produces<FilesourceResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        // Test filesource connection - GET
        group
            .MapGet("/{filesourceId}/exists", FilesourceExistsAsync)
            .WithName("FilesourceExists")
            .WithSummary("Checks if a filesource exists")
            .Produces<FilesourceTestResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.Forbidden);

        // Test filesource connection - GET
        group
            .MapGet("/{filesourceId}/test", TestFilesourceAsync)
            .WithName("TestFilesource")
            .WithSummary("Tests filesource connectivity")
            .Produces<FilesourceTestResponse>((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.Forbidden);

        return app;
    }

    private static async Task<IResult> ListFilesourcesAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default
    )
    {
        var filesources = await service.GetFilesourcesAsync(operationContext, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            filesources = filesources.Where(d =>
                (d.Id != null && d.Id.Contains(filter, StringComparison.OrdinalIgnoreCase))
                || (d.DisplayName != null && d.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                || (d.Description != null && d.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                || (d.Provider != null && d.Provider.Contains(filter, StringComparison.OrdinalIgnoreCase))
                || (d.Tags != null && d.Tags.Any(t => t.Contains(filter, StringComparison.OrdinalIgnoreCase)))
            );
        }
        return Results.Ok(new FilesourceListResponse(filesources));
    }

    private static async Task<IResult> GetFilesourceAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        CancellationToken cancellationToken = default
    )
    {
        var filesource = await service
            .GetFilesourceAsync(operationContext, filesourceId, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new FilesourceResponse(filesource));
    }

    private static async Task<IResult> CreateFilesourceAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromBody] FilesourceDto filesource,
        CancellationToken cancellationToken = default
    )
    {
        // API layer validation
        ValidationFactory
            .Object(filesource)
            .MaxLength(r => r.Id, 64, nameof(FilesourceDto.Id), inclusive: true)
            .NotNullOrWhiteSpace(r => r.Provider, nameof(FilesourceDto.Provider))
            .MaxLength(r => r.Provider, 10, nameof(FilesourceDto.Provider), inclusive: true)
            .MinLength(r => r.Provider, 2, nameof(FilesourceDto.Provider), inclusive: true)
            .NotNullOrWhiteSpace(r => r.ConnectionString, nameof(FilesourceDto.ConnectionString))
            .MaxLength(r => r.ConnectionString, 2000, nameof(FilesourceDto.ConnectionString), inclusive: false)
            .NotNullOrWhiteSpace(r => r.DisplayName, nameof(FilesourceDto.DisplayName))
            .MaxLength(r => r.DisplayName, 128, nameof(FilesourceDto.DisplayName), inclusive: true)
            .MaxLength(r => r.Description, 1000, nameof(FilesourceDto.Description), inclusive: true)
            .Assert();

        operationContext.RequestBody = filesource;
        operationContext.FilesourceId = filesource.Id;

        var created = await service
            .AddFilesourceAsync(operationContext, filesource, cancellationToken)
            .ConfigureAwait(false);

        return Results.Created(
            $"{operationContext.EndpointPath?.TrimEnd('/')}/{created.Id}",
            new FilesourceResponse(created)
        );
    }

    private static async Task<IResult> UpdateFilesourceAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        [FromBody] FilesourceDto filesource,
        CancellationToken cancellationToken = default
    )
    {
        filesource.Id = filesourceId;

        // API layer validation
        ValidationFactory
            .Object(filesource)
            .MaxLength(r => r.Provider, 10, nameof(FilesourceDto.Provider), inclusive: true) // pgsql
            .MinLength(r => r.Provider, 2, nameof(FilesourceDto.Provider), inclusive: true) // pg
            .MaxLength(r => r.ConnectionString, 2000, nameof(FilesourceDto.ConnectionString), inclusive: false)
            .MaxLength(r => r.DisplayName, 128, nameof(FilesourceDto.DisplayName), inclusive: true)
            .MaxLength(r => r.Description, 1000, nameof(FilesourceDto.Description), inclusive: true)
            .Assert();

        operationContext.RequestBody = filesource;

        var updated = await service
            .UpdateFilesourceAsync(operationContext, filesource, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new FilesourceResponse(updated));
    }

    private static async Task<IResult> DeleteFilesourceAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        CancellationToken cancellationToken = default
    )
    {
        await service.RemoveFilesourceAsync(operationContext, filesourceId, cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    private static async Task<IResult> FilesourceExistsAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await service
            .FilesourceExistsAsync(operationContext, filesourceId, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new FilesourceExistsResponse(exists));
    }

    private static async Task<IResult> TestFilesourceAsync(
        IOperationContext operationContext,
        IMediaMaticService service,
        [FromRoute] string filesourceId,
        CancellationToken cancellationToken = default
    )
    {
        var filesourceConnectivityTest = await service
            .TestFilesourceAsync(operationContext, filesourceId, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new FilesourceTestResponse(filesourceConnectivityTest));
    }
}
