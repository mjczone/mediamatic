// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Tests.TestHelpers;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Memory provider video processing operations.
/// </summary>
public class MemoryProviderVideoTests : IDisposable
{
    private readonly string _tempDirectory;

    public MemoryProviderVideoTests()
    {
        // Temp directory for generating source videos with FFmpeg
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mediamatic-memory-video-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    #region Basic Upload Tests

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Upload_Video_Successfully()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = false, ExtractMetadata = true };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "test-video.mp4", options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Path.Should().Be("test-video.mp4");
        result.Width.Should().BeGreaterThan(0);
        result.Height.Should().BeGreaterThan(0);
        result.Duration.Should().BeGreaterThan(0);
        result.FileSize.Should().BeGreaterThan(0);

        // Verify file exists in memory
        (await vfs.ExistsAsync("test-video.mp4"))
            .Should()
            .BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Generate_Default_Thumbnails()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 3);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = true, ThumbnailCount = 3 };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "test-thumbnails.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Thumbnails.Should().HaveCount(3);

        foreach (var thumbnail in result.Thumbnails)
        {
            thumbnail.Path.Should().NotBeNullOrEmpty();
            thumbnail.Width.Should().BeGreaterThan(0);
            thumbnail.FileSize.Should().BeGreaterThan(0);
            thumbnail.Timestamp.Should().BeGreaterThanOrEqualTo(0);

            // Verify thumbnail exists in memory
            (await vfs.ExistsAsync(thumbnail.Path))
                .Should()
                .BeTrue($"Thumbnail not found: {thumbnail.Path}");
        }
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Generate_Custom_Number_Of_Thumbnails()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 4);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = true, ThumbnailCount = 5 };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "test-5-thumbs.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Thumbnails.Should().HaveCount(5);
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Generate_Thumbnails_At_Specific_Timestamps()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 3);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var timestamps = new List<double> { 0.5, 1.5, 2.5 };
        var options = new VideoUploadOptions { GenerateThumbnails = true, ThumbnailTimestamps = timestamps };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "test-timestamps.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Thumbnails.Should().HaveCount(3);

        // Verify timestamps are approximately correct
        for (int i = 0; i < result.Thumbnails.Count; i++)
        {
            result.Thumbnails[i].Timestamp.Should().BeApproximately(timestamps[i], 0.5);
        }
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Extract_Metadata()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 640, 480, 2, 30);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = false, ExtractMetadata = true };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "test-metadata.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Width.Should().Be(640);
        result.Height.Should().Be(480);
        result.Duration.Should().BeApproximately(2.0, 0.5);
        result.FrameRate.Should().BeGreaterThan(0);
        result.VideoCodec.Should().NotBeNullOrEmpty();
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_Should_Handle_Without_Thumbnail_Generation()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = false };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "no-thumbs.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Thumbnails.Should().BeEmpty();
    }

    #endregion

    #region Thumbnail Generation Tests

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Generate_Thumbnails_From_Existing_Video()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 3);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "existing-video.mp4", uploadOptions);

        var thumbnailOptions = new ThumbnailOptions { Count = 4, Width = 160 };

        // Act
        var thumbnails = await vfs.GenerateThumbnailsAsync("existing-video.mp4", thumbnailOptions);

        // Assert
        thumbnails.Should().HaveCount(4);

        foreach (var thumbnail in thumbnails)
        {
            thumbnail.Width.Should().Be(160);
            thumbnail.FileSize.Should().BeGreaterThan(0);

            // Verify file exists in memory
            (await vfs.ExistsAsync(thumbnail.Path))
                .Should()
                .BeTrue($"Thumbnail not found: {thumbnail.Path}");
        }
    }

    [SkipIfNoFfmpegFact]
    public async Task GenerateThumbnailsAsync_Should_Handle_Single_Thumbnail()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "single-thumb-video.mp4", uploadOptions);

        var thumbnailOptions = new ThumbnailOptions { Count = 1 };

        // Act
        var thumbnails = await vfs.GenerateThumbnailsAsync("single-thumb-video.mp4", thumbnailOptions);

        // Assert
        thumbnails.Should().HaveCount(1);
        thumbnails[0].FileSize.Should().BeGreaterThan(0);
    }

    #endregion

    #region Transcoding Tests

    [SkipIfNoFfmpegFact]
    public async Task TranscodeVideoAsync_Should_Convert_Mp4_To_WebM()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "source-for-transcode.mp4", uploadOptions);

        var transcodeOptions = new TranscodeOptions { Format = VideoFormat.WebM, Codec = VideoCodec.VP8 };

        // Act
        var result = await vfs.TranscodeVideoAsync("source-for-transcode.mp4", "transcoded.webm", transcodeOptions);

        // Assert
        result.Success.Should().BeTrue($"Transcode failed with error: {result.ErrorMessage}");
        result.Path.Should().Be("transcoded.webm");
        result.Format.Should().Be(VideoFormat.WebM);
        result.FileSize.Should().BeGreaterThan(0);

        // Verify file exists in memory
        (await vfs.ExistsAsync("transcoded.webm"))
            .Should()
            .BeTrue();
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeVideoAsync_Should_Resize_Video()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 640, 480, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "large-video.mp4", uploadOptions);

        var transcodeOptions = new TranscodeOptions { Width = 320, Height = 240 };

        // Act
        var result = await vfs.TranscodeVideoAsync("large-video.mp4", "resized-video.mp4", transcodeOptions);

        // Assert
        result.Success.Should().BeTrue($"Transcode failed with error: {result.ErrorMessage}");
        result.Width.Should().Be(320);
        result.Height.Should().Be(240);
    }

    [SkipIfNoFfmpegFact]
    public async Task TranscodeVideoAsync_Should_Strip_Audio()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source-with-audio.mp4");
        await TestDataHelper.CreateTestVideoWithAudioAsync(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "video-with-audio.mp4", uploadOptions);

        var transcodeOptions = new TranscodeOptions { StripAudio = true };

        // Act
        var result = await vfs.TranscodeVideoAsync("video-with-audio.mp4", "video-no-audio.mp4", transcodeOptions);

        // Assert
        result.Success.Should().BeTrue($"Transcode failed with error: {result.ErrorMessage}");
        result.FileSize.Should().BeGreaterThan(0);

        // Verify output file exists in memory
        (await vfs.ExistsAsync("video-no-audio.mp4"))
            .Should()
            .BeTrue();
    }

    #endregion

    #region Metadata Tests

    [SkipIfNoFfmpegFact]
    public async Task GetMetadataAsync_Should_Extract_Video_Metadata()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 640, 480, 3, 25);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "metadata-video.mp4", uploadOptions);

        // Act
        var metadata = await vfs.GetMetadataAsync("metadata-video.mp4");

        // Assert
        metadata.Should().NotBeNull();
        metadata.Width.Should().Be(640);
        metadata.Height.Should().Be(480);
        metadata.Duration.Should().NotBeNull();
        metadata.Duration!.Value.Should().BeCloseTo(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(500));
        metadata.FrameRate.Should().NotBeNull();
        metadata.FrameRate!.Value.Should().BeApproximately(25, 1);
        metadata.VideoCodec.Should().NotBeNullOrEmpty();
    }

    [SkipIfNoFfmpegFact]
    public async Task GetMetadataAsync_Should_Include_Audio_Metadata()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source-audio.mp4");
        await TestDataHelper.CreateTestVideoWithAudioAsync(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "audio-metadata-video.mp4", uploadOptions);

        // Act
        var metadata = await vfs.GetMetadataAsync("audio-metadata-video.mp4");

        // Assert
        metadata.Should().NotBeNull();
        metadata.AudioCodec.Should().NotBeNullOrEmpty();
        metadata.AudioBitrate.Should().BeGreaterThan(0);
    }

    #endregion

    #region Edge Case Tests

    [SkipIfNoFfmpegTheory]
    [InlineData("mp4")]
    [InlineData("webm")]
    public async Task UploadVideoAsync_Should_Handle_Various_Formats(string format)
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, $"source-{format}.{format}");
        await TestDataHelper.CreateTestVideoAsync(sourceVideoPath, 320, 240, 2, 30, format);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = false };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, $"test-format.{format}", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed for {format}: {result.ErrorMessage}");
        result.Path.Should().EndWith($".{format}");
    }

    [SkipIfNoFfmpegTheory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task UploadVideoAsync_Should_Handle_Various_Durations(int durationSeconds)
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, $"source-{durationSeconds}s.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, durationSeconds);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions { GenerateThumbnails = false, ExtractMetadata = true };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, $"duration-{durationSeconds}s.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed for {durationSeconds}s: {result.ErrorMessage}");
        result.Duration.Should().BeApproximately(durationSeconds, 0.5);
    }

    [SkipIfNoFfmpegFact]
    public async Task UploadVideoAsync_With_Custom_Thumbnail_Width()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 640, 480, 3);
        using var videoStream = File.OpenRead(sourceVideoPath);

        var options = new VideoUploadOptions
        {
            GenerateThumbnails = true,
            ThumbnailCount = 2,
            ThumbnailWidth = 160,
        };

        // Act
        var result = await vfs.UploadVideoAsync(videoStream, "custom-thumb-width.mp4", options);

        // Assert
        result.Success.Should().BeTrue($"Upload failed with error: {result.ErrorMessage}");
        result.Thumbnails.Should().HaveCount(2);
        result.Thumbnails.Should().AllSatisfy(t => t.Width.Should().Be(160));
    }

    [SkipIfNoFfmpegFact]
    public async Task DownloadAsync_Should_Retrieve_Uploaded_Video()
    {
        // Arrange
        using var vfs = VfsConnection.Create(VfsProviderType.Memory, string.Empty);

        var sourceVideoPath = Path.Combine(_tempDirectory, "source.mp4");
        await TestDataHelper.CreateTestMp4Async(sourceVideoPath, 320, 240, 2);
        using var videoStream = File.OpenRead(sourceVideoPath);
        var originalLength = videoStream.Length;

        var uploadOptions = new VideoUploadOptions { GenerateThumbnails = false };
        await vfs.UploadVideoAsync(videoStream, "download-test.mp4", uploadOptions);

        // Act
        using var downloadStream = await vfs.DownloadAsync("download-test.mp4");

        // Assert
        downloadStream.Length.Should().BeGreaterThan(0);
        // Downloaded size should be close to original (within 10%)
        var difference = Math.Abs(downloadStream.Length - originalLength);
        difference.Should().BeLessThan((long)(originalLength * 0.1));
    }

    #endregion
}
