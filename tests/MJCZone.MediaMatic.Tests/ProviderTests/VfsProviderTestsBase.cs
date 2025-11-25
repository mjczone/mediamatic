// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Base class for VFS provider tests ensuring consistent test coverage across all providers.
/// All providers (Memory, Local, S3, Minio, etc.) must pass these tests.
/// </summary>
public abstract class VfsProviderTestsBase
{
    /// <summary>
    /// Creates a VFS connection for the specific provider being tested.
    /// </summary>
    protected abstract Task<IVfsConnection> CreateConnectionAsync();

    #region Basic File Operations

    [Fact]
    public virtual async Task Should_Upload_File_To_Root()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        var testContent = "Hello, VFS!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "test.txt");

        // Assert
        result.Should().Be("test.txt");
        var exists = await vfs.ExistsAsync("test.txt");
        exists.Should().BeTrue();
    }

    [Fact]
    public virtual async Task Should_Upload_File_To_Nested_Path()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        var testContent = "Nested file content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "docs/nested/file.txt");

        // Assert
        result.Should().Be("docs/nested/file.txt");
        var exists = await vfs.ExistsAsync("docs/nested/file.txt");
        exists.Should().BeTrue();
    }

    [Fact]
    public virtual async Task Should_Download_File_From_Root()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        var testContent = "Download test content";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, "download.txt");

        // Act
        using var downloadStream = await vfs.DownloadAsync("download.txt");
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Fact]
    public virtual async Task Should_Download_File_From_Nested_Path()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        var testContent = "Nested download content";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(uploadStream, "docs/nested/download.txt");

        // Act
        using var downloadStream = await vfs.DownloadAsync("docs/nested/download.txt");
        using var reader = new StreamReader(downloadStream);
        var content = await reader.ReadToEndAsync();

        // Assert
        content.Should().Be(testContent);
    }

    [Fact]
    public virtual async Task Should_Delete_File()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "delete-test.txt");

        // Act
        await vfs.DeleteAsync("delete-test.txt");

        // Assert
        var exists = await vfs.ExistsAsync("delete-test.txt");
        exists.Should().BeFalse();
    }

    [Fact]
    public virtual async Task Should_Check_File_Exists()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await vfs.UploadFileAsync(stream, "exists-test.txt");

        // Act
        var exists = await vfs.ExistsAsync("exists-test.txt");
        var notExists = await vfs.ExistsAsync("does-not-exist.txt");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    #endregion

    #region File Listing Operations

    [Fact]
    public virtual async Task Should_List_Files_At_Root()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "file1.txt", "Content 1");
        await UploadTestFileAsync(vfs, "file2.txt", "Content 2");
        await UploadTestFileAsync(vfs, "file3.txt", "Content 3");

        // Act
        var files = await vfs.ListFilesAsync();

        // Assert
        files.Should().HaveCountGreaterThanOrEqualTo(3);
        files.Should().Contain(f => f.Contains("file1.txt"));
        files.Should().Contain(f => f.Contains("file2.txt"));
        files.Should().Contain(f => f.Contains("file3.txt"));
    }

    [Fact]
    public virtual async Task Should_List_Files_In_Folder()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "documents/doc1.txt", "Doc 1");
        await UploadTestFileAsync(vfs, "documents/doc2.txt", "Doc 2");
        await UploadTestFileAsync(vfs, "documents/doc3.txt", "Doc 3");
        await UploadTestFileAsync(vfs, "other/file.txt", "Other"); // Should not appear

        // Act
        var files = await vfs.ListFilesAsync("documents");

        // Assert
        files.Should().HaveCountGreaterThanOrEqualTo(3);
        files.Should().Contain(f => f.Contains("doc1.txt"));
        files.Should().Contain(f => f.Contains("doc2.txt"));
        files.Should().Contain(f => f.Contains("doc3.txt"));
    }

    #endregion

    #region Folder Operations

    [Fact]
    public virtual async Task Should_List_Folders_At_Root()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "folder1/test.txt", "Content 1");
        await UploadTestFileAsync(vfs, "folder2/test.txt", "Content 2");
        await UploadTestFileAsync(vfs, "folder3/test.txt", "Content 3");

        // Act
        var folders = await vfs.ListFoldersAsync();

        // Assert
        folders.Should().HaveCountGreaterThanOrEqualTo(3);
        folders.Should().Contain(f => f.Contains("folder1"));
        folders.Should().Contain(f => f.Contains("folder2"));
        folders.Should().Contain(f => f.Contains("folder3"));
    }

    [Fact]
    public virtual async Task Should_List_Folders_In_Nested_Path()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "parent/child1/test.txt", "Content 1");
        await UploadTestFileAsync(vfs, "parent/child2/test.txt", "Content 2");
        await UploadTestFileAsync(vfs, "parent/child3/test.txt", "Content 3");

        // Act
        var folders = await vfs.ListFoldersAsync("parent");

        // Assert
        folders.Should().HaveCountGreaterThanOrEqualTo(3);
        folders.Should().Contain(f => f.Contains("child1"));
        folders.Should().Contain(f => f.Contains("child2"));
        folders.Should().Contain(f => f.Contains("child3"));
    }

    [Fact]
    public virtual async Task Should_Delete_Folder()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "to-delete/file1.txt", "Content 1");
        await UploadTestFileAsync(vfs, "to-delete/file2.txt", "Content 2");

        // Act
        await vfs.DeleteFolderAsync("to-delete");

        // Assert
        var exists = await vfs.ExistsAsync("to-delete/file1.txt");
        exists.Should().BeFalse();
    }

    [Fact]
    public virtual async Task Should_Delete_Nested_Folder()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();
        await UploadTestFileAsync(vfs, "parent/child/file.txt", "Content");

        // Act
        await vfs.DeleteFolderAsync("parent/child");

        // Assert
        var exists = await vfs.ExistsAsync("parent/child/file.txt");
        exists.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static async Task UploadTestFileAsync(IVfsConnection vfs, string path, string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await vfs.UploadFileAsync(stream, path);
    }

    #endregion
}
