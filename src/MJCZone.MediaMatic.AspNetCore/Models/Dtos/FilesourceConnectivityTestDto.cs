// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Result of a filesource connectivity test.
/// </summary>
public class FilesourceConnectivityTestDto
{
    /// <summary>
    /// Gets or sets the filesource ID that was tested.
    /// </summary>
    public string? FilesourceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection was successful.
    /// </summary>
    public bool Connected { get; set; }

    /// <summary>
    /// Gets or sets the filesource provider name.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets the filesource name if available.
    /// </summary>
    public string? FilesourceName { get; set; }

    /// <summary>
    /// Gets or sets any error message if the connection failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }
}
