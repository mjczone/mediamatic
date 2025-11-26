// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Utility class for identifying MediaMatic operation tags.
/// </summary>
public static class OperationTags
{
    /// <summary>
    /// Tag for all MediaMatic Datasource-related operations.
    /// </summary>
    public const string Filesources = "MediaMatic Filesources";

    /// <summary>
    /// Tag for all MediaMatic Folder-related operations.
    /// </summary>
    public const string FilesourceFolders = "MediaMatic Folders";

    /// <summary>
    /// Tag for all MediaMatic File-related operations.
    /// </summary>
    public const string FilesourceFiles = "MediaMatic Files";

    /// <summary>
    /// Tag for all MediaMatic Utility-related operations.
    /// </summary>
    public const string FilesourceUtilities = "MediaMatic Utilities";

    /// <summary>
    /// Tag for all MediaMatic Image Transformation operations.
    /// </summary>
    public const string FilesourceTransformations = "MediaMatic Transformations";
}
