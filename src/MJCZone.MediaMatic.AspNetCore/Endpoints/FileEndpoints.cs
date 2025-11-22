// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Routing;

using MJCZone.MediaMatic.AspNetCore.Extensions;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic filesource file endpoints.
/// </summary>
public static class FileEndpoints
{
    /// <summary>
    /// Maps all MediaMatic filesource endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/dm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticFileEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        var rootFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/fi",
            OperationTags.FilesourceFiles
        );

        var subFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/fip/{*filePath}",
            OperationTags.FilesourceFiles
        );

        var bucketFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/fi",
            OperationTags.FilesourceFiles
        );

        var bucketSubFileGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/fip/{*filePath}",
            OperationTags.FilesourceFiles
        );

        return app;
    }
}
