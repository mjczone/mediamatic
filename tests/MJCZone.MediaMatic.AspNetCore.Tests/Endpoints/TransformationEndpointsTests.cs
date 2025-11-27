// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using FluentAssertions;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Tests.Factories;
using MJCZone.MediaMatic.AspNetCore.Tests.Infrastructure;
using SkiaSharp;

namespace MJCZone.MediaMatic.AspNetCore.Tests.Endpoints;

/// <summary>
/// Integration tests for MediaMatic image transformation endpoints.
/// </summary>
public class TransformationEndpointsTests
{
    private static FilesourceDto CreateMemoryFilesource(
        string id = "test-memory",
        [System.Runtime.CompilerServices.CallerMemberName] string? testName = null
    ) =>
        new()
        {
            Id = id,
            Provider = "Memory",
            ConnectionString = $"memory://name=TransformationEndpointsTests_{testName ?? Guid.NewGuid().ToString()}",
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

    #region Basic Transformation Tests

    [Fact]
    public async Task Should_resize_image_by_width_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Upload should succeed");

        // Verify the file exists via download
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/fi/test-image.jpg");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Download should succeed to verify file exists");

        // Transform with width only
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/w_400/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Transform should succeed");
        transformResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/jpeg");

        // Verify the image was resized
        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        resultBytes.Length.Should().BeGreaterThan(0);

        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Width.Should().Be(400);
        resultImage.Height.Should().Be(300); // Maintains 4:3 aspect ratio
    }

    [Fact]
    public async Task Should_resize_image_by_height_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform with height only
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/h_300/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Height.Should().Be(300);
        resultImage.Width.Should().Be(400); // Maintains 4:3 aspect ratio
    }

    [Fact]
    public async Task Should_resize_image_with_both_dimensions_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform with both width and height (fit mode by default)
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/w_400,h_400/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();

        // With fit mode, the image should fit within 400x400 while maintaining aspect ratio
        resultImage.Width.Should().BeLessThanOrEqualTo(400);
        resultImage.Height.Should().BeLessThanOrEqualTo(400);
    }

    #endregion

    #region Format Conversion Tests

    [Fact]
    public async Task Should_convert_image_to_webp_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test JPEG image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Convert to WebP format
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/f_webp/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        transformResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        resultBytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Should_convert_image_to_png_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test JPEG image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Convert to PNG format
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/f_png/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        transformResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        resultBytes.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Resize Mode Tests

    [Fact]
    public async Task Should_apply_cover_crop_mode_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform with cover/fill mode (crops to fill exact dimensions)
        var transformResponse = await client.GetAsync(
            "/api/mm/fs/test-memory/transform/w_300,h_300,c_fill/test-image.jpg"
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Width.Should().Be(300);
        resultImage.Height.Should().Be(300);
    }

    [Fact]
    public async Task Should_apply_pad_mode_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform with pad mode
        var transformResponse = await client.GetAsync(
            "/api/mm/fs/test-memory/transform/w_400,h_400,c_pad/test-image.jpg"
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Width.Should().Be(400);
        resultImage.Height.Should().Be(400);
    }

    #endregion

    #region Combined Transformations Tests

    [Fact]
    public async Task Should_apply_multiple_transformations_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Apply multiple transformations: resize + quality + format
        var transformResponse = await client.GetAsync(
            "/api/mm/fs/test-memory/transform/w_400,h_300,q_80,f_webp/test-image.jpg"
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        transformResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Width.Should().Be(400);
        resultImage.Height.Should().Be(300);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Should_return_not_found_for_non_existent_file_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/w_400/non-existent.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_not_found_for_non_existent_filesource_Async()
    {
        using var factory = new WafWithInMemoryFilesourceRepository([]);
        using var client = factory.CreateClient();

        var transformResponse = await client.GetAsync("/api/mm/fs/non-existent-fs/transform/w_400/test.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task Should_return_etag_header_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform and check for ETag
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/w_400/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        transformResponse.Headers.ETag.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_return_cache_control_header_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/test-image.jpg", content);

        // Transform and check for Cache-Control
        var transformResponse = await client.GetAsync("/api/mm/fs/test-memory/transform/w_400/test-image.jpg");
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        transformResponse.Headers.CacheControl.Should().NotBeNull();
        transformResponse.Headers.CacheControl!.Public.Should().BeTrue();
    }

    #endregion

    #region Nested Path Tests

    [Fact]
    public async Task Should_transform_image_in_nested_path_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image to a nested path
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/fi/images/gallery/photo.jpg", content);

        // Transform the nested image
        var transformResponse = await client.GetAsync(
            "/api/mm/fs/test-memory/transform/w_200/images/gallery/photo.jpg"
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultBytes = await transformResponse.Content.ReadAsByteArrayAsync();
        using var resultImage = SKBitmap.Decode(resultBytes);
        resultImage.Should().NotBeNull();
        resultImage.Width.Should().Be(200);
    }

    #endregion
}
