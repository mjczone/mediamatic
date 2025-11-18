// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Minio;

/// <summary>
/// Represents a factory for creating connections to a Minio virtual file system.
/// </summary>
public class MinioVfsConnectionFactory : VfsConnectionFactoryBase<MinioVfsConnection>
{
    static MinioVfsConnectionFactory()
    {
        StorageFactory.Modules.UseAwsStorage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinioVfsConnectionFactory"/> class.
    /// </summary>
    public MinioVfsConnectionFactory()
        : base(VfsProviderType.Minio) { }

    /// <summary>
    /// Creates a new connection to the Minio virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the Minio virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new MinioVfsConnection(connectionString);
    }
}
