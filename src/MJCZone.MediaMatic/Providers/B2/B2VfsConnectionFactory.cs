// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.B2;

/// <summary>
/// Represents a factory for creating connections to a B2 virtual file system.
/// </summary>
public class B2VfsConnectionFactory : VfsConnectionFactoryBase<B2VfsConnection>
{
    static B2VfsConnectionFactory()
    {
        StorageFactory.Modules.UseAwsStorage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="B2VfsConnectionFactory"/> class.
    /// </summary>
    public B2VfsConnectionFactory()
        : base(VfsProviderType.B2) { }

    /// <summary>
    /// Creates a new connection to the B2 virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the B2 virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new B2VfsConnection(connectionString);
    }
}
