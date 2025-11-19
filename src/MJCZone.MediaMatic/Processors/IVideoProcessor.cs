// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Provides video processing operations (thumbnails, transcoding).
/// </summary>
public interface IVideoProcessor
{
    /// <summary>
    /// Generates thumbnails from a video file.
    /// </summary>
    /// <param name="videoPath">Path to the video file.</param>
    /// <param name="options">Thumbnail generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of paths to generated thumbnail files.</returns>
    Task<List<string>> GenerateThumbnailsAsync(
        string videoPath,
        ThumbnailOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Transcodes a video to a different format/codec.
    /// </summary>
    /// <param name="sourcePath">Path to the source video file.</param>
    /// <param name="destinationPath">Path to the destination video file.</param>
    /// <param name="options">Transcoding options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the transcoded video file.</returns>
    Task<string> TranscodeAsync(
        string sourcePath,
        string destinationPath,
        TranscodeOptions options,
        CancellationToken cancellationToken = default
    );
}
