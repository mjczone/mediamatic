// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Result of an image upload operation.
/// </summary>
public class ImageUploadResult
{
    /// <summary>
    /// Gets or sets the path to the original (primary) uploaded image.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the original width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the original height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the detected image format.
    /// </summary>
    public ImageFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the extracted metadata.
    /// </summary>
    public MediaMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the generated variants (different formats and sizes).
    /// </summary>
    public List<ImageVariant> Variants { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the upload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets error message if upload failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
