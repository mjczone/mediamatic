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
using SkiaSharp;

namespace MJCZone.MediaMatic.AspNetCore.Tests.Endpoints;

/// <summary>
/// Integration tests for MediaMatic metadata REST endpoints.
/// </summary>
public class MetadataEndpointsTests
{
    private static FilesourceDto CreateMemoryFilesource(
        string id = "test-memory",
        [System.Runtime.CompilerServices.CallerMemberName] string? testName = null
    ) =>
        new()
        {
            Id = id,
            Provider = "Memory",
            ConnectionString = $"memory://name=MetadataEndpointsTests_{testName ?? Guid.NewGuid().ToString()}",
            DisplayName = $"Test Memory Storage {id}",
            IsEnabled = true,
        };

    /// <summary>
    /// Creates a simple test JPEG image for testing.
    /// </summary>
    private static byte[] CreateTestJpeg(int width = 800, int height = 600, int quality = 85)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Fill with gradient background
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [SKColors.Blue, SKColors.Green, SKColors.Yellow],
                SKShaderTileMode.Clamp
            ),
        };
        canvas.DrawRect(0, 0, width, height, paint);

        // Encode to JPEG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

        using var stream = new MemoryStream();
        data.SaveTo(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Creates a simple test PNG image for testing.
    /// </summary>
    private static byte[] CreateTestPng(int width = 640, int height = 480)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Fill with solid color
        canvas.Clear(SKColors.Coral);

        // Encode to PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = new MemoryStream();
        data.SaveTo(stream);
        return stream.ToArray();
    }

    #region Image Metadata Tests

    [Fact]
    public async Task Should_return_metadata_for_jpeg_image_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test JPEG image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/photo.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get metadata
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/photo.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();
        result!.Metadata.Should().NotBeNull();
        result.Metadata.Width.Should().Be(800);
        result.Metadata.Height.Should().Be(600);
        result.Metadata.MimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Should_return_metadata_for_png_image_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test PNG image
        var imageData = CreateTestPng(640, 480);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/image.png", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get metadata
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/image.png");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();
        result!.Metadata.Should().NotBeNull();
        result.Metadata.Width.Should().Be(640);
        result.Metadata.Height.Should().Be(480);
        result.Metadata.MimeType.Should().Be("image/png");
    }

    [Fact]
    public async Task Should_return_metadata_in_nested_path_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image to a nested path
        var imageData = CreateTestJpeg(1024, 768);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/photos/vacation/beach.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get metadata from nested path
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/photos/vacation/beach.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();
        result!.Metadata.Width.Should().Be(1024);
        result.Metadata.Height.Should().Be(768);
    }

    [Fact]
    public async Task Should_return_metadata_from_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image to a bucket
        var imageData = CreateTestJpeg(1920, 1080);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/bu/images-bucket/fi/banner.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get metadata from bucket
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/bu/images-bucket/metadata/banner.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();
        result!.Metadata.Width.Should().Be(1920);
        result.Metadata.Height.Should().Be(1080);
        result.Metadata.MimeType.Should().Be("image/jpeg");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Should_return_not_found_for_non_existent_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/does-not-exist.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var metadataResponse = await client.GetAsync("/api/mm/fs/non-existent-fs/metadata/test.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Non-Media File Tests

    [Fact]
    public async Task Should_return_basic_metadata_for_text_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a text file
        var textContent = "Hello, this is a test file!";
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(textContent));
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/readme.txt", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get metadata for non-media file
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/readme.txt");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();
        result!.Metadata.Should().NotBeNull();

        // Non-media files should return basic metadata
        // Note: MimeDetective uses content-based detection and may return octet-stream for plain text
        result.Metadata.MimeType.Should().NotBeNullOrEmpty();
        result.Metadata.Size.Should().BeGreaterThan(0);
    }

    #endregion

    #region Provider Metadata Tests

    [Fact]
    public async Task Should_include_provider_in_metadata_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(400, 300);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/test.jpg", content);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"Upload failed with body: {uploadBody}");

        // Get metadata
        var metadataResponse = await client.GetAsync("/api/mm/fs/test-memory/metadata/test.jpg");
        metadataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await metadataResponse.ReadAsJsonAsync<FileMetadataResponse>();
        result.Should().NotBeNull();

        // Provider should be identified (MetadataExtractor for images)
        result!.Metadata.Provider.Should().NotBeNullOrEmpty();
    }

    #endregion
}
