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
/// Integration tests for MediaMatic file REST endpoints.
/// </summary>
public class FileEndpointsTests
{
    private static FilesourceDto CreateMemoryFilesource(
        string id = "test-memory",
        [System.Runtime.CompilerServices.CallerMemberName] string? testName = null
    ) =>
        new()
        {
            Id = id,
            Provider = "Memory",
            ConnectionString = $"memory://name=FileEndpointsTests_{testName ?? Guid.NewGuid().ToString()}",
            DisplayName = $"Test Memory Storage {id}",
            IsEnabled = true,
        };

    #region Upload and Download Tests

    [Fact]
    public async Task Should_upload_and_download_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Hello, MediaMatic!"));

        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/files/test.txt", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var uploadResult = await uploadResponse.ReadAsJsonAsync<FileUploadResponse>();
        uploadResult.Should().NotBeNull();
        uploadResult!.Path.Should().Be("test.txt");

        // Download the file
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/test.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var downloadedContent = await downloadResponse.Content.ReadAsStringAsync();
        downloadedContent.Should().Be("Hello, MediaMatic!");
    }

    [Fact]
    public async Task Should_upload_file_to_nested_path_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Nested file content"));

        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/files/docs/nested/file.txt", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var uploadResult = await uploadResponse.ReadAsJsonAsync<FileUploadResponse>();
        uploadResult.Should().NotBeNull();
        uploadResult!.Path.Should().Be("docs/nested/file.txt");

        // Verify download
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/docs/nested/file.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var downloadedContent = await downloadResponse.Content.ReadAsStringAsync();
        downloadedContent.Should().Be("Nested file content");
    }

    [Fact]
    public async Task Should_overwrite_existing_file_with_PUT_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload initial file
        var initialContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Initial content"));
        await client.PostAsync("/api/mm/fs/test-memory/files/overwrite.txt", initialContent);

        // Overwrite with PUT
        var newContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Updated content"));

        var overwriteResponse = await client.PutAsync("/api/mm/fs/test-memory/files/overwrite.txt", newContent);
        overwriteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify new content
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/overwrite.txt");
        var downloadedContent = await downloadResponse.Content.ReadAsStringAsync();
        downloadedContent.Should().Be("Updated content");
    }

    [Fact]
    public async Task Should_download_file_with_download_query_param_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Download me!"));
        await client.PostAsync("/api/mm/fs/test-memory/files/download-test.txt", content);

        // Download with ?download=true
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/download-test.txt?download=true");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check Content-Disposition header
        downloadResponse.Content.Headers.ContentDisposition.Should().NotBeNull();
        downloadResponse.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Should_delete_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("To be deleted"));
        await client.PostAsync("/api/mm/fs/test-memory/files/delete-me.txt", content);

        // Delete the file
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/files/delete-me.txt");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify file is gone
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/delete-me.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_when_deleting_non_existent_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/files/does-not-exist.txt");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region File Exists Tests

    [Fact]
    public async Task Should_check_if_file_exists_using_HEAD_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Exists test"));
        await client.PostAsync("/api/mm/fs/test-memory/files/exists.txt", content);

        // Check if file exists
        var headRequest = new HttpRequestMessage(HttpMethod.Head, "/api/mm/fs/test-memory/files/exists.txt");
        var headResponse = await client.SendAsync(headRequest);
        headResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check for non-existent file
        var headRequest2 = new HttpRequestMessage(HttpMethod.Head, "/api/mm/fs/test-memory/files/not-exists.txt");
        var headResponse2 = await client.SendAsync(headRequest2);
        headResponse2.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var downloadResponse = await client.GetAsync("/api/mm/fs/non-existent/files/test.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_when_downloading_non_existent_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/does-not-exist.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Bucket Tests

    [Fact]
    public async Task Should_upload_and_download_file_in_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file to a bucket
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Bucket file content"));

        var uploadResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/files/bucket-file.txt",
            content
        );
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Download the file from the bucket
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/files/bucket-file.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var downloadedContent = await downloadResponse.Content.ReadAsStringAsync();
        downloadedContent.Should().Be("Bucket file content");
    }

    [Fact]
    public async Task Should_delete_file_from_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a file to a bucket
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("Delete from bucket"));
        await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/files/delete-bucket.txt", content);

        // Delete the file
        var deleteResponse = await client.DeleteAsync("/api/mm/fs/test-memory/bu/my-bucket/files/delete-bucket.txt");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify file is gone
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/my-bucket/files/delete-bucket.txt");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
