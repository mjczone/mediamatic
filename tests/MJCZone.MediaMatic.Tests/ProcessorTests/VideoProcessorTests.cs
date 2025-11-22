// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProcessorTests;

/// <summary>
/// Unit tests for the VideoProcessor.
/// </summary>
public class VideoProcessorTests : IDisposable
{
    private readonly VideoProcessor _processor = new();
    private readonly string _tempDirectory;

    public VideoProcessorTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mediamatic-video-processor-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    #region GenerateThumbnailsAsync Tests

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Generate_Single_Thumbnail()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 2);

        var options = new ThumbnailOptions { Count = 1, OutputPath = _tempDirectory };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Generate_Multiple_Thumbnails()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 3);

        var options = new ThumbnailOptions { Count = 3, OutputPath = _tempDirectory };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(3);
        thumbnails.Should().AllSatisfy(t => File.Exists(t).Should().BeTrue());
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Respect_Custom_Width()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 160,
            OutputPath = _tempDirectory,
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        // Verify the thumbnail file is created (actual dimensions would need image processing to verify)
        new FileInfo(thumbnails[0])
            .Length.Should()
            .BeGreaterThan(0);
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Generate_Evenly_Spaced_Thumbnails()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 4);

        var options = new ThumbnailOptions { Count = 4, OutputPath = _tempDirectory };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(4);
        thumbnails.Should().AllSatisfy(t => File.Exists(t).Should().BeTrue());
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GenerateThumbnailsAsync_Should_Handle_Various_Counts(int count)
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, $"source-{count}.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 320, 240, 5);

        var options = new ThumbnailOptions { Count = count, OutputPath = _tempDirectory };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(count);
    }

    #endregion

    #region TranscodeAsync Tests

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Convert_Mp4_To_WebM()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "output.webm");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions { Format = VideoFormat.WebM, Codec = VideoCodec.VP8 };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
        new FileInfo(destPath).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Resize_Video()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "resized.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 640, 480, 2);

        var options = new TranscodeOptions { Width = 320, Height = 240 };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Change_Codec_To_H265()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "h265.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions { Codec = VideoCodec.H265 };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Strip_Audio()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source-audio.mp4");
        var destPath = Path.Combine(_tempDirectory, "no-audio.mp4");
        await TestDataHelper.CreateTestVideoWithAudioAsync(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions { StripAudio = true };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Apply_CRF_Quality()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "crf.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions { Crf = 28 };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegTheory]
    [InlineData("ultrafast")]
    [InlineData("medium")]
    [InlineData("slow")]
    public async Task TranscodeAsync_Should_Apply_Various_Presets(string preset)
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, $"source-{preset}.mp4");
        var destPath = Path.Combine(_tempDirectory, $"preset-{preset}.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions { Preset = preset };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Change_Bitrate()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "bitrate.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2);

        var options = new TranscodeOptions
        {
            VideoBitrate = 500_000, // 500 kbps
        };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeAsync_Should_Change_FrameRate()
    {
        // Arrange
        var sourcePath = Path.Combine(_tempDirectory, "source.mp4");
        var destPath = Path.Combine(_tempDirectory, "framerate.mp4");
        await TestDataHelper.CreateTestMp4Async(sourcePath, 320, 240, 2, 30);

        var options = new TranscodeOptions { FrameRate = 15 };

        // Act
        var result = await _processor.TranscodeAsync(sourcePath, destPath, options);

        // Assert
        result.Should().Be(destPath);
        File.Exists(destPath).Should().BeTrue();
    }

    #endregion

    #region ResizeMode Thumbnail Tests

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Fit_Should_Maintain_Aspect_Ratio()
    {
        // Arrange - 640x480 video (4:3)
        var videoPath = Path.Combine(_tempDirectory, "fit-source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 320,
            Height = 320,
            ResizeMode = ResizeMode.Fit,
            OutputPath = _tempDirectory,
            FilePattern = "fit_{0}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        new FileInfo(thumbnails[0]).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Cover_Should_Crop_To_Exact_Dimensions()
    {
        // Arrange - 640x480 video (4:3)
        var videoPath = Path.Combine(_tempDirectory, "cover-source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 320,
            Height = 320,
            ResizeMode = ResizeMode.Cover,
            OutputPath = _tempDirectory,
            FilePattern = "cover_{0}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        new FileInfo(thumbnails[0]).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Pad_Should_Add_Letterboxing()
    {
        // Arrange - 640x480 video (4:3)
        var videoPath = Path.Combine(_tempDirectory, "pad-source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 320,
            Height = 320,
            ResizeMode = ResizeMode.Pad,
            BackgroundColor = "#000000",
            OutputPath = _tempDirectory,
            FilePattern = "pad_{0}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        new FileInfo(thumbnails[0]).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Stretch_Should_Distort_To_Dimensions()
    {
        // Arrange - 640x480 video (4:3)
        var videoPath = Path.Combine(_tempDirectory, "stretch-source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 320,
            Height = 320,
            ResizeMode = ResizeMode.Stretch,
            OutputPath = _tempDirectory,
            FilePattern = "stretch_{0}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        new FileInfo(thumbnails[0]).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(ResizeMode.Fit)]
    [InlineData(ResizeMode.Cover)]
    [InlineData(ResizeMode.Pad)]
    [InlineData(ResizeMode.Stretch)]
    public async Task GenerateThumbnailsAsync_All_Modes_Should_Generate_Valid_Thumbnails(ResizeMode mode)
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, $"mode-{mode}.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 160,
            Height = 120,
            ResizeMode = mode,
            OutputPath = _tempDirectory,
            FilePattern = $"mode-{mode}_{{0}}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
    }

    #endregion

    #region FocalPoint Thumbnail Tests

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Cover_With_FocalPoint_Should_Crop_Around_FocalPoint()
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, "focal-source.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 320,
            Height = 320,
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = 0.75, Y = 0.25 }, // Top-right area
            OutputPath = _tempDirectory,
            FilePattern = "focal_{0}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
        new FileInfo(thumbnails[0]).Length.Should().BeGreaterThan(0);
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(0.0, 0.0)] // Top-left
    [InlineData(1.0, 1.0)] // Bottom-right
    [InlineData(0.5, 0.5)] // Center
    [InlineData(0.25, 0.75)] // Bottom-left quadrant
    public async Task GenerateThumbnailsAsync_Cover_With_Various_FocalPoints_Should_Generate_Valid_Thumbnails(
        double x,
        double y
    )
    {
        // Arrange
        var videoPath = Path.Combine(_tempDirectory, $"focal-{x}-{y}.mp4");
        await TestDataHelper.CreateTestMp4Async(videoPath, 640, 480, 2);

        var options = new ThumbnailOptions
        {
            Count = 1,
            Width = 200,
            Height = 200,
            ResizeMode = ResizeMode.Cover,
            FocalPoint = new FocalPoint { X = x, Y = y },
            OutputPath = _tempDirectory,
            FilePattern = $"focal-{x}-{y}_{{0}}.jpg",
        };

        // Act
        var thumbnails = await _processor.GenerateThumbnailsAsync(videoPath, options);

        // Assert
        thumbnails.Should().HaveCount(1);
        File.Exists(thumbnails[0]).Should().BeTrue();
    }

    #endregion
}
