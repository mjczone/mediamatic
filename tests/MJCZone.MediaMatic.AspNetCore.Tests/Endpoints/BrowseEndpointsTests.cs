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
/// Integration tests for MediaMatic browse REST endpoints.
/// </summary>
public class BrowseEndpointsTests
{
    private static FilesourceDto CreateMemoryFilesource(
        string id = "test-memory",
        [System.Runtime.CompilerServices.CallerMemberName] string? testName = null
    ) =>
        new()
        {
            Id = id,
            Provider = "Memory",
            ConnectionString = $"memory://name=BrowseEndpointsTests_{testName ?? Guid.NewGuid().ToString()}",
            DisplayName = $"Test Memory Storage {id}",
            IsEnabled = true,
        };

    #region Browse Root Tests

    [Fact]
    public async Task Should_browse_root_directory_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create some files and folders
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/file1.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Content 1"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/file2.pdf",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Content 2"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/folder1/nested.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Nested content"))
        );

        // Browse root
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Folders.Should().NotBeNull();
        res.Result.Files.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_browse_nested_directory_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files in nested folder
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/documents/reports/report1.pdf",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Report 1"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/documents/reports/report2.pdf",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Report 2"))
        );

        // Browse nested folder
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/documents/reports");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Files.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Filter by Type Tests

    [Fact]
    public async Task Should_filter_by_files_only_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files and folders
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/file.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Content"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/folder/nested.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Nested"))
        );

        // Browse with type=files
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/?type=files");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Folders.Should().BeEmpty();
        res.Result.Files.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_filter_by_folders_only_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files in different folders
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/folder1/file.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Content 1"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/folder2/file.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Content 2"))
        );

        // Browse with type=folders
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/?type=folders");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Files.Should().BeEmpty();
        res.Result.Folders.Should().NotBeEmpty();
    }

    #endregion

    #region Wildcard Filter Tests

    [Fact]
    public async Task Should_filter_files_by_wildcard_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create various files
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/document.pdf",
            new ByteArrayContent(Encoding.UTF8.GetBytes("PDF content"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/image.jpg",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Image content"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/report.pdf",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Report content"))
        );

        // Browse with filter=*.pdf
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/?filter=*.pdf");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Files.Should().OnlyContain(f => f.Extension == ".pdf");
    }

    #endregion

    #region Recursive Browse Tests

    [Fact]
    public async Task Should_browse_recursively_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create nested structure
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/root.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Root"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/level1/l1.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Level 1"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/files/level1/level2/l2.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Level 2"))
        );

        // Browse recursively
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/browse/?recursive=true");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Files.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Bucket Browse Tests

    [Fact]
    public async Task Should_browse_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Create files in bucket
        await client.PostAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/files/file1.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Bucket content 1"))
        );
        await client.PostAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/files/file2.txt",
            new ByteArrayContent(Encoding.UTF8.GetBytes("Bucket content 2"))
        );

        // Browse bucket
        var browseResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/browse/");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await browseResponse.ReadAsJsonAsync<BrowseResponse>();
        res.Should().NotBeNull();
        res!.Result.Files.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var browseResponse = await client.GetAsync("/api/mm/fs/non-existent/browse/");
        browseResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
