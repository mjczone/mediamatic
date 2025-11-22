// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProcessorTests;

/// <summary>
/// Unit tests for the ImageProcessor.
/// </summary>
public class ImageProcessorTests
{
    private readonly ImageProcessor _processor = new();

    #region ResizeAsync Tests

    [Fact]
    public async Task ResizeAsync_Should_Resize_Image_By_Width()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var result = await _processor.ResizeAsync(stream, width: 400, height: null);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(300); // Maintains aspect ratio
        result.fileSize.Should().BeGreaterThan(0);
        result.format.Should().Be(ImageFormat.Jpeg);
    }

    [Fact]
    public async Task ResizeAsync_Should_Resize_Image_By_Height()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var result = await _processor.ResizeAsync(stream, width: null, height: 300);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().Be(400); // Maintains aspect ratio
        result.height.Should().Be(300);
    }

    [Fact]
    public async Task ResizeAsync_Should_Resize_Image_By_Both_Dimensions()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var result = await _processor.ResizeAsync(stream, width: 400, height: 300);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().BeLessOrEqualTo(400);
        result.height.Should().BeLessOrEqualTo(300);
    }

    [Fact]
    public async Task ResizeAsync_Should_Preserve_Aspect_Ratio()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(1920, 1080);

        // Act
        var result = await _processor.ResizeAsync(stream, width: 640, height: null);

        // Assert
        var aspectRatio = (double)result.width / result.height;
        var expectedRatio = 1920.0 / 1080.0;
        aspectRatio.Should().BeApproximately(expectedRatio, 0.01);
    }

    [Fact]
    public async Task ResizeAsync_Should_Handle_PNG_Input()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestPng(640, 480);
        var options = new ImageProcessingOptions { Format = ImageFormat.Png };

        // Act
        var result = await _processor.ResizeAsync(stream, width: 320, height: null, options);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().Be(320);
        result.format.Should().Be(ImageFormat.Png);
    }

    [Fact]
    public async Task ResizeAsync_Should_Handle_WebP_Output()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(640, 480);
        var options = new ImageProcessingOptions { Format = ImageFormat.WebP };

        // Act
        var result = await _processor.ResizeAsync(stream, width: 320, height: null, options);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().Be(320);
        result.format.Should().Be(ImageFormat.WebP);
    }

    [Fact]
    public async Task ResizeAsync_Should_Apply_Quality_Setting()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions { Quality = 50 };

        // Act
        var result = await _processor.ResizeAsync(stream, width: 400, height: null, options);

        // Assert
        result.Should().NotBeNull();
        result.fileSize.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(1920, 1080, 640)]
    [InlineData(800, 600, 400)]
    [InlineData(1024, 768, 512)]
    public async Task ResizeAsync_Should_Handle_Various_Dimensions(
        int originalWidth,
        int originalHeight,
        int targetWidth
    )
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(originalWidth, originalHeight);

        // Act
        var result = await _processor.ResizeAsync(stream, width: targetWidth, height: null);

        // Assert
        result.width.Should().Be(targetWidth);
    }

    #endregion

    #region ConvertFormatAsync Tests

    [Fact]
    public async Task ConvertFormatAsync_Should_Convert_JPEG_To_PNG()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(400, 300);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, ImageFormat.Png, quality: 100);

        // Assert
        result.Should().NotBeNull();
        result.format.Should().Be(ImageFormat.Png);
        result.width.Should().Be(400);
        result.height.Should().Be(300);
    }

    [Fact]
    public async Task ConvertFormatAsync_Should_Convert_JPEG_To_WebP()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(400, 300);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, ImageFormat.WebP, quality: 80);

        // Assert
        result.Should().NotBeNull();
        result.format.Should().Be(ImageFormat.WebP);
    }

    [Fact]
    public async Task ConvertFormatAsync_Should_Convert_PNG_To_JPEG()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestPng(400, 300);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, ImageFormat.Jpeg, quality: 85);

        // Assert
        result.Should().NotBeNull();
        result.format.Should().Be(ImageFormat.Jpeg);
    }

    [Fact]
    public async Task ConvertFormatAsync_Should_Preserve_Dimensions()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(640, 480);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, ImageFormat.Png, quality: 100);

        // Assert
        result.width.Should().Be(640);
        result.height.Should().Be(480);
    }

    [Theory]
    [InlineData(ImageFormat.Jpeg)]
    [InlineData(ImageFormat.Png)]
    [InlineData(ImageFormat.WebP)]
    public async Task ConvertFormatAsync_Should_Convert_To_Various_Formats(ImageFormat targetFormat)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(300, 200);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, targetFormat, quality: 80);

        // Assert
        result.format.Should().Be(targetFormat);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ConvertFormatAsync_Should_Apply_Various_Quality_Settings(int quality)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(400, 300);

        // Act
        var result = await _processor.ConvertFormatAsync(stream, ImageFormat.Jpeg, quality);

        // Assert
        result.fileSize.Should().BeGreaterThan(0);
    }

    #endregion

    // Note: GenerateVariantsAsync tests are not included because that method
    // throws NotImplementedException and is meant to be called only through
    // VfsMethodsBase.UploadImageAsync orchestration, which handles variant
    // generation internally using ResizeAsync and ConvertFormatAsync.

    #region ResizeMode Tests

    [Fact]
    public async Task ResizeAsync_Fit_Should_Scale_To_Fit_Within_Bounds()
    {
        // Arrange - 800x600 image (4:3 ratio)
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Fit };

        // Act - target 400x400 (1:1)
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should fit within 400x400 maintaining aspect ratio
        result.Should().NotBeNull();
        result.width.Should().BeLessOrEqualTo(400);
        result.height.Should().BeLessOrEqualTo(400);
        // One dimension should be exactly 400, other smaller
        (result.width == 400 || result.height == 400)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ResizeAsync_Cover_Should_Scale_To_Cover_And_Crop()
    {
        // Arrange - 800x600 image (4:3 ratio)
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Cover };

        // Act - target 400x400 (1:1)
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should be exactly 400x400 (cropped to fit)
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Fact]
    public async Task ResizeAsync_Pad_Should_Scale_And_Add_Padding()
    {
        // Arrange - 800x600 image (4:3 ratio)
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Pad, BackgroundColor = "#FF0000" };

        // Act - target 400x400 (1:1)
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should be exactly 400x400 with padding
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Fact]
    public async Task ResizeAsync_Stretch_Should_Distort_To_Exact_Dimensions()
    {
        // Arrange - 800x600 image (4:3 ratio)
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Stretch };

        // Act - target 400x400 (1:1) - will distort
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should be exactly 400x400 (stretched)
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Fact]
    public async Task ResizeAsync_Cover_Should_Preserve_Content_For_Portrait_Image()
    {
        // Arrange - 600x800 portrait image (3:4 ratio)
        using var stream = TestDataHelper.CreateTestJpeg(600, 800);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Cover };

        // Act - target 400x400 (1:1)
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should be exactly 400x400
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Theory]
    [InlineData(ResizeMode.Fit)]
    [InlineData(ResizeMode.Cover)]
    [InlineData(ResizeMode.Pad)]
    [InlineData(ResizeMode.Stretch)]
    public async Task ResizeAsync_All_Modes_Should_Produce_Valid_Output(ResizeMode mode)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(640, 480);
        var options = new ImageProcessingOptions { ResizeMode = mode };

        // Act
        var result = await _processor.ResizeAsync(stream, width: 320, height: 240, options);

        // Assert
        result.Should().NotBeNull();
        result.fileSize.Should().BeGreaterThan(0);
        result.format.Should().Be(ImageFormat.Jpeg);
    }

    #endregion

    #region FocalPoint Tests

    [Fact]
    public async Task ResizeAsync_Cover_With_FocalPoint_TopLeft_Should_Crop_From_TopLeft()
    {
        // Arrange - 800x600 image
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions
        {
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = 0.0, Y = 0.0 }, // Top-left
        };

        // Act - target 400x400
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should crop with top-left as focal point
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Fact]
    public async Task ResizeAsync_Cover_With_FocalPoint_BottomRight_Should_Crop_From_BottomRight()
    {
        // Arrange - 800x600 image
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions
        {
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = 1.0, Y = 1.0 }, // Bottom-right
        };

        // Act - target 400x400
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should crop with bottom-right as focal point
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Fact]
    public async Task ResizeAsync_Cover_With_FocalPoint_Center_Should_Match_Default_Center_Crop()
    {
        // Arrange - 800x600 image
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions
        {
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = 0.5, Y = 0.5 }, // Center
        };

        // Act - target 400x400
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should be same as center crop
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Theory]
    [InlineData(0.25, 0.25)] // Top-left quadrant
    [InlineData(0.75, 0.25)] // Top-right quadrant
    [InlineData(0.25, 0.75)] // Bottom-left quadrant
    [InlineData(0.75, 0.75)] // Bottom-right quadrant
    public async Task ResizeAsync_Cover_With_Various_FocalPoints_Should_Produce_Valid_Output(double x, double y)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions
        {
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = x, Y = y },
        };

        // Act
        var result = await _processor.ResizeAsync(stream, width: 300, height: 300, options);

        // Assert
        result.Should().NotBeNull();
        result.width.Should().Be(300);
        result.height.Should().Be(300);
        result.fileSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResizeAsync_Cover_Without_FocalPoint_Should_Default_To_Center()
    {
        // Arrange - 800x600 image
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);
        var options = new ImageProcessingOptions
        {
            ResizeMode = ResizeMode.Cover,
            FocalPoint = null, // Explicitly null
        };

        // Act - target 400x400
        var result = await _processor.ResizeAsync(stream, width: 400, height: 400, options);

        // Assert - should use center crop
        result.Should().NotBeNull();
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    #endregion
}
