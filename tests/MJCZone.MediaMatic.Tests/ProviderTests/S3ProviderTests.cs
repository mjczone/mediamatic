// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Tests.Fixtures;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for S3 provider basic file operations.
/// Uses shared LocalStack testcontainer via Collection Fixture.
/// </summary>
[Collection("S3Provider")]
public class S3ProviderTests : VfsProviderTestsBase, IAsyncLifetime
{
    private readonly LocalStackFixture _localStackFixture;
    private IAmazonS3? _s3Client;

    public S3ProviderTests(LocalStackFixture localStackFixture)
    {
        _localStackFixture = localStackFixture;
    }

    public async Task InitializeAsync()
    {
        // Create S3 client for test setup
        var config = new AmazonS3Config
        {
            ServiceURL = _localStackFixture.Container.GetConnectionString(),
            ForcePathStyle = true,
            // RegionEndpoint = Amazon.RegionEndpoint.USEast1,
        };

        _s3Client = new AmazonS3Client("test", "test", config);

        // Create test bucket
        try
        {
            await _s3Client.PutBucketAsync(LocalStackFixture.TestBucketName);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
        {
            // Bucket already exists, that's fine
        }
    }

    public Task DisposeAsync()
    {
        _s3Client?.Dispose();
        return Task.CompletedTask;
    }

    protected override Task<IVfsConnection> CreateConnectionAsync()
    {
        var connection = VfsConnection.Create(
            VfsProviderType.S3,
            _localStackFixture.S3ConnectionString
        );
        return Task.FromResult(connection);
    }

    #region Additional S3-Specific Tests

    [Fact]
    public async Task UploadFile_Should_Create_File_Verifiable_Via_AWS_SDK()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var testContent = "Hello, MediaMatic S3!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var testKey = $"test-{Guid.NewGuid()}.txt";

        // Act
        var result = await vfs.UploadFileAsync(stream, testKey);

        // Assert - Verify via AWS SDK directly
        result.Should().Be(testKey);
        var response = await _s3Client!.GetObjectAsync(LocalStackFixture.TestBucketName, testKey);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    #endregion
}
