# Image Processing

MediaMatic uses [SkiaSharp](https://github.com/mono/SkiaSharp) for image processing, providing high-performance operations with MIT licensing.

## Why SkiaSharp?

| Library | License | Cost for $1M+ Revenue |
|---------|---------|------------------------|
| **SkiaSharp** | MIT | **Free forever** |
| ImageSharp | Commercial | $799-4,999/year |

SkiaSharp is battle-tested (used by Xamarin, Blazor) and offers comparable performance to ImageSharp.

## Supported Formats

| Format | Read | Write | Notes |
|--------|------|-------|-------|
| JPEG | ✅ | ✅ | Lossy compression |
| PNG | ✅ | ✅ | Lossless, transparency |
| WebP | ✅ | ✅ | Modern format, best compression |
| GIF | ✅ | ✅ | Animation support |
| BMP | ✅ | ✅ | Uncompressed |
| HEIC | ✅ | ❌ | Apple format (read-only) |

## ImageProcessor

The `ImageProcessor` class provides all image operations:

```csharp
using MJCZone.MediaMatic.Processors;

var processor = new ImageProcessor();
```

## Resizing Images

### Basic Resize

```csharp
using var inputStream = File.OpenRead("photo.jpg");

// Resize by width (height maintains aspect ratio)
var result = await processor.ResizeAsync(inputStream, width: 800, height: null);

// Resize by height (width maintains aspect ratio)
var result = await processor.ResizeAsync(inputStream, width: null, height: 600);

// Resize to exact dimensions
var result = await processor.ResizeAsync(inputStream, width: 800, height: 600);
```

### Resize Modes

MediaMatic supports four resize modes:

#### Fit (Default)

Scale to fit within bounds, maintaining aspect ratio. One dimension matches target, other is smaller.

```csharp
var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Fit };
var result = await processor.ResizeAsync(stream, 400, 400, options);
// 800x600 image → 400x300 (fits within 400x400)
```

#### Cover

Scale to cover bounds, then crop excess. Both dimensions match target exactly.

```csharp
var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Cover };
var result = await processor.ResizeAsync(stream, 400, 400, options);
// 800x600 image → 400x400 (cropped to fill)
```

#### Pad

Scale to fit within bounds, then add background padding. Both dimensions match target exactly.

```csharp
var options = new ImageProcessingOptions
{
    ResizeMode = ResizeMode.Pad,
    BackgroundColor = "#FFFFFF", // White padding
};
var result = await processor.ResizeAsync(stream, 400, 400, options);
// 800x600 image → 400x400 (with white bars)
```

#### Stretch

Stretch to exact dimensions, ignoring aspect ratio.

```csharp
var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Stretch };
var result = await processor.ResizeAsync(stream, 400, 400, options);
// 800x600 image → 400x400 (distorted)
```

## Focal Point Cropping

When using `Cover` mode, specify a focal point to control where cropping occurs:

```csharp
var options = new ImageProcessingOptions
{
    ResizeMode = ResizeMode.Cover,
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.3 }, // Upper-center
};
var result = await processor.ResizeAsync(stream, 400, 400, options);
```

### Focal Point Values

| Position | X | Y |
|----------|---|---|
| Top-Left | 0.0 | 0.0 |
| Top-Center | 0.5 | 0.0 |
| Top-Right | 1.0 | 0.0 |
| Center-Left | 0.0 | 0.5 |
| Center (default) | 0.5 | 0.5 |
| Center-Right | 1.0 | 0.5 |
| Bottom-Left | 0.0 | 1.0 |
| Bottom-Center | 0.5 | 1.0 |
| Bottom-Right | 1.0 | 1.0 |

### Example: Portrait Photo

For a portrait where the face is in the upper third:

```csharp
var options = new ImageProcessingOptions
{
    ResizeMode = ResizeMode.Cover,
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.33 },
};
```

## Format Conversion

Convert between image formats:

```csharp
using var inputStream = File.OpenRead("photo.jpg");

// JPEG to PNG
var result = await processor.ConvertFormatAsync(inputStream, ImageFormat.Png, quality: 100);

// JPEG to WebP (better compression)
var result = await processor.ConvertFormatAsync(inputStream, ImageFormat.WebP, quality: 85);
```

### Quality Settings

| Format | Quality Range | Notes |
|--------|---------------|-------|
| JPEG | 1-100 | Lower = smaller file, more artifacts |
| PNG | N/A | Lossless, quality setting ignored |
| WebP | 1-100 | Similar to JPEG but better compression |

## Common Scenarios

### E-Commerce Product Images

```csharp
var options = new ImageProcessingOptions
{
    Format = ImageFormat.WebP,
    Quality = 90,
    ResizeMode = ResizeMode.Pad,
    BackgroundColor = "#FFFFFF",
};

// Generate multiple sizes
var sizes = new[] { 100, 300, 600, 1200 };
var results = new List<ImageResult>();

foreach (var size in sizes)
{
    stream.Position = 0;
    var result = await processor.ResizeAsync(stream, size, size, options);
    results.Add(result);
}
```

### User Avatars

```csharp
var options = new ImageProcessingOptions
{
    Format = ImageFormat.WebP,
    Quality = 80,
    ResizeMode = ResizeMode.Cover, // Square crop
};

// Standard avatar sizes
var result = await processor.ResizeAsync(stream, 256, 256, options);
```

### Blog Post Hero Images

```csharp
var options = new ImageProcessingOptions
{
    Format = ImageFormat.WebP,
    Quality = 85,
    ResizeMode = ResizeMode.Cover,
    FocalPoint = new FocalPoint { X = 0.5, Y = 0.5 },
};

// 16:9 aspect ratio
var result = await processor.ResizeAsync(stream, 1920, 1080, options);
```

### Thumbnail Grid

```csharp
var options = new ImageProcessingOptions
{
    Format = ImageFormat.WebP,
    Quality = 75,
    ResizeMode = ResizeMode.Cover,
};

// Square thumbnails for grid layout
var result = await processor.ResizeAsync(stream, 300, 300, options);
```

## Processing Results

All operations return an `ImageResult`:

```csharp
var result = await processor.ResizeAsync(stream, 800, null);

Console.WriteLine($"Width: {result.width}");
Console.WriteLine($"Height: {result.height}");
Console.WriteLine($"File Size: {result.fileSize} bytes");
Console.WriteLine($"Format: {result.format}");

// Save to file
using var outputStream = File.Create("output.webp");
result.stream.CopyTo(outputStream);
```

## Performance Tips

### Parallel Processing

Process multiple images concurrently:

```csharp
var tasks = images.Select(async img =>
{
    using var stream = File.OpenRead(img);
    return await processor.ResizeAsync(stream, 800, null);
});

var results = await Task.WhenAll(tasks);
```

### Stream Management

Dispose of streams properly:

```csharp
using var inputStream = File.OpenRead(path);
var result = await processor.ResizeAsync(inputStream, 800, null);

// Result contains a stream that must be used before disposal
using var outputStream = File.Create(outputPath);
result.stream.CopyTo(outputStream);
```

### Memory Efficiency

For very large images, process in chunks:

```csharp
// Resize large image in steps to reduce memory peak
var result1 = await processor.ResizeAsync(stream, 4000, null); // First pass
result1.stream.Position = 0;
var result2 = await processor.ResizeAsync(result1.stream, 2000, null); // Second pass
```

## Error Handling

```csharp
try
{
    var result = await processor.ResizeAsync(stream, width, height);
}
catch (ArgumentException ex)
{
    // Invalid parameters
    logger.LogError(ex, "Invalid resize parameters");
}
catch (NotSupportedException ex)
{
    // Unsupported format
    logger.LogError(ex, "Image format not supported");
}
```

## Next Steps

- [Video Processing](video-processing.md) - Generate thumbnails and transcode videos
- [Metadata Extraction](metadata-extraction.md) - Extract EXIF and other metadata
- [Configuration](configuration.md) - Configure default options
