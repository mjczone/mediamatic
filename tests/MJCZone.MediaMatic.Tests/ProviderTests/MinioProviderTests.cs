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
using MJCZone.MediaMatic.Providers;
using MJCZone.MediaMatic.Tests.Fixtures;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Minio provider (S3-compatible) basic file operations.
/// Uses shared Minio testcontainer via Collection Fixture.
/// </summary>
[Collection("MinioProvider")]
public class MinioProviderTests : IAsyncLifetime
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

    [Fact]
    public async Task UploadFile_Should_Create_File_In_Minio_Bucket()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var testContent = "Hello, MediaMatic Minio!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var testKey = $"test-{Guid.NewGuid()}.txt";

        // Act
        var result = await vfs.UploadFileAsync(stream, testKey);

        // Assert
        result.Should().Be(testKey);

        // Verify file exists in Minio
        var response = await _s3Client!.GetObjectAsync(MinioFixture.TestBucketName, testKey);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task DownloadAsync_Should_Return_Minio_Object_Content()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var testContent = "Minio download test content";
        var testKey = $"download-{Guid.NewGuid()}.txt";

        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, testKey);

        // Act
        using var downloadStream = await vfs.DownloadAsync(testKey);
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Object_Exists()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var testKey = $"exists-{Guid.NewGuid()}.txt";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, testKey);

        // Act
        var exists = await vfs.ExistsAsync(testKey);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Object_Does_Not_Exist()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        // Act
        var exists = await vfs.ExistsAsync($"non-existent-{Guid.NewGuid()}.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Theory]
    [InlineData("file1.txt", "Minio File 1")]
    [InlineData("file2.txt", "Minio File 2")]
    [InlineData("file3.txt", "Minio File 3")]
    public async Task ListFilesAsync_Should_Include_Uploaded_File(string fileName, string content)
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await vfs.UploadFileAsync(stream, uniqueFileName);

        // Act
        var files = await vfs.ListFilesAsync();

        // Assert
        files.Should().Contain(f => f.Contains(uniqueFileName));
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Minio_Object()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var testKey = $"delete-{Guid.NewGuid()}.txt";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, testKey);

        // Act
        await vfs.DeleteAsync(testKey);

        // Assert
        var exists = await vfs.ExistsAsync(testKey);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFolderAsync_Should_Remove_All_Objects_With_Prefix()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

        var folderPrefix = $"folder-{Guid.NewGuid()}";

        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        await vfs.UploadFileAsync(stream1, $"{folderPrefix}/file1.txt");

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        await vfs.UploadFileAsync(stream2, $"{folderPrefix}/file2.txt");

        // Act
        await vfs.DeleteFolderAsync(folderPrefix);

        // Assert
        var exists1 = await vfs.ExistsAsync($"{folderPrefix}/file1.txt");
        var exists2 = await vfs.ExistsAsync($"{folderPrefix}/file2.txt");
        exists1.Should().BeFalse();
        exists2.Should().BeFalse();
    }

    [Fact]
    public async Task Large_File_Upload_Should_Work()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.S3, _minioFixture.S3ConnectionString);

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
}
