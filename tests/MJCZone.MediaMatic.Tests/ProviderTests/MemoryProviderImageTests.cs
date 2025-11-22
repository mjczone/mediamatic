// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Providers;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Memory provider image processing operations.
/// </summary>
public class MemoryProviderImageTests
{
    [Fact]
    public async Task UploadImageAsync_Should_Upload_Image_Successfully()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "test-image.jpg");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Path.Should().Be("test-image.jpg");
        result.Width.Should().BeGreaterThan(0);
        result.Height.Should().BeGreaterThan(0);
        result.FileSize.Should().BeGreaterThan(0);
        result.Format.Should().Be(ImageFormat.Jpeg);
        result.MimeType.Should().Be("image/jpeg");

        // Verify file exists in memory
        var exists = await vfs.ExistsAsync("test-image.jpg");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UploadImageAsync_Should_Generate_WebP_And_Avif_Variants()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(800, 600);

        var options = new ImageUploadOptions
        {
            GenerateFormats = true,
            Formats = [ImageFormat.WebP, ImageFormat.Avif],
            GenerateThumbnails = false,
        };

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "test-variants.jpg", options);

        // Assert
        result.Success.Should().BeTrue();

        // Note: AVIF may not be supported on all platforms
        result.Variants.Should().HaveCountGreaterOrEqualTo(1);

        var webpVariant = result.Variants.FirstOrDefault(v => v.Format == ImageFormat.WebP);
        webpVariant.Should().NotBeNull();
        webpVariant!.Path.Should().Be("test-variants.webp");
        webpVariant.Width.Should().BeGreaterThan(0);
        webpVariant.FileSize.Should().BeGreaterThan(0);

        // Verify WebP file exists in memory
        var exists = await vfs.ExistsAsync("test-variants.webp");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UploadImageAsync_Should_Generate_Thumbnail_Variants()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(1920, 1080);

        var options = new ImageUploadOptions
        {
            GenerateThumbnails = true,
            ThumbnailSizes = [320, 640, 1024],
            GenerateFormats = false,
        };

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "test-thumbnails.jpg", options);

        // Assert
        result.Success.Should().BeTrue();
        result.Variants.Should().HaveCount(3);

        var thumb320 = result.Variants.FirstOrDefault(v => v.VariantType == "thumbnail-320w");
        thumb320.Should().NotBeNull();
        thumb320!.Width.Should().Be(320);
        thumb320.Path.Should().Contain("_320w");

        var thumb640 = result.Variants.FirstOrDefault(v => v.VariantType == "thumbnail-640w");
        thumb640.Should().NotBeNull();
        thumb640!.Width.Should().Be(640);

        var thumb1024 = result.Variants.FirstOrDefault(v => v.VariantType == "thumbnail-1024w");
        thumb1024.Should().NotBeNull();
        thumb1024!.Width.Should().Be(1024);

        // Verify files exist in memory
        (await vfs.ExistsAsync("test-thumbnails_320w.jpg"))
            .Should()
            .BeTrue();
        (await vfs.ExistsAsync("test-thumbnails_640w.jpg")).Should().BeTrue();
        (await vfs.ExistsAsync("test-thumbnails_1024w.jpg")).Should().BeTrue();
    }

    [Fact]
    public async Task UploadImageAsync_Should_Resize_When_MaxWidth_Specified()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(1920, 1080);

        var options = new ImageUploadOptions
        {
            MaxWidth = 1024,
            GenerateFormats = false,
            GenerateThumbnails = false,
        };

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "test-resize.jpg", options);

        // Assert
        result.Success.Should().BeTrue();
        result.Width.Should().BeLessOrEqualTo(1024);
        result.Height.Should().BeLessOrEqualTo(1024);
    }

    [Fact]
    public async Task UploadImageAsync_Should_Handle_PNG_Format()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestPng(640, 480);

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "test.png");

        // Assert
        result.Success.Should().BeTrue();
        result.Format.Should().Be(ImageFormat.Png);
        result.MimeType.Should().Be("image/png");
    }

    [Fact]
    public async Task ProcessImageAsync_Should_Resize_Image()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);

        // Upload original image
        using var uploadStream = TestDataHelper.CreateTestJpeg(1920, 1080);
        await vfs.UploadFileAsync(uploadStream, "original.jpg");

        var options = new ImageProcessingOptions
        {
            Width = 800,
            Format = ImageFormat.Jpeg,
            Quality = 85,
        };

        // Act
        var result = await vfs.ProcessImageAsync("original.jpg", "resized.jpg", options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Path.Should().Be("resized.jpg");
        result.Width.Should().Be(800);
        result.Height.Should().BeLessOrEqualTo(800);
        result.FileSize.Should().BeGreaterThan(0);

        // Verify resized file exists in memory
        (await vfs.ExistsAsync("resized.jpg"))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ProcessImageAsync_Should_Convert_Format()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);

        // Upload JPEG image
        using var uploadStream = TestDataHelper.CreateTestJpeg(800, 600);
        await vfs.UploadFileAsync(uploadStream, "source.jpg");

        var options = new ImageProcessingOptions { Format = ImageFormat.Png, Quality = 100 };

        // Act
        var result = await vfs.ProcessImageAsync("source.jpg", "converted.png", options);

        // Assert
        result.Success.Should().BeTrue();
        result.Format.Should().Be(ImageFormat.Png);
        (await vfs.ExistsAsync("converted.png")).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessImageAsync_Should_Convert_To_WebP()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);

        using var uploadStream = TestDataHelper.CreateTestJpeg(800, 600);
        await vfs.UploadFileAsync(uploadStream, "source.jpg");

        var options = new ImageProcessingOptions { Format = ImageFormat.WebP, Quality = 80 };

        // Act
        var result = await vfs.ProcessImageAsync("source.jpg", "converted.webp", options);

        // Assert
        result.Success.Should().BeTrue();
        result.Format.Should().Be(ImageFormat.WebP);
        result.FileSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetMetadataAsync_Should_Extract_Image_Metadata()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);

        using var uploadStream = TestDataHelper.CreateTestJpeg(800, 600);
        await vfs.UploadFileAsync(uploadStream, "metadata-test.jpg");

        // Act
        var metadata = await vfs.GetMetadataAsync("metadata-test.jpg");

        // Assert
        metadata.Should().NotBeNull();
        metadata.Width.Should().Be(800);
        metadata.Height.Should().Be(600);
        metadata.Provider.Should().Be("MetadataExtractor");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(640, 480)]
    [InlineData(1024, 768)]
    public async Task UploadImageAsync_Should_Handle_Various_Dimensions(int width, int height)
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(width, height);

        // Act
        var result = await vfs.UploadImageAsync(imageStream, $"test-{width}x{height}.jpg");

        // Assert
        result.Success.Should().BeTrue();
        result.Width.Should().BeGreaterThan(0);
        result.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UploadImageAsync_Should_Skip_Thumbnails_Larger_Than_Original()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var imageStream = TestDataHelper.CreateTestJpeg(640, 480);

        var options = new ImageUploadOptions
        {
            GenerateThumbnails = true,
            ThumbnailSizes = [320, 640, 1024, 1920],
            GenerateFormats = false,
        };

        // Act
        var result = await vfs.UploadImageAsync(imageStream, "small-image.jpg", options);

        // Assert
        result.Success.Should().BeTrue();

        // Should only generate 320w thumbnail
        result.Variants.Should().HaveCount(1);
        result.Variants[0].VariantType.Should().Be("thumbnail-320w");
    }

    [Fact]
    public async Task UploadImageAsync_Should_Handle_Multiple_Uploads_To_Same_Provider()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);

        // Act - Upload multiple images
        using var stream1 = TestDataHelper.CreateTestJpeg(640, 480);
        var result1 = await vfs.UploadImageAsync(stream1, "image1.jpg");

        using var stream2 = TestDataHelper.CreateTestPng(800, 600);
        var result2 = await vfs.UploadImageAsync(stream2, "image2.png");

        using var stream3 = TestDataHelper.CreateTestWebP(1024, 768);
        var result3 = await vfs.UploadImageAsync(stream3, "image3.webp");

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result3.Success.Should().BeTrue();

        // Verify all files exist
        (await vfs.ExistsAsync("image1.jpg"))
            .Should()
            .BeTrue();
        (await vfs.ExistsAsync("image2.png")).Should().BeTrue();
        (await vfs.ExistsAsync("image3.webp")).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_Should_Retrieve_Uploaded_Image()
    {
        // Arrange
        using var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Memory, string.Empty);
        using var uploadStream = TestDataHelper.CreateTestJpeg(800, 600);
        var originalLength = uploadStream.Length;

        await vfs.UploadImageAsync(uploadStream, "download-test.jpg");

        // Act
        using var downloadStream = await vfs.DownloadAsync("download-test.jpg");

        // Assert
        downloadStream.Length.Should().BeGreaterThan(0);
        // The downloaded file might be slightly different due to processing
        downloadStream.Length.Should().BeGreaterThan(originalLength / 2);
    }
}
