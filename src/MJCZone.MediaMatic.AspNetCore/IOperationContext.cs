// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Interface for MediaMatic operation context.
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// Gets or sets the user's claims principal.
    /// </summary>
    ClaimsPrincipal? User { get; set; }

    /// <summary>
    /// Gets or sets the operation being performed (e.g., "filesources/get", "filesources/post").
    /// </summary>
    string? Operation { get; set; }

    /// <summary>
    /// Gets or sets the name of the filesource being accessed, if applicable.
    /// </summary>
    string? FilesourceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the bucket being accessed, if applicable.
    /// </summary>
    string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the name of the folder being accessed, if applicable.
    /// </summary>
    string? FolderName { get; set; }

    /// <summary>
    /// Gets or sets the path of the folder being accessed, if applicable.
    /// </summary>
    string? FolderPath { get; set; }

    /// <summary>
    /// Gets or sets the original file name being uploaded or accessed, if applicable.
    /// </summary>
    string? OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the name of the file being accessed on disk, if applicable.
    /// </summary>
    string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the path of the file being accessed, if applicable.
    /// </summary>
    string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the size of the file being accessed, if applicable.
    /// </summary>
    long? FileSizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file being accessed, if applicable.
    /// </summary>
    string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method (GET, POST, etc.).
    /// </summary>
    string? HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the endpoint path.
    /// </summary>
    string? EndpointPath { get; set; }

    /// <summary>
    /// Gets or sets the request payload, if any.
    /// </summary>
    object? RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the query parameters as a case-insensitive dictionary, if any.
    /// </summary>
    Dictionary<string, StringValues>? QueryParameters { get; set; }

    /// <summary>
    /// Gets or sets the route values as a case-insensitive dictionary, if any.
    /// </summary>
    Dictionary<string, string>? RouteValues { get; set; }

    /// <summary>
    /// Gets or sets the header values as a case-insensitive dictionary, if any.
    /// </summary>
    Dictionary<string, string>? HeaderValues { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the client.
    /// </summary>
    string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the request ID for correlating logs.
    /// </summary>
    string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets additional properties for custom authorization logic.
    /// </summary>
    Dictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// Gets the request body as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to cast the request body to.</typeparam>
    /// <returns>The request body as type T, or default if the cast fails.</returns>
    T? GetRequest<T>()
        where T : class;

    /// <summary>
    /// Gets a query parameter value by key.
    /// </summary>
    /// <param name="key">The query parameter key.</param>
    /// <returns>The query parameter value, or null if not found.</returns>
    string? GetQueryParameter(string key);
}
