// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic.Interfaces;

/// <summary>
/// Defines a connection factory to a virtual file system.
/// </summary>
public interface IVfsConnectionFactory
{
    /// <summary>
    /// Gets the provider type for the virtual file system.
    /// </summary>
    public VfsProviderType ProviderType { get; }

    /// <summary>
    /// Creates a new connection to the virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public IVfsConnection CreateConnection(string connectionString);
}
