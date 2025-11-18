// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Interfaces;

/// <summary>
/// Defines a connection to a virtual file system.
/// </summary>
public interface IVfsConnection : IDisposable
{
    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    string ConnectionString { get; }
}
