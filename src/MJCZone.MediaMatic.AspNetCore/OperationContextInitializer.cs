// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Default implementation of IOperationContextInitializer that populates basic HTTP context information.
/// </summary>
public partial class OperationContextInitializer : IOperationContextInitializer
{
    /// <summary>
    /// List of HTTP headers to include in the operation context for potential custom initialization.
    /// </summary>
    /// <remarkes>
    /// This list can be modified at application startup to include additional headers as needed.
    /// This list is NOT thread-safe to modify at runtime. It's here for convenience.
    /// </remarkes>
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable SA1401 // Fields should be private
    public static string[] HeadersToInclude =
    [
        HeaderNames.UserAgent,
        HeaderNames.Accept,
        HeaderNames.AcceptEncoding,
        HeaderNames.AcceptLanguage,
        HeaderNames.Cookie,
        HeaderNames.Referer,
        HeaderNames.Authorization,
        HeaderNames.ContentType,
        // Add cloudflare headers
        "True-Client-IP", // the original client IP address (Enterprise customers only)
        "CF-Connecting-IP", // the client IP address connecting to Cloudflare
        "CF-Ray", // hashed value that encodes information about the data center and the visitor's request
        "CF-IPCountry", // two-character country code of the originating visitor's country (see https://www.iso.org/iso-3166-country-codes.html)
    ];
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA2211 // Non-constant fields should not be visible

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationContextInitializer"/> class.
    /// </summary>
    /// <remarks>
    /// This class has no dependencies and can be used as-is or as a base class for custom initializers.
    /// </remarks>
    /// <param name="context">The operation context to initialize.</param>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task InitializeAsync(IOperationContext context, HttpContext httpContext)
    {
        var basePathFromOptions =
            httpContext.RequestServices.GetService<IOptions<MediaMaticOptions>>()?.Value.BasePath?.TrimEnd('/')
            ?? string.Empty;
        var endpoint = httpContext.GetEndpoint() as RouteEndpoint;
        var routePattern = endpoint?.RoutePattern.RawText; // e.g., "/api/mm/f/{filesourceId}"

        string? routePath = null;
        if (routePattern != null)
        {
            // If base path is set, strip it from the route pattern
            if (
                !string.IsNullOrWhiteSpace(basePathFromOptions)
                && routePattern.StartsWith(basePathFromOptions, StringComparison.OrdinalIgnoreCase)
            )
            {
                routePattern = routePattern[basePathFromOptions.Length..];
            }

            // Extract the route path by removing route parameters
            if (string.IsNullOrWhiteSpace(routePattern))
            {
                routePath = string.Empty;
            }
            else
            {
                routePath = RouteValueStripperRegex().Replace(routePattern, string.Empty);
                routePath = DoubleForwardSlashReducerRegex().Replace(routePath, "/").Trim('/');
            }
        }

        // If not a MediaMatic route, skip initialization
        if (routePath == null)
        {
            return Task.CompletedTask;
        }

        // There's always the possibility that the context has already been initialized.
        // Don't overwrite existing values, if a calling code has already set them.
        // This can happen if an initializer implements this one and calls base.InitializeAsync after setting some values.
        // The recommendation for custom initializers is to call this base method first, then set/override any custom values.

        // Populate user information
        context.User ??= httpContext.User;

        // Set request ID for correlation
        context.RequestId ??= httpContext.TraceIdentifier;

        // Extract IP address (handle proxies with X-Forwarded-For)
        context.IpAddress ??= GetClientIpAddress(httpContext);

        // Populate HTTP context information in Properties for potential endpoint inference
        context.HttpMethod ??= httpContext.Request.Method;
        context.EndpointPath ??= httpContext.Request.Path.Value ?? string.Empty;
        context.QueryParameters ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        context.RouteValues ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        context.HeaderValues ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Now populate query parameters and route values
        foreach (var kvp in httpContext.Request.Query)
        {
            // Do not overwrite existing values
            context.QueryParameters.TryAdd(kvp.Key, kvp.Value);
        }

        // Now populate route values
        foreach (var kvp in httpContext.Request.RouteValues)
        {
            var valueStr = kvp.Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                // Do not overwrite existing values
                context.RouteValues.TryAdd(kvp.Key, valueStr);
            }
        }

