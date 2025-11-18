// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a variant of an uploaded image (different format or size).
/// </summary>
public class ImageVariant
{
    /// <summary>
    /// Gets or sets the path to the variant file.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the image format.
    /// </summary>
    public required ImageFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the variant type (original, thumbnail, format variant).
    /// </summary>
    public string? VariantType { get; set; }
}
