// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for ZipFile provider basic file operations.
/// No testcontainer needed - creates zip files on disk.
/// </summary>
public class ZipFileProviderTests : IDisposable
{
    private readonly string _testZipPath;

    public ZipFileProviderTests()
    {
        // Create unique zip file for this test run
        _testZipPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");
    }

    public void Dispose()
    {
        // Clean up zip file after tests
        if (File.Exists(_testZipPath))
        {
            File.Delete(_testZipPath);
        }
    }

    [Fact]
    public async Task UploadFile_Should_Create_Entry_In_ZipFile()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        var testContent = "Hello, ZipFile Storage!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "test.txt");

        // Assert
        result.Should().Be("test.txt");

        // Verify file exists in zip
        var exists = await vfs.ExistsAsync("test.txt");
        exists.Should().BeTrue();

        // Dispose VFS connection to release file lock
        vfs.Dispose();

        // Verify content using ZipArchive
        using var zipArchive = ZipFile.OpenRead(_testZipPath);
        var entry = zipArchive.GetEntry("test.txt");
        entry.Should().NotBeNull();

        using var entryStream = entry!.Open();
        using var reader = new StreamReader(entryStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task DownloadAsync_Should_Return_Zip_Entry_Content()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        var testContent = "ZipFile download test";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, "download-test.txt");

        // Act
        using var downloadStream = await vfs.DownloadAsync("download-test.txt");
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Entry_Exists()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "exists-test.txt");

        // Act
        var exists = await vfs.ExistsAsync("exists-test.txt");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Entry_Does_Not_Exist()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        // Act
        var exists = await vfs.ExistsAsync("non-existent.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Theory]
    [InlineData("file1.txt", "Zip File 1")]
    [InlineData("file2.txt", "Zip File 2")]
    [InlineData("file3.txt", "Zip File 3")]
    public async Task Multiple_Files_Can_Be_Stored_In_Zip(string fileName, string content)
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        // Act
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await vfs.UploadFileAsync(stream, fileName);

        using var downloadStream = await vfs.DownloadAsync(fileName);
        using var reader = new StreamReader(downloadStream);
        var retrievedContent = await reader.ReadToEndAsync();

        // Assert
        retrievedContent.Should().Be(content);
    }

    [Fact]
    public async Task ListFilesAsync_Should_Include_All_Entries()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        // Upload multiple files
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        await vfs.UploadFileAsync(stream1, "list-test-1.txt");

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        await vfs.UploadFileAsync(stream2, "list-test-2.txt");

        // Act
        var files = await vfs.ListFilesAsync();

        // Assert
        files.Should().Contain("/list-test-1.txt");
        files.Should().Contain("/list-test-2.txt");
    }

    [Fact]
    public async Task Files_In_Subdirectories_Should_Be_Accessible()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        var testContent = "Nested content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        await vfs.UploadFileAsync(stream, "folder/subfolder/nested.txt");

        // Assert
        var exists = await vfs.ExistsAsync("folder/subfolder/nested.txt");
        exists.Should().BeTrue();

        using var downloadStream = await vfs.DownloadAsync("folder/subfolder/nested.txt");
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entry_From_Zip()
    {
        // Arrange
        var connectionString = $"zip://path={_testZipPath}";
        using var vfs = VfsConnection.Create(VfsProviderType.ZipFile, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "delete-test.txt");

        // Act
        await vfs.DeleteAsync("delete-test.txt");

        // Assert
        var exists = await vfs.ExistsAsync("delete-test.txt");
        exists.Should().BeFalse();
    }
}
