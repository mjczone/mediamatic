// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.SFTP;

/// <summary>
/// Represents a factory for creating connections to a SFTP virtual file system.
/// </summary>
public class SFTPVfsConnectionFactory : VfsConnectionFactoryBase<SFTPVfsConnection>
{
    static SFTPVfsConnectionFactory()
    {
        StorageFactory.Modules.UseSftpStorage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SFTPVfsConnectionFactory"/> class.
    /// </summary>
    public SFTPVfsConnectionFactory()
        : base(VfsProviderType.SFTP) { }

    /// <summary>
    /// Creates a new connection to the SFTP virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the SFTP virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new SFTPVfsConnection(connectionString);
    }
}
