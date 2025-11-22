// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.AspNetCore.Factories;

/// <summary>
/// Factory interface for creating vfs connections from filesource configurations.
/// </summary>
public interface IVfsConnectionFactory
{
    /// <summary>
    /// Creates a filesource connection for the specified provider and connection string.
    /// </summary>
    /// <param name="provider">The filesource provider.</param>
    /// <param name="connectionString">The connection string.</param>
    /// /// <returns>A configured filesource connection.</returns>
    IVfsConnection CreateConnection(string provider, string connectionString);
}
