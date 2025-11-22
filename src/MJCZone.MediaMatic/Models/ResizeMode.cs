// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Specifies how an image or video frame should be resized to fit target dimensions.
/// </summary>
public enum ResizeMode
{
    /// <summary>
    /// Scale to fit within the target dimensions while preserving aspect ratio.
    /// The result may be smaller than the target in one dimension.
    /// Equivalent to CSS object-fit: contain.
    /// </summary>
    Fit,

    /// <summary>
    /// Scale to cover the target dimensions while preserving aspect ratio.
    /// Excess content is cropped from center.
    /// Equivalent to CSS object-fit: cover.
    /// </summary>
    Cover,

    /// <summary>
    /// Scale to fit within target dimensions and add padding to fill remaining space.
    /// Uses BackgroundColor for padding.
    /// </summary>
    Pad,

    /// <summary>
    /// Stretch to exactly match target dimensions, ignoring aspect ratio.
    /// Equivalent to CSS object-fit: fill.
    /// </summary>
    Stretch,
}
