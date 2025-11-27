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
/// Tests for Minio provider (S3-compatible) basic file operations.
/// Uses shared Minio testcontainer via Collection Fixture.
/// </summary>
[Collection("MinioProvider")]
public class MinioProviderTests : VfsProviderTestsBase, IAsyncLifetime
{
    private readonly MinioFixture _minioFixture;
    private IAmazonS3? _s3Client;

    public MinioProviderTests(MinioFixture minioFixture)
    {
        _minioFixture = minioFixture;
    }

    public async Task InitializeAsync()
    {
        // Create S3 client for test setup
        var config = new AmazonS3Config
        {
            ServiceURL = _minioFixture.Container.GetConnectionString(),
            ForcePathStyle = true,
        };

        _s3Client = new AmazonS3Client(MinioFixture.AccessKey, MinioFixture.SecretKey, config);

        // Create test bucket
        try
        {
            await _s3Client.PutBucketAsync(MinioFixture.TestBucketName);
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
        var connection = VfsConnection.Create(VfsProviderType.S3, _minioFixture.S3ConnectionString);
        return Task.FromResult(connection);
    }

    #region Additional Minio-Specific Tests

    [Fact]
    public async Task UploadFile_Should_Create_File_Verifiable_Via_AWS_SDK()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var testContent = "Hello, MediaMatic Minio!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var testKey = $"test-{Guid.NewGuid()}.txt";

        // Act
        var result = await vfs.UploadFileAsync(stream, testKey);

        // Assert - Verify via AWS SDK directly
        result.Should().Be(testKey);
        var response = await _s3Client!.GetObjectAsync(MinioFixture.TestBucketName, testKey);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task Large_File_Upload_Should_Work()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var testKey = $"large-{Guid.NewGuid()}.bin";
        var largeData = new byte[10 * 1024 * 1024]; // 10MB
        new Random().NextBytes(largeData);

        using var stream = new MemoryStream(largeData);

        // Act
        await vfs.UploadFileAsync(stream, testKey);

        // Assert
        var exists = await vfs.ExistsAsync(testKey);
        exists.Should().BeTrue();

        using var downloadStream = await vfs.DownloadAsync(testKey);
        using var ms = new MemoryStream();
        await downloadStream.CopyToAsync(ms);
        ms.ToArray().Should().Equal(largeData);
    }

    #endregion
}
