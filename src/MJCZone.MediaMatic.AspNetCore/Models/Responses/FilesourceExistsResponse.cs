// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response model for checking if a filesource exists.
/// </summary>
public class FilesourceExistsResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceExistsResponse"/> class.
    /// </summary>
    public FilesourceExistsResponse()
    {
        Result = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesourceExistsResponse"/> class.
    /// </summary>
    /// <param name="exists">Whether the filesource exists.</param>
    public FilesourceExistsResponse(bool exists)
    {
        Result = exists;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the filesource exists.
    /// </summary>
    public bool Result { get; set; }
}
