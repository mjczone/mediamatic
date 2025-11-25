// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Endpoints;

/// <summary>
/// Constants for valid folder content types.
/// </summary>
public static class FolderContentTypes
{
    /// <summary>
    /// List files in a folder.
    /// </summary>
    public const string Files = "files";

    /// <summary>
    /// List subfolders in a folder.
    /// </summary>
    public const string Folders = "folders";
}
