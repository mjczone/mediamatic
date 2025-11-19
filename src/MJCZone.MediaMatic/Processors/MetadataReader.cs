// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Extracts metadata from images and videos using MetadataExtractor and FFMpegCore.
/// </summary>
public class MetadataReader : IMetadataReader
{
    /// <inheritdoc/>
    public Task<MediaMetadata> ExtractImageMetadataAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using MetadataExtractor library
        throw new NotImplementedException("Image metadata extraction not yet implemented");
    }

    /// <inheritdoc/>
    public Task<MediaMetadata> ExtractVideoMetadataAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using FFMpegCore library
        throw new NotImplementedException("Video metadata extraction not yet implemented");
    }
}
