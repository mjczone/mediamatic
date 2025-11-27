// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using System.Text;
using FluentAssertions;
using MJCZone.MediaMatic.AspNetCore.Endpoints;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Tests.Factories;
using MJCZone.MediaMatic.AspNetCore.Tests.Infrastructure;

namespace MJCZone.MediaMatic.AspNetCore.Tests.Endpoints;

/// <summary>
/// Integration tests for MediaMatic folder REST endpoints.
/// </summary>
public class FolderEndpointsTests
{
    private static FilesourceDto CreateMemoryFilesource(
        string id = "test-memory",
        [System.Runtime.CompilerServices.CallerMemberName] string? testName = null
    ) =>
        new()
        {
            Id = id,
            Provider = "Memory",
            ConnectionString = $"memory://name=FolderEndpointsTests_{testName ?? Guid.NewGuid().ToString()}",
            DisplayName = $"Test Memory Storage {id}",
            IsEnabled = true,
        };

    #region List Folders Tests

    [Fact]
    public async Task Should_list_folders_at_root_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create some files in different folders to ensure folders exist
        var folders = new[] { "folder1", "folder2", "folder3" };
        foreach (var folder in folders)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes($"Content in {folder}"));
            await client.PostAsync($"/api/mm/fs/test-memory/fi/{folder}/test.txt", content);
        }

        // List folders at root
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.ReadAsJsonAsync<FolderListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Folders.Should().NotBeNull();
        listResult.Folders.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Should_list_folders_in_nested_path_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create nested folder structure
        var paths = new[] { "parent/child1", "parent/child2", "parent/child3" };
        foreach (var path in paths)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes($"Content in {path}"));
            await client.PostAsync($"/api/mm/fs/test-memory/fi/{path}/test.txt", content);
        }

        // List folders in parent
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/parent?type=folders");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.ReadAsJsonAsync<FolderListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Folders.Should().NotBeNull();
        listResult.Folders.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region List Files in Folder Tests

    [Fact]
    public async Task Should_list_files_in_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files in a folder
        var files = new[] { "file1.txt", "file2.txt", "file3.txt" };
        foreach (var file in files)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes($"Content of {file}"));
            await client.PostAsync($"/api/mm/fs/test-memory/fi/documents/{file}", content);
        }

        // List files in folder
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/documents?type=files");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.ReadAsJsonAsync<FileListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Files.Should().NotBeNull();
        listResult.Files.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Should_list_files_in_folder_without_type_parameter_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files in a folder
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Default type test"));
        await client.PostAsync("/api/mm/fs/test-memory/fi/myfiles/test.txt", content);

        // List contents without type parameter (should default to files)
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/myfiles");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.ReadAsJsonAsync<FileListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Files.Should().NotBeNull();
    }

    #endregion

    #region Delete Folder Tests

    [Fact]
    public async Task Should_delete_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create a folder with files
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("File in folder to delete"));
        await client.PostAsync("/api/mm/fs/test-memory/fi/to-delete/file.txt", content);

        // Delete the folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/fo/to-delete");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify folder is gone by trying to list it
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/to-delete");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_delete_nested_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create nested folder structure
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Nested file"));
        await client.PostAsync("/api/mm/fs/test-memory/fi/parent/child/file.txt", content);

        // Delete child folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/fo/parent/child");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify child folder is gone
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/parent/child");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var listResponse = await client.GetAsync("/api/mm/fs/non-existent/fo/");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_when_listing_non_existent_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/fo/does-not-exist");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_when_deleting_non_existent_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/fo/does-not-exist");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Bucket Tests

    [Fact]
    public async Task Should_list_folders_in_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create folders in bucket
        var folders = new[] { "bucket-folder1", "bucket-folder2" };
        foreach (var folder in folders)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes($"Content in bucket {folder}"));
            await client.PostAsync($"/api/mm/fs/test-memory/bu/my-bucket/fi/{folder}/test.txt", content);
        }

        // List folders in bucket
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/fo/");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.ReadAsJsonAsync<FolderListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Folders.Should().NotBeNull();
        listResult.Folders.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_delete_folder_from_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create folder in bucket
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Bucket folder to delete"));
        await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/fi/delete-folder/file.txt", content);

        // Delete the folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/bu/my-bucket/fo/delete-folder");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify folder is gone
        var listResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/fo/delete-folder");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
