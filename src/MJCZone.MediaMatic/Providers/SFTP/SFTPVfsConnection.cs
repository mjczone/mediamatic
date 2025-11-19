// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;
using Renci.SshNet;

namespace MJCZone.MediaMatic.Providers.SFTP;

/// <summary>
/// Represents a connection to a SFTP virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be in one of the following formats:
/// - "sftp://host=...;port=...;user=...;password=..."
/// - "sftp://host=...;port=...;user=...;privatekey=...".
/// </remarks>
public class SFTPVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SFTPVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public SFTPVfsConnection(string connectionString)
        : base(
            VfsProviderType.SFTP,
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
            ["sftp://"],
            out normalizedConnectionString
        );

        if (
            !csParts.TryGetFirstMatchingValue(["host", "hostname", "server"], out var host)
            || string.IsNullOrWhiteSpace(host)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'host' parameter.",
                nameof(connectionString)
            );
        }

        if (
            !csParts.TryGetFirstMatchingValue(["user", "username", "usr"], out var userName)
            || string.IsNullOrWhiteSpace(userName)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'user' parameter.",
                nameof(connectionString)
            );
        }

        // attempt to get private key path
        string keyFile = string.Empty;
        if (
            !csParts.TryGetFirstMatchingValue(["password", "pwd", "pass"], out var password)
            || string.IsNullOrWhiteSpace(password)
        )
        {
            if (
                csParts.TryGetFirstMatchingValue(
                    ["privatekey", "keyfile", "keypath", "key", "privateKeyFile", "private_key"],
                    out var keyPath
                ) && !string.IsNullOrWhiteSpace(keyPath)
            )
            {
                keyFile = keyPath;
            }
        }
        if (string.IsNullOrWhiteSpace(keyFile) && string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'password' or 'privatekey' parameter.",
                nameof(connectionString)
            );
        }

        var port =
            csParts.TryGetValue("port", out var portValue) && int.TryParse(portValue, out var portAsInt)
                ? portAsInt
                : 22;

#pragma warning disable CA2000 // Dispose objects before losing scope
        SftpClient sftpClient = !string.IsNullOrWhiteSpace(keyFile)
            ? new SftpClient(host, port, userName, new PrivateKeyFile(keyFile))
            : new SftpClient(host, port, userName, password!);
#pragma warning restore CA2000 // Dispose objects before losing scope

        return StorageFactory.Blobs.Sftp(sftpClient, true);
    }
}
