// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Testcontainers.Minio;

namespace MJCZone.MediaMatic.Tests.Fixtures;

/// <summary>
/// Fixture for Minio (S3-compatible) testcontainer.
/// Shared across all Minio provider tests using xUnit Collection Fixture.
/// </summary>
public class MinioFixture : IAsyncLifetime
{
    private MinioContainer? _container;

    /// <summary>
    /// Gets the Minio container.
    /// </summary>
    public MinioContainer Container => _container ?? throw new InvalidOperationException("Container not initialized");

    /// <summary>
    /// Gets the S3 connection string for this Minio instance.
    /// </summary>
    public string S3ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the default bucket name for tests.
    /// </summary>
    public const string TestBucketName = "test-bucket";

    /// <summary>
    /// Gets the default access key.
    /// </summary>
    public const string AccessKey = "minioadmin";

    /// <summary>
    /// Gets the default secret key.
    /// </summary>
    public const string SecretKey = "minioadmin";

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _container = new MinioBuilder()
            .WithImage("minio/minio:latest")
            .WithUsername(AccessKey)
            .WithPassword(SecretKey)
            .Build();

        await _container.StartAsync();

        // Build connection string for MediaMatic S3 provider
        S3ConnectionString =
            $"s3://bucketName={TestBucketName};serviceUrl={_container.GetConnectionString()};accessKey={AccessKey};secretKey={SecretKey}";
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
