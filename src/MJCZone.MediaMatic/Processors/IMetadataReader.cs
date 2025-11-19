// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Provides metadata extraction from images and videos.
/// </summary>
public interface IMetadataReader
{
    /// <summary>
    /// Extracts metadata from an image stream.
    /// </summary>
    /// <param name="stream">The image stream. Position will be reset after extraction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted metadata.</returns>
    Task<MediaMetadata> ExtractImageMetadataAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts metadata from a video file.
    /// </summary>
    /// <param name="filePath">The path to the video file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted metadata.</returns>
    Task<MediaMetadata> ExtractVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default);
}
