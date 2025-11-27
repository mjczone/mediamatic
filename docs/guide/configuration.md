# Configuration

MediaMatic provides flexible configuration options for customizing behavior.

## Image Processing Options

The `ImageProcessingOptions` class controls image processing behavior:

```csharp
var options = new ImageProcessingOptions
{
    // Output format
    Format = ImageFormat.WebP,

    // Quality (1-100)
    Quality = 85,

    // Resize mode
    ResizeMode = ResizeMode.Cover,

    // Background color for Pad mode (hex)
    BackgroundColor = "#FFFFFF",

    // Focal point for Cover mode (0.0-1.0)
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.3 },

    // Strip EXIF metadata
    StripMetadata = false,
};
```

### Resize Modes

| Mode | Behavior |
|------|----------|
| `Fit` | Scale to fit within bounds, maintain aspect ratio |
| `Cover` | Scale to cover bounds, crop excess |
| `Pad` | Scale to fit, add background padding |
| `Stretch` | Stretch to exact dimensions (distorts) |

### Focal Point

The focal point determines the center of cropping in `Cover` mode:

```csharp
// Crop from top-left corner
options.FocalPoint = new FocalPoint { X = 0.0, Y = 0.0 };

// Crop from center (default)
options.FocalPoint = new FocalPoint { X = 0.5, Y = 0.5 };

// Crop from bottom-right
options.FocalPoint = new FocalPoint { X = 1.0, Y = 1.0 };

// Focus on a face in upper-third
options.FocalPoint = new FocalPoint { X = 0.5, Y = 0.33 };
```

## Thumbnail Options

The `ThumbnailOptions` class controls video thumbnail generation:

```csharp
var options = new ThumbnailOptions
{
    // Number of thumbnails to generate
    Count = 5,

    // Or specific timestamps (seconds)
    Timestamps = new List<double> { 1.0, 5.0, 10.0 },

    // Thumbnail dimensions
    Width = 320,
    Height = 180, // null = maintain aspect ratio

    // Resize mode
    ResizeMode = ResizeMode.Cover,

    // Background color for Pad mode
    BackgroundColor = "#000000",

    // Focal point for Cover mode
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.5 },

    // Output format
    Format = ImageFormat.Jpeg,

    // Quality (1-100)
    Quality = 85,

    // Output directory (null = same as video)
    OutputPath = "/thumbnails",

    // Filename pattern ({0}=index, {1}=timestamp)
    FilePattern = "thumb_{0}.jpg",
};
```

## Transcode Options

The `TranscodeOptions` class controls video transcoding:

```csharp
var options = new TranscodeOptions
{
    // Output format
    Format = VideoFormat.Mp4,

    // Video codec
    Codec = VideoCodec.H264,

    // Resolution
    Width = 1920,
    Height = 1080,

    // Bitrates (kbps)
    VideoBitrate = 5000,
    AudioBitrate = 128,

    // Frame rate
    FrameRate = 30,

    // Constant Rate Factor (quality, lower = better)
    Crf = 23,

    // Encoding preset
    Preset = "medium", // ultrafast, fast, medium, slow, veryslow

    // Remove audio track
    StripAudio = false,

    // Remove metadata
    StripMetadata = false,

    // Use hardware acceleration
    UseHardwareAcceleration = false,
};
```

### Video Codecs

| Codec | Use Case |
|-------|----------|
| `H264` | Best compatibility, good quality |
| `H265` | Better compression, less compatible |
| `VP8` | WebM format, older |
| `VP9` | WebM format, modern |

### Encoding Presets

| Preset | Speed | File Size | Quality |
|--------|-------|-----------|---------|
| `ultrafast` | Fastest | Largest | Lowest |
| `fast` | Fast | Large | Low |
| `medium` | Balanced | Medium | Good |
| `slow` | Slow | Small | High |
| `veryslow` | Slowest | Smallest | Highest |

## ASP.NET Core Configuration

### Service Registration

```csharp
builder.Services.AddMediaMatic(options =>
{
    // Default image processing options
    options.DefaultImageOptions = new ImageProcessingOptions
    {
        Quality = 85,
        Format = ImageFormat.WebP,
    };

    // Default thumbnail options
    options.DefaultThumbnailOptions = new ThumbnailOptions
    {
        Count = 3,
        Width = 320,
        Quality = 85,
    };
});
```

### Configuration from appsettings.json

```json
{
  "MediaMatic": {
    "DefaultQuality": 85,
    "DefaultFormat": "WebP",
    "ThumbnailSizes": [100, 300, 600, 1200],
    "EnableHardwareAcceleration": false
  }
}
```

```csharp
builder.Services.Configure<MediaMaticOptions>(
    builder.Configuration.GetSection("MediaMatic")
);
```

## Storage Provider Configuration

### AWS S3

```csharp
using MJCZone.MediaMatic;

using var vfs = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=AKIAIOSFODNN7EXAMPLE;key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;bucket=my-bucket;region=us-east-1"
);
```

Environment variables:
```bash
AWS_ACCESS_KEY_ID=AKIAIOSFODNN7EXAMPLE
AWS_SECRET_ACCESS_KEY=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
AWS_DEFAULT_REGION=us-east-1
```

### Local File System

```csharp
using var vfs = VfsConnection.Create(
    VfsProviderType.Local,
    "/var/media"
);
```

See [Storage Providers](storage-providers.md) for detailed configuration of all providers.

## Performance Tuning

### Parallel Processing

```csharp
// Process multiple files concurrently
var tasks = files.Select(f => processor.ProcessAsync(f, options));
await Task.WhenAll(tasks);
```

### Memory Management

```csharp
// For large files, use streaming
using var stream = File.OpenRead(largePath);
await processor.ProcessAsync(stream, options);

// Dispose of results when done
using var result = await processor.ResizeAsync(stream, width, height);
```

### Caching

```csharp
// Cache processed results
builder.Services.AddMemoryCache();
builder.Services.AddMediaMatic(options =>
{
    options.EnableResultCaching = true;
    options.CacheDuration = TimeSpan.FromHours(1);
});
```

## Next Steps

- [Image Processing](image-processing.md) - Detailed image operations
- [Video Processing](video-processing.md) - Video processing guide
- [Storage Providers](storage-providers.md) - Provider configuration
