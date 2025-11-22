// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response model for filesource connectivity test operations.
/// </summary>
public class FilesourceTestResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceTestResponse"/> class.
    /// </summary>
    public FilesourceTestResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceTestResponse"/> class.
    /// </summary>
    /// <param name="result">The filesource connectivity test information.</param>
    public FilesourceTestResponse(FilesourceConnectivityTestDto result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the filesource connectivity tests data.
    /// </summary>
    public FilesourceConnectivityTestDto? Result { get; set; }
}
