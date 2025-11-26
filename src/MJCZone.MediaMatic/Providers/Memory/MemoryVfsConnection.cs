// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Memory;

/// <summary>
/// Represents a connection to a Memory virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be empty or specify a name for the in-memory storage instance.
/// Format: "memory://" (default instance) or "memory://name=mystore" (named instance).
/// All connections with the same name share the same underlying storage.
/// </remarks>
public class MemoryVfsConnection : VfsConnectionBase
{
    private static readonly ConcurrentDictionary<string, IBlobStorage> SharedStorageInstances = new();
    private readonly string _instanceName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">Connection string (optional, defaults to "memory://").</param>
    public MemoryVfsConnection(string? connectionString = null)
        : base(VfsProviderType.Memory, GetOrCreateStorage(connectionString ?? "memory://"))
    {
        _instanceName = ParseInstanceName(connectionString ?? "memory://");
    }

    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    public override string ConnectionString =>
        string.IsNullOrEmpty(_instanceName) ? "memory://" : $"memory://name={_instanceName}";

    private static string ParseInstanceName(string connectionString)
    {
        // Parse "memory://name=foo" to extract "foo", default to empty string
        if (string.IsNullOrEmpty(connectionString) || connectionString == "memory://")
        {
            return string.Empty;
        }

        // Simple parsing: look for "name=" parameter
        var namePrefix = "name=";
        var startIndex = connectionString.IndexOf(namePrefix, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return string.Empty;
        }

        var valueStart = startIndex + namePrefix.Length;
        var ampersandIndex = connectionString.IndexOf('&', valueStart);
        return ampersandIndex > 0
            ? connectionString.Substring(valueStart, ampersandIndex - valueStart)
            : connectionString.Substring(valueStart);
    }

    private static IBlobStorage GetOrCreateStorage(string connectionString)
    {
        var instanceName = ParseInstanceName(connectionString);
        return SharedStorageInstances.GetOrAdd(instanceName, _ => StorageFactory.Blobs.InMemory());
    }
}
