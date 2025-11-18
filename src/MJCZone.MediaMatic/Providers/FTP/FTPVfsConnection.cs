// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using FluentFTP;
using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.FTP;

/// <summary>
/// Represents a connection to a FTP virtual file system.
/// </summary>
/// <remarks>
/// The connection string should specify the path to the FTP file and should be in the format:
/// "ftp://host=...;user=...;password=...".
/// </remarks>
public class FTPVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FTPVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public FTPVfsConnection(string connectionString)
        : base(
            VfsProviderType.FTP,
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
            ["ftp://"],
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

        if (
            !csParts.TryGetFirstMatchingValue(["password", "pwd", "pass"], out var password)
            || string.IsNullOrWhiteSpace(password)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'password' parameter.",
                nameof(connectionString)
            );
        }

        var port =
            csParts.TryGetValue("port", out var portValue) && int.TryParse(portValue, out var portAsInt)
                ? portAsInt
                : 0;

        // return StorageFactory.Blobs.Ftp("myhost.com", new NetworkCredential("username", "password"));

#pragma warning disable CA2000 // Dispose objects before losing scope
        var ftpClient = new AsyncFtpClient(host, new NetworkCredential(userName, password), port);
#pragma warning restore CA2000 // Dispose objects before losing scope

        // Set some common configuration options
        ftpClient.Config.EncryptionMode = FtpEncryptionMode.None;
        ftpClient.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
        ftpClient.Config.ValidateAnyCertificate = false;

        if (csParts.TryGetFirstMatchingValue(["path", "root"], out var path) && !string.IsNullOrWhiteSpace(path))
        {
            ftpClient.SetWorkingDirectory(path);
        }

        return StorageFactory.Blobs.FtpFromFluentFtpClient(ftpClient);
    }
}
