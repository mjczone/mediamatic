// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Net;
using System.Text;
using System.Text.Json;
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
        var uploadResponse = await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Upload should succeed");

        // Verify the file exists via download
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/test-image.jpg");
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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

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
        await client.PostAsync("/api/mm/fs/test-memory/files/images/gallery/photo.jpg", content);

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

    #region POST Transform Tests (Single)

    [Fact]
    public async Task Should_generate_and_save_transform_with_POST_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

        // POST transform to generate and save
        var request = new TransformRequestDto { SaveTo = "thumbs/test-image_400.webp" };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/transform/w_400,f_webp/test-image.jpg",
            requestContent
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformResultDto>();
        result.Should().NotBeNull();
        result!.Path.Should().Be("thumbs/test-image_400.webp");
        result.Success.Should().BeTrue();
        result.Width.Should().Be(400);
        result.Format.Should().Be("webp");
        result.Size.Should().BeGreaterThan(0);

        // Verify the saved file exists
        var downloadResponse = await client.GetAsync("/api/mm/fs/test-memory/files/thumbs/test-image_400.webp");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_return_bad_request_when_saveTo_missing_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

        // POST transform without saveTo
        var request = new TransformRequestDto { SaveTo = "" };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/transform/w_400/test-image.jpg",
            requestContent
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_generate_transform_with_quality_option_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

        // POST transform with quality option
        var request = new TransformRequestDto { SaveTo = "thumbs/test-image_q50.jpg" };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/transform/w_400,q_50,f_jpeg/test-image.jpg",
            requestContent
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformResultDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Format.Should().Be("jpeg");
    }

    #endregion

    #region POST Batch Transform Tests

    [Fact]
    public async Task Should_generate_batch_transforms_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

        // POST batch transform
        var request = new TransformBatchRequestDto
        {
            Source = "test-image.jpg",
            Variants =
            [
                new TransformVariantDto { Transformations = "w_400,f_webp", SaveTo = "thumbs/test-image_400.webp" },
                new TransformVariantDto { Transformations = "w_800,f_webp", SaveTo = "thumbs/test-image_800.webp" },
                new TransformVariantDto
                {
                    Transformations = "w_200,h_200,c_fill,f_webp",
                    SaveTo = "thumbs/test-image_200x200.webp",
                },
            ],
        };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync("/api/mm/fs/test-memory/transform-batch/", requestContent);
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformBatchResponseDto>();
        result.Should().NotBeNull();
        result!.Results.Should().HaveCount(3);
        result.Results.Should().OnlyContain(r => r.Success);

        // Verify all files exist
        var download1 = await client.GetAsync("/api/mm/fs/test-memory/files/thumbs/test-image_400.webp");
        download1.StatusCode.Should().Be(HttpStatusCode.OK);

        var download2 = await client.GetAsync("/api/mm/fs/test-memory/files/thumbs/test-image_800.webp");
        download2.StatusCode.Should().Be(HttpStatusCode.OK);

        var download3 = await client.GetAsync("/api/mm/fs/test-memory/files/thumbs/test-image_200x200.webp");
        download3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_return_bad_request_when_source_missing_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // POST batch transform without source
        var request = new TransformBatchRequestDto
        {
            Source = "",
            Variants = [new TransformVariantDto { Transformations = "w_400,f_webp", SaveTo = "thumbs/thumb.webp" }],
        };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync("/api/mm/fs/test-memory/transform-batch/", requestContent);
        transformResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_return_bad_request_when_variants_empty_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // POST batch transform without variants
        var request = new TransformBatchRequestDto { Source = "test-image.jpg", Variants = [] };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync("/api/mm/fs/test-memory/transform-batch/", requestContent);
        transformResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_handle_partial_failure_in_batch_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/files/test-image.jpg", content);

        // POST batch transform with one invalid variant (missing saveTo)
        var request = new TransformBatchRequestDto
        {
            Source = "test-image.jpg",
            Variants =
            [
                new TransformVariantDto { Transformations = "w_400,f_webp", SaveTo = "thumbs/valid.webp" },
                new TransformVariantDto { Transformations = "w_800,f_webp", SaveTo = "" }, // Invalid - no saveTo
            ],
        };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync("/api/mm/fs/test-memory/transform-batch/", requestContent);
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformBatchResponseDto>();
        result.Should().NotBeNull();
        result!.Results.Should().HaveCount(2);
        result.Results[0].Success.Should().BeTrue();
        result.Results[1].Success.Should().BeFalse();
        result.Results[1].ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region POST Transform in Bucket Tests

    [Fact]
    public async Task Should_generate_transform_in_bucket_with_POST_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image to bucket
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/files/test-image.jpg", content);

        // POST transform in bucket
        var request = new TransformRequestDto { SaveTo = "thumbs/test-image_400.webp" };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/transform/w_400,f_webp/test-image.jpg",
            requestContent
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformResultDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify the saved file exists in bucket
        var downloadResponse = await client.GetAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/files/thumbs/test-image_400.webp"
        );
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_generate_batch_transforms_in_bucket_Async()
    {
        var filesource = CreateMemoryFilesource();
        using var factory = new WafWithInMemoryFilesourceRepository([filesource]);
        using var client = factory.CreateClient();

        // Upload a test image to bucket
        var imageData = CreateTestJpeg(800, 600);
        var content = new ByteArrayContent(imageData);
        await client.PostAsync("/api/mm/fs/test-memory/bu/my-bucket/files/test-image.jpg", content);

        // POST batch transform in bucket
        var request = new TransformBatchRequestDto
        {
            Source = "test-image.jpg",
            Variants =
            [
                new TransformVariantDto { Transformations = "w_400,f_webp", SaveTo = "thumbs/thumb_400.webp" },
                new TransformVariantDto { Transformations = "w_800,f_webp", SaveTo = "thumbs/thumb_800.webp" },
            ],
        };
        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var transformResponse = await client.PostAsync(
            "/api/mm/fs/test-memory/bu/my-bucket/transform-batch/",
            requestContent
        );
        transformResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await transformResponse.ReadAsJsonAsync<TransformBatchResponseDto>();
        result.Should().NotBeNull();
        result!.Results.Should().HaveCount(2);
        result.Results.Should().OnlyContain(r => r.Success);
    }

    #endregion
}
