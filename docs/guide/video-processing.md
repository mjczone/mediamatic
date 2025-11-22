# Video Processing

MediaMatic uses [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore) for video processing, providing thumbnail generation, transcoding, and metadata extraction.

## Prerequisites

FFmpeg must be installed on your system. See [Installation](installation.md#ffmpeg-optional) for setup instructions.

## VideoProcessor

The `VideoProcessor` class provides all video operations:

```csharp
using MJCZone.MediaMatic.Processors;

var processor = new VideoProcessor();
```

## Generating Thumbnails

### Basic Thumbnail Generation

```csharp
var options = new ThumbnailOptions
{
    Count = 5,           // Generate 5 thumbnails
    Width = 320,         // 320px wide
    OutputPath = "/thumbnails",
};

var thumbnails = await processor.GenerateThumbnailsAsync(videoPath, options);

foreach (var thumb in thumbnails)
{
    Console.WriteLine($"Generated: {thumb}");
}
```

### Specific Timestamps

```csharp
var options = new ThumbnailOptions
{
    Timestamps = new List<double> { 1.0, 5.0, 10.0, 30.0 }, // Seconds
    Width = 640,
    Quality = 90,
};

var thumbnails = await processor.GenerateThumbnailsAsync(videoPath, options);
```

### Thumbnail Resize Modes

Like images, thumbnails support multiple resize modes:

#### Fit (Default)

```csharp
var options = new ThumbnailOptions
{
    Width = 320,
    Height = 180, // 16:9
    ResizeMode = ResizeMode.Fit,
};
```

#### Cover with Cropping

```csharp
var options = new ThumbnailOptions
{
    Width = 320,
    Height = 320, // Square
    ResizeMode = ResizeMode.Cover,
};
```

#### Pad with Background

```csharp
var options = new ThumbnailOptions
{
    Width = 320,
    Height = 320,
    ResizeMode = ResizeMode.Pad,
    BackgroundColor = "#000000", // Black bars
};
```

### Focal Point for Thumbnails

Control where cropping occurs in Cover mode:

```csharp
var options = new ThumbnailOptions
{
    Width = 320,
    Height = 320,
    ResizeMode = ResizeMode.Cover,
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.3 }, // Focus on upper-center
};
```

### Filename Patterns

Customize thumbnail filenames:

```csharp
var options = new ThumbnailOptions
{
    Count = 5,
    Width = 320,
    FilePattern = "thumb_{0:D3}.jpg",     // thumb_000.jpg, thumb_001.jpg, ...
};

// Or include timestamp
var options = new ThumbnailOptions
{
    Timestamps = new List<double> { 5.0, 10.0, 15.0 },
    FilePattern = "frame_{1:F1}s.jpg",    // frame_5.0s.jpg, frame_10.0s.jpg, ...
};
```

Pattern placeholders:
- `{0}` - Index (0, 1, 2, ...)
- `{1}` - Timestamp in seconds

## Transcoding Videos

### Basic Transcoding

```csharp
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Codec = VideoCodec.H264,
};

var outputPath = await processor.TranscodeAsync(
    sourcePath,
    destinationPath,
    options
);
```

### Resolution and Bitrate

```csharp
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Width = 1920,
    Height = 1080,
    VideoBitrate = 5000,  // 5 Mbps
    AudioBitrate = 128,   // 128 kbps
};
```

### Quality-Based Encoding (CRF)

Use CRF for quality-based encoding instead of bitrate:

```csharp
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Codec = VideoCodec.H264,
    Crf = 23, // Lower = better quality, larger file
};
```

CRF values:
- 18-20: Visually lossless
- 21-23: High quality (default)
- 24-28: Good quality
- 29+: Lower quality

### Encoding Presets

Balance speed vs compression:

```csharp
var options = new TranscodeOptions
{
    Preset = "medium", // Default
};
```

| Preset | Use Case |
|--------|----------|
| `ultrafast` | Testing, previews |
| `fast` | Quick encodes |
| `medium` | Balanced (default) |
| `slow` | Final delivery |
| `veryslow` | Maximum compression |

### Frame Rate

```csharp
var options = new TranscodeOptions
{
    FrameRate = 30, // 30 fps
};
```

### Strip Audio

```csharp
var options = new TranscodeOptions
{
    StripAudio = true, // Remove audio track
};
```

### Strip Metadata

```csharp
var options = new TranscodeOptions
{
    StripMetadata = true, // Remove all metadata
};
```

### Hardware Acceleration

Enable GPU encoding (if available):

```csharp
var options = new TranscodeOptions
{
    UseHardwareAcceleration = true,
};
```

## Video Formats and Codecs

### Formats

| Format | Extension | Use Case |
|--------|-----------|----------|
| `Mp4` | .mp4 | Universal compatibility |
| `WebM` | .webm | Web streaming |
| `Mkv` | .mkv | High quality archival |

### Codecs

| Codec | Format | Notes |
|-------|--------|-------|
| `H264` | MP4 | Best compatibility |
| `H265` | MP4 | Better compression, less compatible |
| `VP8` | WebM | Older WebM codec |
| `VP9` | WebM | Modern WebM codec |

## Common Scenarios

### Video Hosting Platform

Generate multiple resolutions:

```csharp
var resolutions = new[]
{
    (width: 1920, height: 1080, bitrate: 5000),
    (width: 1280, height: 720, bitrate: 2500),
    (width: 854, height: 480, bitrate: 1000),
    (width: 640, height: 360, bitrate: 500),
};

foreach (var (width, height, bitrate) in resolutions)
{
    var options = new TranscodeOptions
    {
        Format = VideoFormat.Mp4,
        Codec = VideoCodec.H264,
        Width = width,
        Height = height,
        VideoBitrate = bitrate,
        Preset = "slow",
    };

    var output = $"video_{height}p.mp4";
    await processor.TranscodeAsync(sourcePath, output, options);
}
```

### Video Thumbnails for Gallery

```csharp
var options = new ThumbnailOptions
{
    Count = 1,
    Timestamps = new List<double> { 2.0 }, // 2 seconds in
    Width = 400,
    Height = 225, // 16:9
    ResizeMode = ResizeMode.Cover,
    Quality = 85,
    Format = ImageFormat.WebP,
    FilePattern = "poster.webp",
};

var thumbnails = await processor.GenerateThumbnailsAsync(videoPath, options);
```

### Social Media Optimization

```csharp
// Instagram square video
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Width = 1080,
    Height = 1080,
    VideoBitrate = 3500,
    FrameRate = 30,
    Crf = 23,
};

// Twitter video
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Width = 1280,
    Height = 720,
    VideoBitrate = 2500,
    AudioBitrate = 128,
};
```

### Quick Preview Generation

```csharp
var options = new TranscodeOptions
{
    Format = VideoFormat.Mp4,
    Width = 640,
    Height = 360,
    Preset = "ultrafast",
    Crf = 28, // Lower quality for speed
};
```

## Performance Considerations

### File Size vs Quality

| Approach | File Size | Quality | Speed |
|----------|-----------|---------|-------|
| High CRF (28+) | Small | Lower | Fast |
| Low CRF (18-20) | Large | Excellent | Slow |
| High bitrate | Large | Excellent | Fast |
| Slow preset | Small | Excellent | Very slow |

### Parallel Processing

Process multiple videos concurrently:

```csharp
var tasks = videos.Select(video =>
    processor.TranscodeAsync(video, GetOutputPath(video), options)
);

await Task.WhenAll(tasks);
```

### Hardware Acceleration

When available, GPU encoding significantly improves speed:

```csharp
var options = new TranscodeOptions
{
    UseHardwareAcceleration = true, // Use NVENC, QSV, or VideoToolbox
};
```

## Error Handling

```csharp
try
{
    await processor.TranscodeAsync(source, dest, options);
}
catch (FileNotFoundException ex)
{
    logger.LogError("Video file not found: {Path}", source);
}
catch (Exception ex) when (ex.Message.Contains("ffmpeg"))
{
    logger.LogError(ex, "FFmpeg error during transcoding");
}
```

## FFMpegCore Configuration

Configure FFmpeg paths if not in system PATH:

```csharp
GlobalFFOptions.Configure(new FFOptions
{
    BinaryFolder = "/usr/local/bin",
    TemporaryFilesFolder = "/tmp/ffmpeg",
});
```

## Next Steps

- [Image Processing](image-processing.md) - Process images with SkiaSharp
- [Metadata Extraction](metadata-extraction.md) - Extract video metadata
- [Configuration](configuration.md) - Configure default options
