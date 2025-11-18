// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.ZipFile;

/// <summary>
/// Represents a connection to a ZipFile virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be in the following format:
/// - "zip://path=path_to_file".
/// </remarks>
public class ZipFileVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public ZipFileVfsConnection(string connectionString)
        : base(
            VfsProviderType.ZipFile,
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
        var csParts = VfsProviderUtils.ParseConnectionString(
            connectionString,
            ["zip://"],
            out normalizedConnectionString
        );

        if (!csParts.TryGetFirstMatchingValue(["path", "file"], out var path) || string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'path' parameter.",
                nameof(connectionString)
            );
        }

        return StorageFactory.Blobs.ZipFile(path);
    }
}
