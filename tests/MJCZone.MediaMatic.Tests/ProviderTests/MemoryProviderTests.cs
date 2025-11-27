// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Memory provider operations.
/// No testcontainer needed - uses in-memory storage.
/// </summary>
public class MemoryProviderTests : VfsProviderTestsBase
{
    protected override Task<IVfsConnection> CreateConnectionAsync()
    {
        var connectionString = "memory://";
        var connection = VfsConnection.Create(VfsProviderType.Memory, connectionString);
        return Task.FromResult(connection);
    }
}
