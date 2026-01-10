// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using System.Text;
using FluentAssertions;
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

    #region Create Folder Tests

    [Fact]
    public async Task Should_create_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create a folder
        var createResponse = await client.PostAsync("/api/mm/fs/test-memory/folders/new-folder", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify folder exists by browsing
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/new-folder");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_create_nested_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create a nested folder
        var createResponse = await client.PostAsync("/api/mm/fs/test-memory/folders/parent/child/grandchild", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Should_return_conflict_when_creating_existing_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create folder with a file to ensure it exists
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Content"));
        await client.PostAsync("/api/mm/fs/test-memory/files/existing-folder/file.txt", content);

        // Try to create the same folder - should conflict
        var createResponse = await client.PostAsync("/api/mm/fs/test-memory/folders/existing-folder", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
        await client.PostAsync("/api/mm/fs/test-memory/files/to-delete/file.txt", content);

        // Delete the folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/folders/to-delete");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify folder is gone by browsing
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/to-delete?type=all");
        var browseResult = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        browseResult.Should().NotBeNull();
        browseResult!.Result.Folders.Should().BeEmpty();
        browseResult.Result.Files.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_delete_nested_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create nested folder structure
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Nested file"));
        await client.PostAsync("/api/mm/fs/test-memory/files/parent/child/file.txt", content);

        // Delete child folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/folders/parent/child");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify child folder is gone (browse the parent)
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/parent?type=folders");
        var browseResult = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        browseResult.Should().NotBeNull();
        browseResult!.Result.Folders.Should().NotContain(f => f.Name == "child");
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/api/mm/fs/non-existent/folders/test");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_when_deleting_non_existent_folder_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/folders/does-not-exist");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Bucket Tests

    [Fact]
    public async Task Should_create_folder_in_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create folder in bucket
        var createResponse = await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/folders/bucket-folder", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Should_delete_folder_from_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create folder in bucket
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Bucket folder to delete"));
        await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/files/delete-folder/file.txt", content);

        // Delete the folder
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/bu/my-bucket/folders/delete-folder");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify folder is gone
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/browse/delete-folder?type=all");
        var browseResult = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        browseResult.Should().NotBeNull();
        browseResult!.Result.Folders.Should().BeEmpty();
        browseResult.Result.Files.Should().BeEmpty();
    }

    #endregion
}
