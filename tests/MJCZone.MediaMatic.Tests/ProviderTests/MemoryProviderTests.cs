// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Memory provider basic file operations.
/// No testcontainer needed - uses in-memory storage.
/// </summary>
public class MemoryProviderTests
{
    [Fact]
    public async Task UploadFile_Should_Store_File_In_Memory()
    {
        // Arrange
        var connectionString = "memory://";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, connectionString);

        var testContent = "Hello, In-Memory Storage!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "test.txt");

        // Assert
        result.Should().Be("test.txt");
        var exists = await vfs.ExistsAsync("test.txt");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_Should_Return_Stored_Content()
    {
        // Arrange
        var connectionString = "memory://";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, connectionString);

        var testContent = "Memory download test";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, "download-test.txt");

        // Act
        using var downloadStream = await vfs.DownloadAsync("download-test.txt");
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Theory]
    [InlineData("file1.txt", "Content 1")]
    [InlineData("file2.txt", "Content 2")]
    [InlineData("file3.txt", "Content 3")]
    public async Task Multiple_Files_Can_Be_Stored_And_Retrieved(string fileName, string content)
    {
        // Arrange
        var connectionString = "memory://";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, connectionString);

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
    public async Task DeleteAsync_Should_Remove_File_From_Memory()
    {
        // Arrange
        var connectionString = "memory://";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "delete-test.txt");

        // Act
        await vfs.DeleteAsync("delete-test.txt");

        // Assert
        var exists = await vfs.ExistsAsync("delete-test.txt");
        exists.Should().BeFalse();
    }
}
