// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Providers;
using MJCZone.MediaMatic.Tests.Fixtures;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for SFTP provider basic file operations.
/// Uses shared SFTP testcontainer via Collection Fixture.
/// </summary>
/// <remarks>
/// See: https://hub.docker.com/r/atmoz/sftp/
/// </remarks>
[Collection("SftpProvider")]
public class SftpProviderTests
{
    private readonly SftpFixture _sftpFixture;

    public SftpProviderTests(SftpFixture sftpFixture)
    {
        _sftpFixture = sftpFixture;
    }

    [Fact]
    public async Task UploadFile_Should_Create_File_On_SFTP_Server()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var testContent = "Hello, MediaMatic SFTP!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var testFileName = $"upload/test-{Guid.NewGuid()}.txt";

        // Act
        var result = await vfs.UploadFileAsync(stream, testFileName);

        // Assert
        result.Should().Be(testFileName);

        // Verify file exists
        var exists = await vfs.ExistsAsync(testFileName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_Should_Return_SFTP_File_Content()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var testContent = "SFTP download test content";
        var testFileName = $"upload/download-{Guid.NewGuid()}.txt";

        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, testFileName);

        // Act
        using var downloadStream = await vfs.DownloadAsync(testFileName);
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_File_Exists()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var testFileName = $"upload/exists-{Guid.NewGuid()}.txt";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, testFileName);

        // Act
        var exists = await vfs.ExistsAsync(testFileName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_File_Does_Not_Exist()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        // Act
        var exists = await vfs.ExistsAsync($"upload/non-existent-{Guid.NewGuid()}.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Theory]
    [InlineData("file1.txt", "SFTP File 1")]
    [InlineData("file2.txt", "SFTP File 2")]
    [InlineData("file3.txt", "SFTP File 3")]
    public async Task Multiple_Files_Can_Be_Stored_And_Retrieved(string fileName, string content)
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var uniqueFileName = $"upload/{Guid.NewGuid()}-{fileName}";

        // Act
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await vfs.UploadFileAsync(stream, uniqueFileName);

        using var downloadStream = await vfs.DownloadAsync(uniqueFileName);
        using var reader = new StreamReader(downloadStream);
        var retrievedContent = await reader.ReadToEndAsync();

        // Assert
        retrievedContent.Should().Be(content);
    }

    [Fact]
    public async Task ListFilesAsync_Should_Include_Uploaded_Files()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var fileName1 = $"/upload/list-{Guid.NewGuid()}-1.txt";
        var fileName2 = $"/upload/list-{Guid.NewGuid()}-2.txt";

        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        await vfs.UploadFileAsync(stream1, fileName1);

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        await vfs.UploadFileAsync(stream2, fileName2);

        // Act
        var files = await vfs.ListFilesAsync("/upload");

        // Assert
        files.Should().Contain(fileName1);
        files.Should().Contain(fileName2);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_SFTP_File()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var testFileName = $"upload/delete-{Guid.NewGuid()}.txt";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, testFileName);

        // Act
        await vfs.DeleteAsync(testFileName);

        // Assert
        var exists = await vfs.ExistsAsync(testFileName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Files_In_Subdirectories_Should_Work()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var testContent = "Nested SFTP content";
        var nestedPath = $"upload/folder-{Guid.NewGuid()}/subfolder/nested.txt";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        await vfs.UploadFileAsync(stream, nestedPath);

        // Assert
        var exists = await vfs.ExistsAsync(nestedPath);
        exists.Should().BeTrue();

        using var downloadStream = await vfs.DownloadAsync(nestedPath);
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(testContent);
    }

    [Fact]
    public async Task DeleteFolderAsync_Should_Remove_All_Files_In_Folder()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.SFTP, _sftpFixture.SftpConnectionString);

        var folderName = $"upload/deletefolder-{Guid.NewGuid()}";

        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        await vfs.UploadFileAsync(stream1, $"{folderName}/file1.txt");

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        await vfs.UploadFileAsync(stream2, $"{folderName}/file2.txt");

        // Act
        await vfs.DeleteFolderAsync(folderName);

        // Assert
        var exists1 = await vfs.ExistsAsync($"{folderName}/file1.txt");
        var exists2 = await vfs.ExistsAsync($"{folderName}/file2.txt");
        exists1.Should().BeFalse();
        exists2.Should().BeFalse();
    }
}
