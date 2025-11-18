// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents supported video formats.
/// </summary>
public enum VideoFormat
{
    /// <summary>
    /// MP4 format (H.264 codec).
    /// </summary>
    Mp4,

    /// <summary>
    /// WebM format (VP9 codec).
    /// </summary>
    WebM,

    /// <summary>
    /// AVI format.
    /// </summary>
    Avi,

    /// <summary>
    /// MOV format (QuickTime).
    /// </summary>
    Mov,

    /// <summary>
    /// MKV format (Matroska).
    /// </summary>
    Mkv,

    /// <summary>
    /// FLV format (Flash Video).
    /// </summary>
    Flv,

    /// <summary>
    /// WMV format (Windows Media Video).
    /// </summary>
    Wmv,
}
