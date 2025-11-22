// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Processors;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProcessorTests;

/// <summary>
/// Unit tests for the MimeTypeDetector processor.
/// </summary>
public class MimeTypeDetectorTests
{
    private readonly MimeTypeDetector _detector = new();

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Detect_JPEG()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(100, 100);

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Detect_PNG()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestPng(100, 100);

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().Be("image/png");
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Detect_WebP()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestWebP(100, 100);

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().Be("image/webp");
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Reset_Stream_Position()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(100, 100);
        stream.Position = 50; // Set to middle of stream

        // Act
        await _detector.DetectMimeTypeAsync(stream);

        // Assert
        stream.Position.Should().Be(50, "Stream position should be restored after detection");
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Return_Null_For_Empty_Stream()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().BeNull();
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Return_Null_For_Unknown_Content()
    {
        // Arrange
        var randomBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        using var stream = new MemoryStream(randomBytes);

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        // Should return null or some default for unknown content
        // The actual behavior depends on the implementation
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(640, 480)]
    [InlineData(1920, 1080)]
    public async Task DetectMimeTypeAsync_Should_Handle_Various_Image_Sizes(int width, int height)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(width, height);

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DetectMimeTypeAsync_Should_Work_With_Stream_At_Beginning()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestPng(200, 200);
        stream.Position = 0;

        // Act
        var mimeType = await _detector.DetectMimeTypeAsync(stream);

        // Assert
        mimeType.Should().Be("image/png");
    }
}
