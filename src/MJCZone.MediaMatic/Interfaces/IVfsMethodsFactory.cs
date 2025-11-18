// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Interfaces;

/// <summary>
/// Factory interface for creating virtual file system methods.
/// </summary>
public interface IVfsMethodsFactory
{
    /// <summary>
    /// Determines whether the factory supports the specified virtual file system connection.
    /// </summary>
    /// <param name="vfs">The virtual file system connection.</param>
    /// <returns><c>true</c> if the factory supports the specified virtual file system connection; otherwise, <c>false</c>.</returns>
    bool SupportsConnection(IVfsConnection vfs);

    /// <summary>
    /// Gets the virtual file system methods for the specified virtual file system connection.
    /// </summary>
    /// <param name="vfs">The virtual file system connection.</param>
    /// <returns>An instance of <see cref="IVfsMethods"/> for the specified virtual file system connection.</returns>
    IVfsMethods GetMethods(IVfsConnection vfs);
}
