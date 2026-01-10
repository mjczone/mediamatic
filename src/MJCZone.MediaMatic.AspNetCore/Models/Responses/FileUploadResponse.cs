// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Responses;

/// <summary>
/// Response containing the path to an uploaded file.
/// </summary>
public class FileUploadResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadResponse"/> class.
    /// </summary>
    public FileUploadResponse()
    {
        Path = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadResponse"/> class.
    /// </summary>
    /// <param name="path">The path to the uploaded file.</param>
    public FileUploadResponse(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Gets or sets the path to the uploaded file.
    /// </summary>
    public string Path { get; set; }
}
