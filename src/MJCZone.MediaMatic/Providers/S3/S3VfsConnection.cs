// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Amazon.S3;
using FluentStorage;
using FluentStorage.Blobs;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.S3;

/// <summary>
/// Represents a connection to a S3 virtual file system.
/// </summary>
/// <remarks>
/// The connection string should be in one of the following format:
/// - "aws.s3://keyId=...;key=...;bucket=...;".
/// Other optional parameters include:
/// - "region=..." (default is "us-east-1").
/// - "serviceUrl=...".
/// </remarks>
public class S3VfsConnection : VfsConnectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S3VfsConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the virtual file system.</param>
    public S3VfsConnection(string connectionString)
        : base(
            VfsProviderType.S3,
            CreateBlobStorageFromConnectionString(connectionString, out var normalizedConnectionString, out var s3Client, out var bucketName)
        )
    {
        ConnectionString = normalizedConnectionString;
        S3Client = s3Client;
        BucketName = bucketName;
    }

    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    public override string ConnectionString { get; }

    /// <summary>
    /// Gets the underlying AWS S3 client for direct SDK access.
    /// </summary>
    /// <remarks>
    /// Exposed for operations where FluentStorage has bugs or limitations.
    /// See: https://github.com/robinrodricks/FluentStorage/issues/120
    /// </remarks>
    public IAmazonS3 S3Client { get; }

    /// <summary>
    /// Gets the S3 bucket name.
    /// </summary>
    public string BucketName { get; }

    private static IBlobStorage CreateBlobStorageFromConnectionString(
        string connectionString,
        out string normalizedConnectionString,
        out IAmazonS3 s3Client,
        out string bucketName
    )
    {
        var csParts = VfsProviderUtils.ParseConnectionString(
            connectionString,
            ["aws.s3://", "s3://", "aws://"],
            out normalizedConnectionString
        );

        if (
            !csParts.TryGetFirstMatchingValue(
                ["accessKeyId", "access_key_id", "keyId", "key_id", "accessKey", "access_key"],
                out var accessKeyId
            ) || string.IsNullOrWhiteSpace(accessKeyId)
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
            !csParts.TryGetFirstMatchingValue(["sessionToken", "session_token", "token"], out var sessionToken)
            || string.IsNullOrWhiteSpace(sessionToken)
        )
        {
            sessionToken = null;
        }

        if (
            !csParts.TryGetFirstMatchingValue(["bucketName", "bucket"], out var bucketNameValue)
            || string.IsNullOrWhiteSpace(bucketNameValue)
        )
        {
            throw new ArgumentException(
                "Connection string must contain a valid 'bucketName' parameter.",
                nameof(connectionString)
            );
        }

        bucketName = bucketNameValue;

        // ------------------------------------------------
        // RegionEndpoint and ServiceURL are mutually exclusive properties. Whichever property is set last will cause the other to automatically be reset to null.
        // 'RegionEndpoint' is not nullable aware.
        // ------------------------------------------------

        if (!csParts.TryGetFirstMatchingValue(["region"], out var region) || string.IsNullOrWhiteSpace(region))
        {
            region = null;
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

        // Throw if both region and serviceUrl are missing
        if (string.IsNullOrWhiteSpace(serviceUrl) && string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "Connection string must contain at least one of 'serviceUrl' or 'region' parameters.",
                nameof(connectionString)
            );
        }

        var clientConfig = new Amazon.S3.AmazonS3Config { ForcePathStyle = true };
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            clientConfig.ServiceURL = serviceUrl;
        }
        else
        {
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
        }

        // Create the AWS S3 client for direct SDK access (exposed for operations where FluentStorage has bugs)
        s3Client = string.IsNullOrWhiteSpace(sessionToken)
            ? new Amazon.S3.AmazonS3Client(accessKeyId, secretAccessKey, clientConfig)
            : new Amazon.S3.AmazonS3Client(accessKeyId, secretAccessKey, sessionToken, clientConfig);

        // Also create FluentStorage blob storage for operations that work properly
        return StorageFactory.Blobs.AwsS3(accessKeyId, secretAccessKey, sessionToken, bucketName, clientConfig);
    }
}
