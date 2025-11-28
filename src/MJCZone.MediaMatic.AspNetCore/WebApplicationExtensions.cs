// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.MediaMatic.AspNetCore.Endpoints;

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Extension methods for configuring MediaMatic minimal API endpoints.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures MediaMatic middleware and maps API endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configureMiddleware">Optional action to configure custom middleware.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseMediaMatic(
        this IApplicationBuilder app,
        Action<IApplicationBuilder>? configureMiddleware = null
    )
    {
        // Add MediaMatic middleware first
        app.UseMiddleware<MediaMaticMiddleware>();

        // Allow users to add custom middleware
        configureMiddleware?.Invoke(app);

        // Map endpoints
        app.UseEndpoints(endpoints => endpoints.MapMediaMaticEndpoints());

        return app;
    }

    /// <summary>
    /// Maps MediaMatic API endpoints to the specified route builder.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder for method chaining.</returns>
    public static IEndpointRouteBuilder MapMediaMaticEndpoints(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetService<IOptions<MediaMaticOptions>>()?.Value ?? new MediaMaticOptions();
        app.MapMediaMaticFilesourceEndpoints(options.BasePath);
        app.MapMediaMaticBrowseEndpoints(options.BasePath);
        app.MapMediaMaticFolderEndpoints(options.BasePath);
        app.MapMediaMaticFileEndpoints(options.BasePath);
        app.MapMediaMaticTransformationEndpoints(options.BasePath);
        app.MapMediaMaticMetadataEndpoints(options.BasePath);
        app.MapMediaMaticArchiveEndpoints(options.BasePath);
        return app;
    }
}
