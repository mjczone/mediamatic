// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Internal representation of a virtual file type with full metadata.
/// </summary>
public class VFileTypeInfo
{
    /// <summary>
    /// Gets or sets the virtual file type name (e.g., "image/png", "video/mp4").
    /// </summary>
    public string ContentType { get; set; } = default!;

    /// <summary>
    /// Gets or sets an optional description or documentation for the virtual file type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the file type category associated with this virtual file type (e.g., ".png", ".mp4").
    /// </summary>
    public VFileTypeCategory Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file type is binary.
    /// </summary>
    public bool IsBinary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file type supports width and height metadata.
    /// </summary>
    public bool SupportsWidthHeight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file type supports duration metadata.
    /// </summary>
    public bool SupportsDuration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file type supports general metadata.
    /// </summary>
    public bool SupportsMetadata { get; set; }
}
