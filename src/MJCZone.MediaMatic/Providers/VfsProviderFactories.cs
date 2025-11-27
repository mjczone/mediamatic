// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Providers;

/// <summary>
/// Manages virtual file system connection factories.
/// </summary>
/// <remarks>
/// Internal implementation. Use <see cref="MJCZone.MediaMatic.VfsConnection"/> for public API.
/// </remarks>
internal static class VfsProviderFactories
{
    private static readonly ConcurrentDictionary<VfsProviderType, IVfsConnectionFactory> NativeFactories = new()
    {
        [VfsProviderType.Memory] = new Memory.MemoryVfsConnectionFactory(),
        [VfsProviderType.Local] = new Local.LocalVfsConnectionFactory(),
        [VfsProviderType.ZipFile] = new ZipFile.ZipFileVfsConnectionFactory(),
        [VfsProviderType.S3] = new S3.S3VfsConnectionFactory(),
        [VfsProviderType.SFTP] = new SFTP.SFTPVfsConnectionFactory(),
        [VfsProviderType.GCP] = new GCP.GCPVfsConnectionFactory(),
        [VfsProviderType.B2] = new B2.B2VfsConnectionFactory(),
        [VfsProviderType.Minio] = new Minio.MinioVfsConnectionFactory(),
    };

    private static readonly ConcurrentDictionary<string, IVfsConnectionFactory> CustomFactories = new();

    /// <summary>
    /// Registers a custom vfs connection factory.
    /// </summary>
    /// <param name="name">The name of the custom factory.</param>
    /// <param name="factory">The custom factory to register.</param>
    public static void RegisterFactory(string name, IVfsConnectionFactory factory)
    {
        CustomFactories.TryAdd(name.ToLowerInvariant(), factory);
    }

    /// <summary>
    /// Registers a vfs connection factory for a specific provider type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <param name="factory">The factory to register.</param>
    public static void RegisterFactory(VfsProviderType providerType, IVfsConnectionFactory factory)
    {
        if (providerType == VfsProviderType.Other)
        {
            RegisterFactory(Guid.NewGuid().ToString(), factory);
            return;
        }

        NativeFactories.AddOrUpdate(providerType, factory, (_, _) => factory);
    }

    /// <summary>
    /// Gets the vfs connection factory for a given provider type and connection string.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The vfs connection.</returns>
    /// <exception cref="NotSupportedException">Thrown when no factory is found for the provider type.</exception>
    public static IVfsConnectionFactory GetVfsConnectionFactory(VfsProviderType providerType)
    {
        if (NativeFactories.TryGetValue(providerType, out var factory))
        {
            return factory;
        }

        foreach (var customFactory in CustomFactories.Values)
        {
            if (customFactory.ProviderType == providerType)
            {
                return customFactory;
            }
        }

        throw new NotSupportedException($"No factory found for provider type {providerType}");
    }

    /// <summary>
    /// Creates a vfs connection for a given provider type and connection string.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The vfs connection.</returns>
    /// <exception cref="NotSupportedException">Thrown when no factory is found for the provider type.</exception>
    public static IVfsConnection CreateConnection(VfsProviderType providerType, string connectionString)
    {
        if (NativeFactories.TryGetValue(providerType, out var factory))
        {
            return factory.CreateConnection(connectionString);
        }

        foreach (var customFactory in CustomFactories.Values)
        {
            if (customFactory.ProviderType == providerType)
            {
                return customFactory.CreateConnection(connectionString);
            }
        }

        throw new NotSupportedException($"No factory found for provider type {providerType}");
    }
}
