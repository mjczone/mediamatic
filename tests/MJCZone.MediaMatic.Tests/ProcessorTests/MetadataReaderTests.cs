// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Processors;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProcessorTests;

/// <summary>
/// Unit tests for the MetadataReader processor.
/// </summary>
public class MetadataReaderTests : IDisposable
{
    private readonly MetadataReader _reader = new();
    private readonly string _tempDirectory;

    public MetadataReaderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mediamatic-metadata-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    #region ExtractImageMetadataAsync Tests

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Extract_JPEG_Dimensions()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Width.Should().Be(800);
        metadata.Height.Should().Be(600);
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Extract_PNG_Dimensions()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestPng(640, 480);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Width.Should().Be(640);
        metadata.Height.Should().Be(480);
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Extract_WebP_Dimensions()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestWebP(1024, 768);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Width.Should().Be(1024);
        metadata.Height.Should().Be(768);
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Reset_Stream_Position()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(400, 300);
        stream.Position = 100; // Set to middle

        // Act
        await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        stream.Position.Should().Be(100, "Stream position should be restored");
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Set_Provider()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(800, 600);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Provider.Should().Be("MetadataExtractor");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(640, 480)]
    [InlineData(320, 240)]
    [InlineData(100, 100)]
    public async Task ExtractImageMetadataAsync_Should_Handle_Various_Dimensions(int width, int height)
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(width, height);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Width.Should().Be(width);
        metadata.Height.Should().Be(height);
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Handle_Large_Images()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(4096, 2160);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Width.Should().Be(4096);
        metadata.Height.Should().Be(2160);
    }

    [Fact]
    public async Task ExtractImageMetadataAsync_Should_Handle_Small_Images()
    {
        // Arrange
        using var stream = TestDataHelper.CreateTestJpeg(16, 16);

        // Act
        var metadata = await _reader.ExtractImageMetadataAsync(stream);

        // Assert
        metadata.Width.Should().Be(16);
        metadata.Height.Should().Be(16);
    }

    #endregion

    #region ExtractVideoMetadataAsync Tests

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_Duration()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 3);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Duration.Should().NotBeNull();
        metadata.Duration!.Value.Should().BeCloseTo(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(500));
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_Dimensions()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.Width.Should().Be(640);
        metadata.Height.Should().Be(480);
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_FrameRate()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 2, 30);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.FrameRate.Should().NotBeNull();
        metadata.FrameRate!.Value.Should().BeApproximately(30, 1);
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_VideoCodec()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.VideoCodec.Should().NotBeNullOrEmpty();
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_AudioCodec_When_Present()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source-audio.mp4");
        await TestDataHelper.CreateTestVideoWithAudioAsync(videoPath, 320, 240, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.AudioCodec.Should().NotBeNullOrEmpty();
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Extract_Bitrate()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.VideoBitrate.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task ExtractVideoMetadataAsync_Should_Handle_Various_Durations(int durationSeconds)
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, $"source-{durationSeconds}s.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, durationSeconds);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.Duration.Should().NotBeNull();
        metadata
            .Duration!.Value.Should()
            .BeCloseTo(TimeSpan.FromSeconds(durationSeconds), TimeSpan.FromMilliseconds(500));
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(320, 240)]
    [InlineData(640, 480)]
    [InlineData(1280, 720)]
    public async Task ExtractVideoMetadataAsync_Should_Handle_Various_Resolutions(int width, int height)
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, $"source-{width}x{height}.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, width, height, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.Width.Should().Be(width);
        metadata.Height.Should().Be(height);
    }

    [SkipIfNoFfmpegFact]
    public async Task ExtractVideoMetadataAsync_Should_Set_Provider()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 2);

        // Act
        var metadata = await _reader.ExtractVideoMetadataAsync(videoPath);

        // Assert
        metadata.Provider.Should().Be("FFMpegCore");
    }

    #endregion
}
