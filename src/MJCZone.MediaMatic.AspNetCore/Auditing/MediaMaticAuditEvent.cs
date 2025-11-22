// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Auditing;

/// <summary>
/// Represents an audit event for a MediaMatic operation.
/// </summary>
public class MediaMaticAuditEvent
{
    /// <summary>
    /// Gets or sets the identifier of the user who performed the operation.
    /// </summary>
    public string UserIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation that was performed (e.g., "datasources/post", "tables/put").
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the filesource involved in the operation, if applicable.
    /// </summary>
    public string? FilesourceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the bucket involved in the operation, if applicable.
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the name of the folder involved in the operation, if applicable.
    /// </summary>
    public string? FolderName { get; set; }

    /// <summary>
    /// Gets or sets the path of the folder involved in the operation, if applicable.
    /// </summary>
    public string? FolderPath { get; set; }

    /// <summary>
    /// Gets or sets the original file name involved in the operation, if applicable.
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the name of the file being accessed, if applicable.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the path of the file being accessed, if applicable.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes, if applicable.
    /// </summary>
    public long? FileSizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the operation outcome.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the operation occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the request ID for correlating logs.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the client.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets additional properties for custom audit information.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = [];
}
