// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MJCZone.MediaMatic.AspNetCore.Auditing;
using MJCZone.MediaMatic.AspNetCore.Validation;

namespace MJCZone.MediaMatic.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring endpoint conventions.
/// </summary>
internal static class EndpointExtensions
{
    /// <summary>
    /// Maps a MediaMatic route group with the specified base path and prefix.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="basePath">The base path for the API endpoints. Defaults to "/api/dm".</param>
    /// <param name="prefix">The prefix for the specific resource (e.g., "datasources", "tables").</param>
    /// <param name="tag">The OpenAPI tag to associate with the group.</param>
    /// <returns>The configured route group.</returns>
    public static RouteGroupBuilder MapMediaMaticEndpointGroup(
        this IEndpointRouteBuilder endpoints,
        string? basePath,
        [StringSyntax("Route")] string prefix,
        string tag
    )
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        basePath ??= "/api/mm";
        return endpoints.MapGroup($"/{basePath.Trim('/')}/{prefix.TrimStart('/')}").WithMediaMaticConventions(tag);
    }

    /// <summary>
    /// Adds MediaMatic conventions to the specified route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <param name="tag">The OpenAPI tag to associate with the group.</param>
    /// <returns>The configured route group.</returns>
    private static RouteGroupBuilder WithMediaMaticConventions(this RouteGroupBuilder group, string tag)
    {
        return group.WithTags(tag).AddEndpointFilter<MediaMaticExceptionFilter>();
    }

    /// <summary>
    /// Exception filter to handle common exceptions and map them to appropriate HTTP results.
    /// </summary>
    public sealed class MediaMaticExceptionFilter : IEndpointFilter
    {
        /// <inheritdoc/>
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            try
            {
                return await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var operationContext = context.HttpContext.RequestServices.GetService<IOperationContext>();
                var auditLogger = context.HttpContext.RequestServices.GetService<IMediaMaticAuditLogger>();
                if (auditLogger != null && operationContext != null)
                {
                    await auditLogger
                        .LogOperationAsync(operationContext.ToAuditEvent(success: false, message: ex.Message))
                        .ConfigureAwait(false);
                }
                return ErrorHandler.HandleError(ex);
            }
        }
    }
}
