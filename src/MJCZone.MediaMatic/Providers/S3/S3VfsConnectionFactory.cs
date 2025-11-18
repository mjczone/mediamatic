// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.S3;

/// <summary>
/// Represents a factory for creating connections to a S3 virtual file system.
/// </summary>
public class S3VfsConnectionFactory : VfsConnectionFactoryBase<S3VfsConnection>
{
    static S3VfsConnectionFactory()
    {
        StorageFactory.Modules.UseAwsStorage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="S3VfsConnectionFactory"/> class.
    /// </summary>
    public S3VfsConnectionFactory()
        : base(VfsProviderType.S3) { }

    /// <summary>
    /// Creates a new connection to the S3 virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the S3 virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new S3VfsConnection(connectionString);
    }
}
