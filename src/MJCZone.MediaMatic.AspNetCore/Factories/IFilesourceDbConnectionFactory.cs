// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Data;

namespace MJCZone.MediaMatic.AspNetCore.Factories;

/// <summary>
/// Factory interface for creating database connections.
/// </summary>
public interface IFilesourceDbConnectionFactory
{
    /// <summary>
    /// Creates a database connection.
    /// </summary>
    /// <returns>A configured database connection.</returns>
    IDbConnection CreateConnection();
}
