// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Models.Responses;
using MJCZone.MediaMatic.AspNetCore.Tests.Factories;
using MJCZone.MediaMatic.AspNetCore.Tests.Infrastructure;

namespace MJCZone.MediaMatic.AspNetCore.Tests.Endpoints;

/// <summary>
/// Integration tests for MediaMatic filesource REST endpoints.
/// </summary>
public class FilesourceEndpointsTests
{
    #region Comprehensive Workflow Tests

    [Fact]
    public async Task Can_perform_complete_workflow_on_filesource_endpoints_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();
        const string testFilesourceId = "WorkflowTestFilesource";

        // 1. GET MULTI - List all filesources (should be empty initially)
        var listResponse1 = await client.GetAsync("/api/mm/fs/");
        listResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResult1 = await listResponse1.ReadAsJsonAsync<FilesourceListResponse>();
        listResult1.Should().NotBeNull();
        listResult1!.Result.Should().NotBeNull();
        listResult1.Result.Should().BeEmpty();

        // 2. GET SINGLE - Try to get non-existent filesource (should return 404)
        var getResponse1 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}");
        getResponse1.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 3. CREATE - Create a new filesource
        var createRequest = new FilesourceDto
        {
            Id = testFilesourceId,
            Provider = "Memory",
            ConnectionString = "memory://",
            DisplayName = "Workflow Test Filesource",
            Description = "A test filesource for workflow validation",
            Tags = ["test", "workflow"],
            IsEnabled = true,
        };
        var createResponse = await client.PostAsJsonAsync("/api/mm/fs/", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResult = await createResponse.ReadAsJsonAsync<FilesourceResponse>();
        createResult.Should().NotBeNull();
        createResult!.Result.Should().NotBeNull();
        createResult.Result!.Id.Should().Be(testFilesourceId);
        createResult.Result.DisplayName.Should().Be("Workflow Test Filesource");

        // 4. EXISTS - Check if filesource exists (should return 200 - found)
        var existsResponse1 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}/exists");
        existsResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        var existsResult1 = await existsResponse1.ReadAsJsonAsync<FilesourceExistsResponse>();
        existsResult1.Should().NotBeNull();
        existsResult1!.Result.Should().BeTrue();

        // 5. GET MULTI - List filesources again (should contain new filesource)
        var listResponse2 = await client.GetAsync("/api/mm/fs/");
        listResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResult2 = await listResponse2.ReadAsJsonAsync<FilesourceListResponse>();
        listResult2.Should().NotBeNull();
        listResult2!.Result.Should().NotBeNull();
        listResult2.Result.Should().HaveCount(1);
        listResult2.Result.Should().Contain(d => d.Id == testFilesourceId);

        // 6. GET SINGLE - Get the created filesource (should return filesource details)
        var getResponse2 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}");
        getResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult2 = await getResponse2.ReadAsJsonAsync<FilesourceResponse>();
        getResult2.Should().NotBeNull();
        getResult2!.Result.Should().NotBeNull();
        getResult2.Result!.Id.Should().Be(testFilesourceId);
        getResult2.Result.DisplayName.Should().Be("Workflow Test Filesource");
        getResult2.Result.Description.Should().Be("A test filesource for workflow validation");
        getResult2.Result.Tags.Should().Contain("test");
        getResult2.Result.Tags.Should().Contain("workflow");
        getResult2.Result.IsEnabled.Should().BeTrue();

        // 7. UPDATE (PUT) - Update the filesource completely
        var updateRequest = new FilesourceDto
        {
            Provider = "Memory",
            ConnectionString = "memory://updated",
            DisplayName = "Updated Workflow Test",
            Description = "Updated description for workflow test",
            Tags = ["updated", "test"],
            IsEnabled = false,
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/mm/fs/{testFilesourceId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateResult = await updateResponse.ReadAsJsonAsync<FilesourceResponse>();
        updateResult.Should().NotBeNull();
        updateResult!.Result.Should().NotBeNull();

        // 8. GET SINGLE - Get updated filesource (should show changes)
        var getResponse3 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}");
        getResponse3.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult3 = await getResponse3.ReadAsJsonAsync<FilesourceResponse>();
        getResult3.Should().NotBeNull();
        getResult3!.Result.Should().NotBeNull();
        getResult3.Result!.DisplayName.Should().Be("Updated Workflow Test");
        getResult3.Result.Description.Should().Be("Updated description for workflow test");
        getResult3.Result.Tags.Should().Contain("updated");
        getResult3.Result.Tags.Should().NotContain("workflow");
        getResult3.Result.IsEnabled.Should().BeFalse();

        // 9. PATCH - Partial update of the filesource
        var patchRequest = new FilesourceDto
        {
            DisplayName = "Partially Updated Test",
            IsEnabled = true, // Only update these fields
        };
        var patchResponse = await client.PatchAsJsonAsync($"/api/mm/fs/{testFilesourceId}", patchRequest);
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 10. GET SINGLE - Verify patch changes
        var getResponse4 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}");
        getResponse4.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult4 = await getResponse4.ReadAsJsonAsync<FilesourceResponse>();
        getResult4.Should().NotBeNull();
        getResult4!.Result.Should().NotBeNull();
        getResult4.Result!.DisplayName.Should().Be("Partially Updated Test");
        getResult4.Result.IsEnabled.Should().BeTrue();
        getResult4.Result.Description.Should().Be("Updated description for workflow test"); // Should remain from PUT

        // 11. DELETE - Delete the filesource
        var deleteResponse = await client.DeleteAsync($"/api/mm/fs/{testFilesourceId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 12. GET SINGLE - Try to get deleted filesource (should return 404)
        var getResponse5 = await client.GetAsync($"/api/mm/fs/{testFilesourceId}");
        getResponse5.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 13. GET MULTI - List filesources (should be empty again)
        var listResponse3 = await client.GetAsync("/api/mm/fs/");
        listResponse3.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResult3 = await listResponse3.ReadAsJsonAsync<FilesourceListResponse>();
        listResult3.Should().NotBeNull();
        listResult3!.Result.Should().NotBeNull();
        listResult3.Result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_handle_filesource_endpoints_filtering_and_search_Async()
    {
        var testFilesources = new List<FilesourceDto>
        {
            new()
            {
                Id = "memory-test",
                Provider = "Memory",
                ConnectionString = "memory://",
                DisplayName = "Test Memory Storage",
                Description = "In-memory test storage",
                Tags = ["test", "memory"],
                IsEnabled = true,
            },
            new()
            {
                Id = "local-test",
                Provider = "Local",
                ConnectionString = "file:///tmp/test",
                DisplayName = "Test Local Storage",
                Description = "Local filesystem storage",
                Tags = ["test", "local"],
                IsEnabled = true,
            },
        };

        using var factory = new WafWithInMemoryFilesourceRepository(testFilesources);
        using var client = factory.CreateClient();

        // Test basic listing with configured filesources
        var listResponse = await client.GetAsync("/api/mm/fs/");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResult = await listResponse.ReadAsJsonAsync<FilesourceListResponse>();
        listResult.Should().NotBeNull();
        listResult!.Result.Should().NotBeNull();
        listResult.Result.Should().HaveCount(2);
        listResult.Result.Should().Contain(fs => fs.Id == "memory-test");
        listResult.Result.Should().Contain(fs => fs.Id == "local-test");

        // Test filtering by name pattern
        var filterResponse = await client.GetAsync("/api/mm/fs/?filter=Memory");
        filterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var filterResult = await filterResponse.ReadAsJsonAsync<FilesourceListResponse>();
        filterResult.Should().NotBeNull();
        filterResult!.Result.Should().NotBeNull();
        filterResult.Result.Should().Contain(fs => fs.Id == "memory-test");
        filterResult.Result.Should().NotContain(fs => fs.Id == "local-test");

        // Test specific filesource retrieval
        var getResponse = await client.GetAsync("/api/mm/fs/memory-test");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult = await getResponse.ReadAsJsonAsync<FilesourceResponse>();
        getResult.Should().NotBeNull();
        getResult!.Result.Should().NotBeNull();
        getResult.Result!.Id.Should().Be("memory-test");
        getResult.Result.Provider.Should().Be("Memory");
        getResult.Result.DisplayName.Should().Be("Test Memory Storage");
    }

    [Fact]
    public async Task Should_handle_filesource_endpoints_auto_generated_id_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        // Test auto-generated GUID ID when not provided
        var createRequest = new FilesourceDto
        {
            // Id intentionally omitted
            Provider = "Memory",
            ConnectionString = "memory://",
            DisplayName = "Auto-Generated ID Test",
            IsEnabled = true,
        };

        var createResponse = await client.PostAsJsonAsync("/api/mm/fs/", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResult = await createResponse.ReadAsJsonAsync<FilesourceResponse>();
        createResult.Should().NotBeNull();
        createResult!.Result.Should().NotBeNull();
        createResult.Result!.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(createResult.Result.Id, out _).Should().BeTrue("Id should be a valid GUID");

        // Clean up
        await client.DeleteAsync($"/api/mm/fs/{createResult.Result.Id}");
    }

    #endregion

    #region Error Scenarios Tests

    [Fact]
    public async Task Should_handle_error_scenarios_for_filesource_endpoints_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        // Test operations on non-existent filesource
        const string nonExistentId = "NonExistentFilesource";

        // GET non-existent filesource
        var getNonExistentResponse = await client.GetAsync($"/api/mm/fs/{nonExistentId}");
        getNonExistentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // UPDATE non-existent filesource
        var updateRequest = new FilesourceDto { DisplayName = "Updated Non-Existent" };
        var updateNonExistentResponse = await client.PutAsJsonAsync($"/api/mm/fs/{nonExistentId}", updateRequest);
        updateNonExistentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // PATCH non-existent filesource
        var patchNonExistentResponse = await client.PatchAsJsonAsync($"/api/mm/fs/{nonExistentId}", updateRequest);
        patchNonExistentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // DELETE non-existent filesource
        var deleteNonExistentResponse = await client.DeleteAsync($"/api/mm/fs/{nonExistentId}");
        deleteNonExistentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Test duplicate filesource creation
        const string duplicateId = "DuplicateTest";
        var duplicateRequest = new FilesourceDto
        {
            Id = duplicateId,
            Provider = "Memory",
            ConnectionString = "memory://",
            DisplayName = "Duplicate Test",
        };

        // First creation should succeed
        var createResponse1 = await client.PostAsJsonAsync("/api/mm/fs/", duplicateRequest);
        createResponse1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second creation with same ID should conflict
        var createResponse2 = await client.PostAsJsonAsync("/api/mm/fs/", duplicateRequest);
        createResponse2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Clean up
        await client.DeleteAsync($"/api/mm/fs/{duplicateId}");
    }

    [Fact]
    public async Task Should_handle_filesource_endpoints_invalid_data_returns_bad_request_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        // Test invalid provider
        var invalidProviderRequest = new FilesourceDto
        {
            Id = "InvalidProvider",
            Provider = "InvalidProviderThatDoesNotExist", // Invalid provider (too long)
            ConnectionString = "memory://",
            DisplayName = "Invalid Provider Test",
        };
        var invalidProviderResponse = await client.PostAsJsonAsync("/api/mm/fs/", invalidProviderRequest);
        invalidProviderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test missing required fields
        var missingFieldsRequest = new FilesourceDto
        {
            Id = "MissingFields",
            // Provider missing
            // ConnectionString missing
            DisplayName = "Missing Fields Test",
        };
        var missingFieldsResponse = await client.PostAsJsonAsync("/api/mm/fs/", missingFieldsRequest);
        missingFieldsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test empty connection string
        var emptyConnectionRequest = new FilesourceDto
        {
            Id = "EmptyConnection",
            Provider = "Memory",
            ConnectionString = "", // Empty connection string
            DisplayName = "Empty Connection Test",
        };
        var emptyConnectionResponse = await client.PostAsJsonAsync("/api/mm/fs/", emptyConnectionRequest);
        emptyConnectionResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Connectivity Tests

    [Fact]
    public async Task Should_test_filesource_connectivity_Async()
    {
        var testFilesources = new List<FilesourceDto>
        {
            new()
            {
                Id = "memory-test",
                Provider = "Memory",
                ConnectionString = "memory://",
                DisplayName = "Test Memory Storage",
                IsEnabled = true,
            },
        };

        using var factory = new WafWithInMemoryFilesourceRepository(testFilesources);
        using var client = factory.CreateClient();

        // Test connectivity for existing filesource
        var testResponse = await client.GetAsync("/api/mm/fs/memory-test/test");
        testResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var testResult = await testResponse.ReadAsJsonAsync<FilesourceTestResponse>();
        testResult.Should().NotBeNull();
        testResult!.Result.Should().NotBeNull();
        testResult.Result!.FilesourceId.Should().Be("memory-test");
        testResult.Result.Connected.Should().BeTrue();
        testResult.Result.Provider.Should().Be("Memory");
        testResult.Result.FilesourceName.Should().Be("Test Memory Storage");
        testResult.Result.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Should_handle_test_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        // Test connectivity for non-existent filesource - should return 200 with Connected=false
        var testResponse = await client.GetAsync("/api/mm/fs/non-existent/test");
        testResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var testResult = await testResponse.ReadAsJsonAsync<FilesourceTestResponse>();
        testResult.Should().NotBeNull();
        testResult!.Result.Should().NotBeNull();
        testResult.Result!.FilesourceId.Should().Be("non-existent");
        testResult.Result.Connected.Should().BeFalse();
        testResult.Result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Provider-Specific Tests

    [Fact]
    public async Task Should_handle_filesource_endpoints_provider_specific_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        // Test creating filesources for different providers
        var providerTests = new[]
        {
            new { Provider = "Memory", ConnectionString = "memory://" },
            new { Provider = "Local", ConnectionString = "file:///tmp/test" },
            new { Provider = "S3", ConnectionString = "s3://test-bucket" },
        };

        var createdIds = new List<string>();

        try
        {
            foreach (var test in providerTests)
            {
                var request = new FilesourceDto
                {
                    Id = $"Test-{test.Provider}",
                    Provider = test.Provider,
                    ConnectionString = test.ConnectionString,
                    DisplayName = $"Test {test.Provider}",
                    IsEnabled = true,
                };

                var response = await client.PostAsJsonAsync("/api/mm/fs/", request);
                response
                    .StatusCode.Should()
                    .Be(HttpStatusCode.Created, $"Provider {test.Provider} should be supported");

                var result = await response.ReadAsJsonAsync<FilesourceResponse>();
                result.Should().NotBeNull();
                result!.Result.Should().NotBeNull();
                result.Result!.Provider.Should().Be(test.Provider);

                createdIds.Add(request.Id!);
            }
        }
        finally
        {
            // Clean up all created filesources
            foreach (var id in createdIds)
            {
                await client.DeleteAsync($"/api/mm/fs/{id}");
            }
        }
    }

    #endregion
}
