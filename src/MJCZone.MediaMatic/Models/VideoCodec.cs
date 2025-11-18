// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents supported video codecs.
/// </summary>
public enum VideoCodec
{
    /// <summary>
    /// H.264/AVC codec (widely supported).
    /// </summary>
    H264,

    /// <summary>
    /// H.265/HEVC codec (better compression).
    /// </summary>
    H265,

    /// <summary>
    /// VP8 codec (WebM).
    /// </summary>
    VP8,

    /// <summary>
    /// VP9 codec (WebM, better than VP8).
    /// </summary>
    VP9,

    /// <summary>
    /// AV1 codec (next-gen, highly efficient).
    /// </summary>
    AV1,
}
