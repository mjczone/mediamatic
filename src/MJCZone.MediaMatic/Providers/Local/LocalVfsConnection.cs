// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Local;

/// <summary>
/// Represents a connection to a Local virtual file system.
/// </summary>
/// <remarks>
/// The connection string should specify the path to the Local file and should be in the format:
/// "disk://path={path to local file}".
/// </remarks>
public class LocalVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the SFTP virtual file system.</param>
    public LocalVfsConnection(string connectionString)
        : base(
            VfsProviderType.Local,
            CreateBlobStorageFromConnectionString(connectionString, out var normalizedConnectionString)
        )
    {
        ConnectionString = normalizedConnectionString;
    }

    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    public override string ConnectionString { get; }

    private static IBlobStorage CreateBlobStorageFromConnectionString(
        string connectionString,
        out string normalizedConnectionString
    )
    {
        // If there is no prefix and there is no path parameter, treat the entire connection string as the path
        if (
            !connectionString.Contains("://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.Contains("path=", StringComparison.OrdinalIgnoreCase)
        )
        {
            connectionString = $"disk://path={connectionString}";
        }

        var csParts = VfsProviderUtils.ParseConnectionString(
            connectionString,
            ["disk://", "local://", "root://"],
            out normalizedConnectionString
        );

        return csParts.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path)
            ? StorageFactory.Blobs.DirectoryFiles(path)
            : throw new ArgumentException(
                "Connection string must contain a valid 'path' parameter.",
                nameof(connectionString)
            );
    }
}
