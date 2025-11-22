// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response model for filesource operations.
/// </summary>
public class FilesourceResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceResponse"/> class.
    /// </summary>
    public FilesourceResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceResponse"/> class.
    /// </summary>
    /// <param name="result">The filesource information.</param>
    public FilesourceResponse(FilesourceDto result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the filesource data.
    /// </summary>
    public FilesourceDto? Result { get; set; }
}
