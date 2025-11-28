// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Request body for single POST transform operations.
/// </summary>
public sealed class TransformRequestDto
{
    /// <summary>
    /// Gets or sets the destination path to save the transformed image.
    /// </summary>
    public string SaveTo { get; set; } = string.Empty;
}
