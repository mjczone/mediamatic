// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.B2;

/// <summary>
/// Represents a connection to a B2 virtual file system.
/// </summary>
/// <remarks>
/// The connection string should specify the path to the B2 file and should be in the format:
/// "b2://keyId=...;key=...;bucket=...;root=...;endpoint_url=...".
///
/// Region is NOT provided for B2 connections.
/// </remarks>
public class B2VfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="B2VfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public B2VfsConnection(string connectionString)
        : base(
            VfsProviderType.B2,
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
            ["b2.s3://", "s3://", "b2://"],
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
            !csParts.TryGetFirstMatchingValue(
                ["serviceUrl", "service_url", "serverUrl", "server_url", "endpointUrl", "endpoint_url"],
                out var serviceUrl
            ) || string.IsNullOrWhiteSpace(serviceUrl)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'serviceUrl' parameter.",
                nameof(connectionString)
            );
        }

        var clientConfig = new Amazon.S3.AmazonS3Config { ServiceURL = serviceUrl, ForcePathStyle = true };
        return StorageFactory.Blobs.AwsS3(accessKeyId, secretAccessKey, null, bucketName, clientConfig);
    }
}
