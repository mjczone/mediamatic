// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Represents a connection to a virtual file system.
/// </summary>
/// <typeparam name="TConnection">The type of the virtual file system connection.</typeparam>
public abstract class VfsConnectionFactoryBase<TConnection> : IVfsConnectionFactory
    where TConnection : class, IVfsConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VfsConnectionFactoryBase{TConnection}"/> class.
    /// </summary>
    /// <param name="providerType">The provider type for the virtual file system.</param>
    protected VfsConnectionFactoryBase(VfsProviderType providerType)
    {
        ProviderType = providerType;
    }

    /// <summary>
    /// Gets the provider type for the virtual file system.
    /// </summary>
    public VfsProviderType ProviderType { get; }

    /// <summary>
    /// Creates a new connection to the virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public abstract IVfsConnection CreateConnection(string connectionString);
}
