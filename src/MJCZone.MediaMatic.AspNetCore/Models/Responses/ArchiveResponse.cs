// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response for archive creation request.
/// </summary>
public class ArchiveResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveResponse"/> class.
    /// </summary>
    public ArchiveResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveResponse"/> class.
    /// </summary>
    /// <param name="result">The column information.</param>
    public ArchiveResponse(ArchiveResultDto result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the column data.
    /// </summary>
    public ArchiveResultDto? Result { get; set; }
}
