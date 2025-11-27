// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic;

/// <summary>
/// Factory for creating virtual file system connections.
/// </summary>
/// <example>
/// <code>
/// using var vfs = VfsConnection.Create(VfsProviderType.S3, "s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1");
/// await vfs.UploadFileAsync(stream, "images/photo.jpg");
/// </code>
/// </example>
public static class VfsConnection
{
    /// <summary>
    /// Creates a VFS connection for the specified provider type and connection string.
    /// </summary>
    /// <param name="providerType">The storage provider type.</param>
    /// <param name="connectionString">The connection string for the provider.</param>
    /// <returns>A new VFS connection instance.</returns>
    /// <exception cref="NotSupportedException">Thrown when no factory is found for the provider type.</exception>
    /// <example>
    /// <code>
    /// // Create an S3 connection
    /// using var s3 = VfsConnection.Create(VfsProviderType.S3, "s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1");
    ///
    /// // Create a local file system connection
    /// using var local = VfsConnection.Create(VfsProviderType.Local, "/var/media");
    ///
    /// // Create an in-memory connection for testing
    /// using var memory = VfsConnection.Create(VfsProviderType.Memory, "memory://name=test");
    /// </code>
    /// </example>
    public static IVfsConnection Create(VfsProviderType providerType, string connectionString)
    {
        return VfsProviderFactories.CreateConnection(providerType, connectionString);
    }

    /// <summary>
    /// Gets the connection factory for a specific provider type.
    /// </summary>
    /// <param name="providerType">The storage provider type.</param>
    /// <returns>The connection factory for the provider.</returns>
    /// <exception cref="NotSupportedException">Thrown when no factory is found for the provider type.</exception>
    public static IVfsConnectionFactory GetFactory(VfsProviderType providerType)
    {
        return VfsProviderFactories.GetVfsConnectionFactory(providerType);
    }

    /// <summary>
    /// Registers a custom connection factory for a provider type.
    /// </summary>
    /// <param name="providerType">The provider type to register.</param>
    /// <param name="factory">The factory instance.</param>
    public static void RegisterFactory(VfsProviderType providerType, IVfsConnectionFactory factory)
    {
        VfsProviderFactories.RegisterFactory(providerType, factory);
    }

    /// <summary>
    /// Registers a custom connection factory by name.
    /// </summary>
    /// <param name="name">The name for the custom factory.</param>
    /// <param name="factory">The factory instance.</param>
    public static void RegisterFactory(string name, IVfsConnectionFactory factory)
    {
        VfsProviderFactories.RegisterFactory(name, factory);
    }
}