        // Now populate header values
        if (httpContext.Request.Headers.Count > 0)
        {
            // Only include commonly useful headers to avoid bloating the context
            foreach (var headerName in HeadersToInclude)
            {
                if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValue))
                {
                    context.HeaderValues.TryAdd(headerName, headerValue.ToString());
                }
            }
        }

        // Add route values if available (useful for endpoint inference)
        if (context.RouteValues.Count > 0)
        {
            // If known route values exist, set them in the context
            if (
                string.IsNullOrWhiteSpace(context.FilesourceId)
                && TryGetRouteValue(context.RouteValues, nameof(context.FilesourceId), out var dsIdStr)
            )
            {
                context.FilesourceId = dsIdStr;
            }

            if (
                string.IsNullOrWhiteSpace(context.BucketName)
                && TryGetRouteValue(context.RouteValues, nameof(context.BucketName), out var bucketStr)
            )
            {
                context.BucketName = bucketStr;
            }

            if (
                string.IsNullOrWhiteSpace(context.FolderName)
                && TryGetRouteValue(context.RouteValues, nameof(context.FolderName), out var folderStr)
            )
            {
                context.FolderName = folderStr;
            }

            if (
                string.IsNullOrWhiteSpace(context.OriginalFileName)
                && TryGetRouteValue(context.RouteValues, nameof(context.OriginalFileName), out var originalFileStr)
            )
            {
                context.OriginalFileName = originalFileStr;
            }

            if (
                string.IsNullOrWhiteSpace(context.FileName)
                && TryGetRouteValue(context.RouteValues, nameof(context.FileName), out var fileStr)
            )
            {
                context.FileName = fileStr;
            }
        }

        if (
            string.IsNullOrWhiteSpace(context.FolderPath)
            && TryGetRouteValue(context.RouteValues, nameof(context.FolderPath), out var folderPathStr)
        )
        {
            context.FolderPath = folderPathStr;
        }

        if (
            string.IsNullOrWhiteSpace(context.FilePath)
            && TryGetRouteValue(context.RouteValues, nameof(context.FilePath), out var filePathStr)
        )
        {
            context.FilePath = filePathStr;
        }

        if (
            string.IsNullOrWhiteSpace(context.FileSizeInBytes?.ToString())
            && TryGetRouteValue(context.RouteValues, nameof(context.FileSizeInBytes), out var fileSizeStr)
        )
        {
            if (long.TryParse(fileSizeStr, out var fileSize))
            {
                context.FileSizeInBytes = fileSize;
            }
        }

        if (string.IsNullOrWhiteSpace(context.Operation))
        {
            var resourceType = GetContextResourceType(context);
            var operationVerb = GetContextOperationVerb(context, endpoint, resourceType, httpContext.Request.Method);
            context.Operation ??= $"{resourceType}/{operationVerb}";
        }

        return Task.CompletedTask;
    }

    private static string GetContextResourceType(IOperationContext context)
    {
        if (string.IsNullOrWhiteSpace(context.EndpointPath))
        {
            return "unknown";
        }

        // Split path into segments
        var segments = context.EndpointPath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        var normalizedSegments = new List<string>();
        foreach (var segment in segments)
        {
            // Skip base path segments
            // TODO: REVIEW THESE SEGMENTS AGAINST ACTUAL ROUTE PATTERNS
            var segmentLowerCase = segment.ToLowerInvariant();
            switch (segmentLowerCase)
            {
                case "fs":
                    normalizedSegments.Add("filesources");
                    break;
                case "fo":
                    normalizedSegments.Add("folders");
                    break;
                case "fi":
                    normalizedSegments.Add("files");
                    break;
                case "ut":
                    normalizedSegments.Add("utils");
                    break;
            }
        }

        return normalizedSegments.LastOrDefault() ?? "unknown";
    }

    private string GetContextOperationVerb(
        IOperationContext context,
        RouteEndpoint? endpoint,
        string resourceType,
        string method
    )
    {
        var routePattern = endpoint?.RoutePattern.RawText ?? string.Empty;

        var methodLowerCase = method.ToLowerInvariant();
        if (methodLowerCase == "post")
        {
            if (routePattern.EndsWith("/test", StringComparison.OrdinalIgnoreCase) && resourceType == "filesources")
            {
                return "test";
            }
            if (
                routePattern.EndsWith("/query", StringComparison.OrdinalIgnoreCase)
                && (resourceType == "tables" || resourceType == "views")
            )
            {
                return "query";
            }
            return "post";
        }

        if (methodLowerCase == "put")
        {
            return "put";
        }

        if (methodLowerCase == "delete")
        {
            return "delete";
        }

        if (methodLowerCase == "patch")
        {
            return "patch";
        }

        if (methodLowerCase == "get")
        {
            if (routePattern.EndsWith("/test", StringComparison.OrdinalIgnoreCase) && resourceType == "filesources")
            {
                return "test";
            }
            if (
                routePattern.EndsWith("/query", StringComparison.OrdinalIgnoreCase)
                && (resourceType == "tables" || resourceType == "views")
            )
            {
                return "query";
            }

            // If the route pattern ends with a parameter, it's a "get" operation
            return routePattern.EndsWith('}') || routePattern.EndsWith("/exists", StringComparison.OrdinalIgnoreCase)
                ? "get"
                : "list";
        }

        return "unknown";
    }

    private bool TryGetRouteValue(Dictionary<string, string> routeValues, string key, out string dsIdStr)
    {
        if (
            // TryGetValue but ignore case
            routeValues.TryGetValue(key, out var dsIdStrVal) && !string.IsNullOrWhiteSpace(dsIdStrVal)
        )
        {
            dsIdStr = dsIdStrVal;
            return true;
        }
        dsIdStr = null!;
        return false;
    }

    /// <summary>
    /// Extracts the client IP address from the HTTP context, handling proxy scenarios.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The client IP address, or null if it cannot be determined.</returns>
    private string? GetClientIpAddress(HttpContext httpContext)
    {
        // Check Cloudflare headers first (for proxy scenarios)
        var trueClientIp = httpContext.Request.Headers["True-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(trueClientIp))
        {
            return trueClientIp.Trim();
        }

        var cloudflareIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cloudflareIp))
        {
            return cloudflareIp.Trim();
        }

        // Check X-Forwarded-For header first (for proxy scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2, etc.)
            // The first one is typically the original client IP
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check X-Real-IP header (used by some proxies)
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Fall back to direct connection IP
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    [GeneratedRegex(@"/+")]
    private static partial Regex DoubleForwardSlashReducerRegex();

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex RouteValueStripperRegex();
}
