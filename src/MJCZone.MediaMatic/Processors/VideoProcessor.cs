// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Processes videos using FFMpegCore library (thumbnails, transcoding).
/// </summary>
public class VideoProcessor : IVideoProcessor
{
    /// <inheritdoc/>
    public Task<List<string>> GenerateThumbnailsAsync(
        string videoPath,
        ThumbnailOptions options,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using FFMpegCore 5.4.0 API
        throw new NotImplementedException("Video thumbnail generation not yet implemented");
    }

    /// <inheritdoc/>
    public Task<string> TranscodeAsync(
        string sourcePath,
        string destinationPath,
        TranscodeOptions options,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using FFMpegCore 5.4.0 API
        throw new NotImplementedException("Video transcoding not yet implemented");
    }
}
