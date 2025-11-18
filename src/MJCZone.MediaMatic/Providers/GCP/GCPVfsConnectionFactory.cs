// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.GCP;

/// <summary>
/// Represents a factory for creating connections to a GCP virtual file system.
/// </summary>
public class GCPVfsConnectionFactory : VfsConnectionFactoryBase<GCPVfsConnection>
{
    static GCPVfsConnectionFactory()
    {
        StorageFactory.Modules.UseGoogleCloudStorage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GCPVfsConnectionFactory"/> class.
    /// </summary>
    public GCPVfsConnectionFactory()
        : base(VfsProviderType.GCP) { }

    /// <summary>
    /// Creates a new connection to the GCP virtual file system.
    /// </summary>
    /// <param name="connectionString">The connection string for the GCP virtual file system.</param>
    /// <returns>A new instance of the <see cref="IVfsConnection"/> class.</returns>
    public override IVfsConnection CreateConnection(string connectionString)
    {
        return new GCPVfsConnection(connectionString);
    }
}
