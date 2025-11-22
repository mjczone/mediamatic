// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response model for listing multiple filesources.
/// </summary>
public class FilesourceListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceListResponse"/> class.
    /// </summary>
    public FilesourceListResponse()
    {
        Result = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceListResponse"/> class.
    /// </summary>
    /// <param name="result">The list of filesources.</param>
    public FilesourceListResponse(IEnumerable<FilesourceDto> result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the list of filesources.
    /// </summary>
    public IEnumerable<FilesourceDto> Result { get; set; }
}
