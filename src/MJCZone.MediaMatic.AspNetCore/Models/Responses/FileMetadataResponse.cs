// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response containing file metadata.
/// </summary>
public class FileMetadataResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileMetadataResponse"/> class.
    /// </summary>
    public FileMetadataResponse()
    {
        Result = new MediaMatic.Models.MediaMetadata();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMetadataResponse"/> class.
    /// </summary>
    /// <param name="metadata">The file metadata.</param>
    public FileMetadataResponse(MediaMatic.Models.MediaMetadata metadata)
    {
        Result = metadata;
    }

    /// <summary>
    /// Gets or sets the file metadata.
    /// </summary>
    public MediaMatic.Models.MediaMetadata Result { get; set; }
}
