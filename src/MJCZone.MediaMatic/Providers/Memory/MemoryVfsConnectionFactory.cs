// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Memory;

/// <summary>
/// Represents a factory for creating connections to a Memory virtual file system.
/// </summary>
public class MemoryVfsConnectionFactory : VfsConnectionFactoryBase<MemoryVfsConnection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVfsConnectionFactory"/> class.
    /// </summary>
    public MemoryVfsConnectionFactory()
        : base(VfsProviderType.Memory) { }

    /// <summary>
    /// Creates a new connection to the Memory virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the Memory virtual file system. This parameter is ignored.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new MemoryVfsConnection();
    }
}
