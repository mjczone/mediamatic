// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic;

/// <summary>
/// Main interface for media storage operations across multiple providers.
/// Wraps FluentStorage IBlobStorage with MediaMatic-specific enhancements.
/// </summary>
public interface IMediaStorage
{
    // TODO: Define core media storage interface
    // - Upload/download operations
    // - Metadata extraction and management
    // - Image processing (resize, convert, optimize)
    // - Video processing (thumbnails, transcoding)
    // - Format recommendations

    /// <summary>
    /// Gets the name of the media storage provider.
    /// </summary>
    string Name { get; }
}
