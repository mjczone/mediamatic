// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents supported image formats.
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// JPEG format (lossy compression).
    /// </summary>
    Jpeg,

    /// <summary>
    /// PNG format (lossless compression).
    /// </summary>
    Png,

    /// <summary>
    /// WebP format (modern, efficient).
    /// </summary>
    WebP,

    /// <summary>
    /// AVIF format (next-gen, highly efficient).
    /// </summary>
    Avif,

    /// <summary>
    /// GIF format (animated images).
    /// </summary>
    Gif,

    /// <summary>
    /// BMP format (uncompressed).
    /// </summary>
    Bmp,

    /// <summary>
    /// TIFF format (high quality).
    /// </summary>
    Tiff,
}
