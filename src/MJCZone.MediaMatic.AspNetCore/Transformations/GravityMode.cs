// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Transformations;

/// <summary>
/// Specifies the gravity point for image cropping operations.
/// </summary>
public enum GravityMode
{
    /// <summary>
    /// Center of the image.
    /// </summary>
    Center,

    /// <summary>
    /// Top center of the image.
    /// </summary>
    North,

    /// <summary>
    /// Bottom center of the image.
    /// </summary>
    South,

    /// <summary>
    /// Right center of the image.
    /// </summary>
    East,

    /// <summary>
    /// Left center of the image.
    /// </summary>
    West,

    /// <summary>
    /// Top right corner of the image.
    /// </summary>
    NorthEast,

    /// <summary>
    /// Top left corner of the image.
    /// </summary>
    NorthWest,

    /// <summary>
    /// Bottom right corner of the image.
    /// </summary>
    SouthEast,

    /// <summary>
    /// Bottom left corner of the image.
    /// </summary>
    SouthWest,

    /// <summary>
    /// Custom focal point specified by x and y coordinates.
    /// </summary>
    Custom,
}
