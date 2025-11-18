// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Minio;

/// <summary>
/// Represents a connection to a Minio virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be in one of the following format:
/// - "minio.s3://keyId=...;key=...;bucket=...;".
/// Other optional parameters include:
/// - "region=..." (default is "us-east-1").
/// - "serviceUrl=...".
/// </remarks>
public class MinioVfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MinioVfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public MinioVfsConnection(string connectionString)
        : base(
            VfsProviderType.Minio,
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
            ["minio.s3://", "s3://", "minio://"],
            out normalizedConnectionString
        );

        if (
            !csParts.TryGetFirstMatchingValue(["accessKeyId", "access_key_id", "keyId", "key_id"], out var accessKeyId)
            || string.IsNullOrWhiteSpace(accessKeyId)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'accessKeyId' parameter.",
                nameof(connectionString)
            );
        }

        if (
            !csParts.TryGetFirstMatchingValue(
                ["secretAccessKey", "secret_access_key", "secretKey", "secret_key", "secret", "key"],
                out var secretAccessKey
            ) || string.IsNullOrWhiteSpace(secretAccessKey)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'secretAccessKey' parameter.",
                nameof(connectionString)
            );
        }

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

        if (
            !csParts.TryGetFirstMatchingValue(["sessionToken", "session_token", "token"], out var sessionToken)
            || string.IsNullOrWhiteSpace(sessionToken)
        )
        {
            sessionToken = null;
        }

        if (!csParts.TryGetFirstMatchingValue(["region"], out var region) || string.IsNullOrWhiteSpace(region))
        {
            region = "us-east-1";
        }

        if (
            !csParts.TryGetFirstMatchingValue(
                ["serviceUrl", "service_url", "serverUrl", "server_url", "endpointUrl", "endpoint_url"],
                out var serviceUrl
            ) || string.IsNullOrWhiteSpace(serviceUrl)
        )
        {
            serviceUrl = null;
        }

        return StorageFactory.Blobs.MinIO(accessKeyId, secretAccessKey, bucketName, region, serviceUrl, sessionToken);
    }
}
