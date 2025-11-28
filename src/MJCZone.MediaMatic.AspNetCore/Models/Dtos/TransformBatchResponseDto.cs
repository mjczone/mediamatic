// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Response containing results from a batch transform operation.
/// </summary>
public sealed class TransformBatchResponseDto
{
    /// <summary>
    /// Gets or sets the list of transformation results.
    /// </summary>
    public List<TransformResultDto> Results { get; set; } = [];
}
