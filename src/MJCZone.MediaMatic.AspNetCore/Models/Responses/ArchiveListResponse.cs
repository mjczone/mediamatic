// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response containing a list of archives.
/// </summary>
public class ArchiveListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveListResponse"/> class.
    /// </summary>
    public ArchiveListResponse()
    {
        Result = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveListResponse"/> class.
    /// </summary>
    /// <param name="archives">The list of archives.</param>
    public ArchiveListResponse(IEnumerable<ArchiveFileDto> archives)
    {
        Result = archives;
    }

    /// <summary>
    /// Gets or sets the list of archives.
    /// </summary>
    public IEnumerable<ArchiveFileDto> Result { get; set; }
}
