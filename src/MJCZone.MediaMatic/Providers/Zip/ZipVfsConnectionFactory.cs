// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.ZipFile;

/// <summary>
/// Represents a factory for creating connections to a ZIP virtual file system.
/// </summary>
public class ZipVfsConnectionFactory : VfsConnectionFactoryBase<ZipFileVfsConnection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipVfsConnectionFactory"/> class.
    /// </summary>
    public ZipVfsConnectionFactory()
        : base(VfsProviderType.ZipFile) { }

    /// <summary>
    /// Creates a new connection to the ZIP virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the ZIP virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new ZipFileVfsConnection(connectionString);
    }
}
