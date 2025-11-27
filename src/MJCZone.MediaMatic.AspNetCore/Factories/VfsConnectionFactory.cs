// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.AspNetCore.Factories;

/// <summary>
/// Default implementation of IDbConnectionFactory that creates provider-specific database connections.
/// </summary>
public sealed class VfsConnectionFactory : IVfsConnectionFactory
{
    /// <summary>
    /// Creates a vfs connection for the specified provider and connection string.
    /// </summary>
    /// <param name="provider">The vfs provider (e.g., Local, S3, SFTP, Minio, B2, GCP, ZipFile).</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>A configured database connection.</returns>
    public IVfsConnection CreateConnection(string provider, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        IVfsConnection? connection = null;
        if (
            provider.Contains("disk", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("local", StringComparison.OrdinalIgnoreCase)
        )
        {
            connection = VfsConnection.Create(VfsProviderType.Local, connectionString);
        }
        else if (provider.Contains("sftp", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.SFTP, connectionString);
        }
        else if (provider.Contains("zip", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);
        }
        else if (provider.Contains("minio", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.Minio, connectionString);
        }
        else if (provider.Contains("b2", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.B2, connectionString);
        }
        else if (provider.Contains("s3", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.S3, connectionString);
        }
        else if (provider.Contains("gcp", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.GCP, connectionString);
        }
        else if (provider.Contains("memory", StringComparison.OrdinalIgnoreCase))
        {
            connection = VfsConnection.Create(VfsProviderType.Memory, connectionString);
        }

        return connection ?? throw new ArgumentException($"Unsupported vfs provider: {provider}", nameof(provider));
    }
}
