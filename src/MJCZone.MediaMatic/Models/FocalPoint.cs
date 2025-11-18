// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a focal point within an image.
/// </summary>
public class FocalPoint
{
    /// <summary>
    /// Gets or sets the X coordinate of the focal point (0.0 to 1.0).
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the focal point (0.0 to 1.0).
    /// </summary>
    public double Y { get; set; }
}
