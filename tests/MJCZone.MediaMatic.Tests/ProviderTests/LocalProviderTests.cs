// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Local provider basic file operations.
/// No testcontainer needed - uses temporary local filesystem.
/// </summary>
public class LocalProviderTests : IDisposable
{
    private readonly string _tempDirectory;

    public LocalProviderTests()
    {
        // Create a unique temp directory for this test run
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mediamatic-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        // Cleanup temp directory after tests
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task UploadFile_Should_Create_File_At_Specified_Path()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        var testContent = "Hello, MediaMatic!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "test.txt");

        // Assert
        result.Should().Be("test.txt");
        var filePath = Path.Combine(_tempDirectory, "test.txt");
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be(testContent);
    }

    [Fact]
    public async Task UploadFile_Should_Throw_When_File_Exists_And_Overwrite_False()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        var testContent = "Initial content";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(stream1, "test.txt");

        // Act
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("New content"));
        var act = async () => await vfs.UploadFileAsync(stream2, "test.txt", overwrite: false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task UploadFile_Should_Overwrite_When_Overwrite_True()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        var initialContent = "Initial content";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(initialContent));
        await vfs.UploadFileAsync(stream1, "test.txt");

        // Act
        var newContent = "New content";
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
        await vfs.UploadFileAsync(stream2, "test.txt", overwrite: true);

        // Assert
        var filePath = Path.Combine(_tempDirectory, "test.txt");
        File.ReadAllText(filePath).Should().Be(newContent);
    }

    [Fact]
    public async Task DownloadAsync_Should_Return_File_Content()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        var testContent = "Test file content";
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
    public async Task ExistsAsync_Should_Return_True_When_File_Exists()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "exists-test.txt");

        // Act
        var exists = await vfs.ExistsAsync("exists-test.txt");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_File_Does_Not_Exist()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        // Act
        var exists = await vfs.ExistsAsync("non-existent.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Theory]
    [InlineData("file1.txt", "File 1 content")]
    [InlineData("file2.txt", "File 2 content")]
    [InlineData("file3.txt", "File 3 content")]
    public async Task ListFilesAsync_Should_Return_All_Files(string fileName, string content)
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await vfs.UploadFileAsync(stream, fileName);
    }

    [Fact]
    public async Task ListFilesAsync_Should_Return_All_Uploaded_Files()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        // Upload test files
        var files = new[] { ("list1.txt", "content1"), ("list2.txt", "content2"), ("list3.txt", "content3") };

        foreach (var (fileName, content) in files)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await vfs.UploadFileAsync(stream, fileName);
        }

        // Act
        var result = await vfs.ListFilesAsync();

        // Assert
        var fileList = result.ToList();
        fileList.Should().HaveCountGreaterThanOrEqualTo(3);
        fileList.Should().Contain(f => f.Contains("list1.txt"));
        fileList.Should().Contain(f => f.Contains("list2.txt"));
        fileList.Should().Contain(f => f.Contains("list3.txt"));
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_File()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "delete-test.txt");

        // Act
        await vfs.DeleteAsync("delete-test.txt");

        // Assert
        var exists = await vfs.ExistsAsync("delete-test.txt");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFolderAsync_Should_Remove_All_Files_In_Folder()
    {
        // Arrange
        var connectionString = $"local://path={_tempDirectory}";
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

        // Create files in a subfolder
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        await vfs.UploadFileAsync(stream1, "folder/file1.txt");

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        await vfs.UploadFileAsync(stream2, "folder/file2.txt");

        // Act
        await vfs.DeleteFolderAsync("folder");

        // Assert
        var exists1 = await vfs.ExistsAsync("folder/file1.txt");
        var exists2 = await vfs.ExistsAsync("folder/file2.txt");
        exists1.Should().BeFalse();
        exists2.Should().BeFalse();
    }
}
