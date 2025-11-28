// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Request body for batch transform operations.
/// </summary>
public sealed class TransformBatchRequestDto
{
    /// <summary>
    /// Gets or sets the source file path to transform.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of transformation variants to generate.
    /// </summary>
    public List<TransformVariantDto> Variants { get; set; } = [];
}
