// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Memory;

/// <summary>
/// Represents a connection to a Memory virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be empty or specify the path to the in-memory storage and should be in the format:
/// "inmemory://".
/// </remarks>
public class MemoryVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVfsConnection"/> class.
    /// </summary>
    public MemoryVfsConnection()
        : base(VfsProviderType.Memory, StorageFactory.Blobs.InMemory()) { }

    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    public override string ConnectionString => "inmemory://";
}
