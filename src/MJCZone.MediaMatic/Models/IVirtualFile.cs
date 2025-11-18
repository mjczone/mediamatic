// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a virtual file in a filesource.
/// </summary>
public interface IVirtualFile
{
    /// <summary>
    /// Gets the name of the bucket this virtual file belongs to, if applicable.
    /// </summary>
    string? BucketName { get; }

    /// <summary>
    /// Gets the path of the virtual file (starting with '/', the root of the bucket and/or virtual file system).
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the name of the file on disk.
    /// </summary>
    string NameOnDisk { get; }

    /// <summary>
    /// Gets the title of the virtual file (a user-friendly name).
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the name of the virtual file (the file name without path, typically the original name of the file).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the slug of the virtual file.
    /// </summary>
    string Slug { get; }

    /// <summary>
    /// Gets the size of the virtual file in bytes.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Gets the charset of the virtual file, if applicable (e.g., "utf-8", "binary").
    /// </summary>
    string? Charset { get; }

    /// <summary>
    /// Gets the creation date and time of the virtual file.
    /// </summary>
    DateTime Created { get; }

    /// <summary>
    /// Gets the last modified date and time of the virtual file.
    /// </summary>
    DateTime LastModified { get; }

    /// <summary>
    /// Gets the ETag of the virtual file, if applicable.
    /// </summary>
    string? ETag { get; }

    /// <summary>
    /// Gets the content type of the virtual file, if applicable.
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    /// Gets a value indicating whether the virtual file is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets a value indicating whether the virtual file exists.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Gets the virtual folder that contains this virtual file.
    /// </summary>
    IVirtualFolder? VirtualFolder { get; }

    /// <summary>
    /// Gets the width of the virtual file, if applicable.
    /// </summary>
    int? Width { get; }

    /// <summary>
    /// Gets the height of the virtual file, if applicable.
    /// </summary>
    int? Height { get; }

    /// <summary>
    /// Gets the duration of the virtual file, if applicable.
    /// </summary>
    TimeSpan? Duration { get; }

    /// <summary>
    /// Gets the focal point of the virtual file, if applicable.
    /// </summary>
    FocalPoint? FocalPoint { get; }
}
