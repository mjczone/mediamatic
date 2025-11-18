// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.GCP;

/// <summary>
/// Represents a connection to a GCP virtual file system.
/// </summary>
/// <remarks>
/// The connection string should specify the path to the GCP file and should be in the format:
/// "google.storage://bucket=...;cred=...".
/// </remarks>
public class GCPVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GCPVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public GCPVfsConnection(string connectionString)
        : base(
            VfsProviderType.GCP,
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
            ["google.storage://", "gcp://"],
            out normalizedConnectionString
        );

        if (
            !csParts.TryGetFirstMatchingValue(["bucketName", "bucket"], out var bucketName)
            || string.IsNullOrWhiteSpace(bucketName)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'bucketName' parameter.",
                nameof(connectionString)
            );
        }

        /*
          The credentials file is expected to be a JSON file path containing the service account credentials.
          The file will look like this:

          {
            "type": "service_account",
            "project_id": "your-project-id",
            "private_key_id": "your-private-key-id",
            "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
            "client_email": "your-service-account-email",
            "client_id": "your-client-id",
            "auth_uri": "https://accounts.google.com/o/oauth2/auth",
            "token_uri": "https://oauth2.googleapis.com/token",
            "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
            "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/your-service-account-email"
          }
         */
        if (
            !csParts.TryGetFirstMatchingValue(["credentialsFilePath", "path", "cred"], out var credentialsFilePath)
            || string.IsNullOrWhiteSpace(credentialsFilePath)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'credentialsFilePath' parameter.",
                nameof(connectionString)
            );
        }

        return StorageFactory.Blobs.GoogleCloudStorageFromJsonFile(bucketName, credentialsFilePath);
    }
}
