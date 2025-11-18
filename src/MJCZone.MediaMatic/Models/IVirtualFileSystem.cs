// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a virtual file system in a filesource.
/// </summary>
public interface IVirtualFileSystem
{
    /// <summary>
    /// Gets the name of the provider for this virtual file system.
    /// </summary>
    string? ProviderName { get; }

    /// <summary>
    /// Gets the list of virtual folders in the virtual file system.
    /// </summary>
    /// <returns>List of virtual folders.</returns>
    IEnumerable<IVirtualFolder> GetVirtualFolders();

    /// <summary>
    /// Gets the list of virtual files in the virtual file system.
    /// </summary>
    /// <returns>List of virtual files.</returns>
    IEnumerable<IVirtualFile> GetVirtualFiles();
}
