// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Providers;

/// <summary>
/// Provides methods for registering and retrieving vfs method factories.
/// </summary>
public static class VfsMethodsProvider
{
    private static readonly ConcurrentDictionary<VfsProviderType, IVfsMethodsFactory> NativeFactories = new()
    {
        [VfsProviderType.Memory] = new Memory.MemoryMethodsFactory(),
        [VfsProviderType.Local] = new Local.LocalMethodsFactory(),
        [VfsProviderType.ZipFile] = new ZipFile.ZipFileMethodsFactory(),
        [VfsProviderType.S3] = new S3.S3MethodsFactory(),
        [VfsProviderType.FTP] = new FTP.FTPMethodsFactory(),
        [VfsProviderType.SFTP] = new SFTP.SFTPMethodsFactory(),
        [VfsProviderType.GCP] = new GCP.GCPMethodsFactory(),
        [VfsProviderType.B2] = new B2.B2MethodsFactory(),
        [VfsProviderType.Minio] = new Minio.MinioMethodsFactory(),
    };

    private static readonly ConcurrentDictionary<string, IVfsMethodsFactory> CustomFactories = new();

    /// <summary>
    /// Registers a custom vfs methods factory.
    /// </summary>
    /// <param name="name">The name of the custom factory.</param>
    /// <param name="factory">The custom factory to register.</param>
    public static void RegisterFactory(string name, IVfsMethodsFactory factory)
    {
        CustomFactories.TryAdd(name.ToLowerInvariant(), factory);
    }

    /// <summary>
    /// Registers a vfs methods factory for a specific provider type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <param name="factory">The factory to register.</param>
    public static void RegisterFactory(VfsProviderType providerType, IVfsMethodsFactory factory)
    {
        if (providerType == VfsProviderType.Other)
        {
            RegisterFactory(Guid.NewGuid().ToString(), factory);
            return;
        }

        NativeFactories.AddOrUpdate(providerType, factory, (_, _) => factory);
    }

    /// <summary>
    /// Gets the vfs methods for a given vfs connection.
    /// </summary>
    /// <param name="vfs">The vfs connection.</param>
    /// <returns>The vfs methods.</returns>
    /// <exception cref="NotSupportedException">Thrown when no factory is found for the connection type.</exception>
    public static IVfsMethods GetMethods(IVfsConnection vfs)
    {
        foreach (var factory in CustomFactories.Values)
        {
            if (factory.SupportsConnection(vfs))
            {
                return factory.GetMethods(vfs);
            }
        }

        foreach (var factory in NativeFactories.Values)
        {
            if (factory.SupportsConnection(vfs))
            {
                return factory.GetMethods(vfs);
            }
        }

        throw new NotSupportedException($"No factory found for connection type {vfs.GetType().FullName}");
    }
}
