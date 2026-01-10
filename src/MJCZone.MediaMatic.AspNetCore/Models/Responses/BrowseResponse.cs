// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Response containing files and folders from a browse operation.
/// </summary>
public sealed class BrowseResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseResponse"/> class.
    /// </summary>
    public BrowseResponse()
    {
        Result = new BrowseResultDto();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseResponse"/> class.
    /// </summary>
    /// <param name="result">The browse result data.</param>
    public BrowseResponse(BrowseResultDto result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the browse result data.
    /// </summary>
    public BrowseResultDto Result { get; set; }
}
