// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Represents a single transformation variant in a batch request.
/// </summary>
public sealed class TransformVariantDto
{
    /// <summary>
    /// Gets or sets the transformation parameters (e.g., "w_400,h_300,f_webp").
    /// </summary>
    public string Transformations { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination path to save this transformed variant.
    /// </summary>
    public string SaveTo { get; set; } = string.Empty;
}
