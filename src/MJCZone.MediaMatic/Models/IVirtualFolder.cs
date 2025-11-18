// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a virtual folder in a filesource.
/// </summary>
public interface IVirtualFolder
{
    /// <summary>
    /// Gets the name of the bucket this virtual folder belongs to, if applicable.
    /// </summary>
    string? BucketName { get; }

    /// <summary>
    /// Gets the name of the virtual folder.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the slug of the virtual folder.
    /// </summary>
    string Slug { get; }

    /// <summary>
    /// Gets the path of the virtual folder.
    /// </summary>
    string Path { get; }
}
