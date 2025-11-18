// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Testcontainers.LocalStack;

namespace MJCZone.MediaMatic.Tests.Fixtures;

/// <summary>
/// Fixture for LocalStack (S3-compatible) testcontainer.
/// Shared across all S3 provider tests using xUnit Collection Fixture.
/// </summary>
public class LocalStackFixture : IAsyncLifetime
{
    private LocalStackContainer? _container;

    /// <summary>
    /// Gets the LocalStack container.
    /// </summary>
    public LocalStackContainer Container =>
        _container ?? throw new InvalidOperationException("Container not initialized");

    /// <summary>
    /// Gets the S3 connection string for this LocalStack instance.
    /// </summary>
    public string S3ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the default bucket name for tests.
    /// </summary>
    public const string TestBucketName = "test-bucket";

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _container = new LocalStackBuilder().WithImage("localstack/localstack:latest").Build();

        await _container.StartAsync();

        // Build connection string for MediaMatic S3 provider
        S3ConnectionString =
            $"s3://bucketName={TestBucketName};serviceUrl={_container.GetConnectionString()};accessKey=test;secretKey=test";
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
