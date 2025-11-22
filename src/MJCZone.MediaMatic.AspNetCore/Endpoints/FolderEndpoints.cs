// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Routing;

using MJCZone.MediaMatic.AspNetCore.Extensions;

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Extension methods for registering MediaMatic filesource folder endpoints.
/// </summary>
public static class FolderEndpoints
{
    /// <summary>
    /// Maps all MediaMatic filesource endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/dm".</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticFolderEndpoints(
        this IEndpointRouteBuilder app,
        string? basePath = null
    )
    {
        var rootFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/fo",
            OperationTags.FilesourceFolders
        );

        var subFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/fop/{*folderPath}",
            OperationTags.FilesourceFolders
        );

        var bucketFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/fo",
            OperationTags.FilesourceFolders
        );

        var bucketSubFolderGroup = app.MapMediaMaticEndpointGroup(
            basePath,
            "/fs/{filesourceId}/bu/{bucketName}/fop/{*folderPath}",
            OperationTags.FilesourceFolders
        );

        return app;
    }
}
