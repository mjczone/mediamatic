# Getting Started with MediaMatic

This guide will help you get started with MediaMatic in just a few minutes.

## Prerequisites

- **.NET 8.0** or later
- **FFmpeg** (for video processing) - [Installation Guide](#ffmpeg-installation)

## Installation

### Core Library

```bash
dotnet add package MJCZone.MediaMatic
```

### ASP.NET Core Integration (Optional)

```bash
dotnet add package MJCZone.MediaMatic.AspNetCore
```

## FFmpeg Installation

FFmpeg is required for video processing features. It's not needed if you only use image processing.

### Ubuntu/Debian
```bash
sudo apt-get install ffmpeg
```

### macOS (Homebrew)
```bash
brew install ffmpeg
```

### Windows (Chocolatey)
```bash
choco install ffmpeg
```

### Docker
```dockerfile
RUN apt-get update && apt-get install -y ffmpeg
```

## Basic Usage

### 1. Create a Storage Provider

MediaMatic uses [FluentStorage](https://github.com/robinrodricks/FluentStorage) for storage abstraction:

```csharp
using FluentStorage;
using MJCZone.MediaMatic;

// AWS S3
var storage = StorageFactory.Blobs.FromConnectionString(
    "aws.s3://keyId=YOUR_KEY;key=YOUR_SECRET;bucket=my-bucket;region=us-east-1"
);

// Azure Blob Storage
var storage = StorageFactory.Blobs.FromConnectionString(
    "azure://accountName=myaccount;accountKey=KEY;container=my-container"
);

// Local File System
var storage = StorageFactory.Blobs.DirectoryFiles("/path/to/storage");

// In-Memory (for testing)
var storage = StorageFactory.Blobs.InMemory();
```

### 2. Upload and Optimize an Image

```csharp
using var fileStream = File.OpenRead("photo.jpg");

var result = await storage.UploadImageAsync(fileStream, "gallery/photo.jpg", options =>
{
    // Generate multiple sizes
    options.GenerateThumbnails(new[] { 320, 640, 960, 1280, 1920 });

    // Generate modern formats
    options.GenerateFormats(ImageFormat.WebP, ImageFormat.Avif);

    // Optimize quality (1-100)
    options.OptimizeQuality(85);

    // Extract metadata
    options.ExtractMetadata();

    // Strip EXIF data for privacy
    options.PreserveExif(false);
});

Console.WriteLine($"Uploaded {result.FilesGenerated} files");
Console.WriteLine($"Original: {result.OriginalSize} bytes");
Console.WriteLine($"Total: {result.TotalSize} bytes");
```

### 3. Process a Video

```csharp
using var videoStream = File.OpenRead("video.mp4");

var result = await storage.ProcessVideoAsync(videoStream, "videos/demo.mp4", options =>
{
    // Generate thumbnail at 3 seconds
    options.GenerateThumbnail(TimeSpan.FromSeconds(3));

    // Generate poster image at 10 seconds
    options.GeneratePosterImage(TimeSpan.FromSeconds(10));

    // Transcode to multiple resolutions
    options.TranscodeTo(VideoFormat.Mp4, resolution: 1080, bitrate: 5000);
    options.TranscodeTo(VideoFormat.Mp4, resolution: 720, bitrate: 2500);
    options.TranscodeTo(VideoFormat.Mp4, resolution: 480, bitrate: 1000);

    // Extract metadata
    options.ExtractMetadata();
});

Console.WriteLine($"Generated {result.Variants.Count} video variants");
Console.WriteLine($"Thumbnail: {result.ThumbnailPath}");
```

### 4. Extract Metadata

```csharp
// Get comprehensive metadata
var metadata = await storage.GetMetadataAsync("gallery/photo.jpg");

Console.WriteLine($"MIME Type: {metadata.MimeType}");
Console.WriteLine($"Dimensions: {metadata.Width}x{metadata.Height}");
Console.WriteLine($"File Size: {metadata.Size} bytes");
Console.WriteLine($"Created: {metadata.CreatedAt}");

// EXIF data (if available)
if (metadata.CustomMetadata.TryGetValue("Camera", out var camera))
{
    Console.WriteLine($"Camera: {camera}");
}

if (metadata.CustomMetadata.TryGetValue("DateTaken", out var dateTaken))
{
    Console.WriteLine($"Date Taken: {dateTaken}");
}
```

## ASP.NET Core Integration

### 1. Register MediaMatic Services

```csharp
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add MediaMatic with filesource repository
builder.Services.AddMediaMatic(options =>
{
    // Use in-memory repository (for development/testing)
    options.UseInMemoryFilesourceRepository();

    // Or use file-based repository
    // options.UseFileFilesourceRepository("./filesources.json");

    // Or use database repository
    // options.UseDatabaseFilesourceRepository(connectionString);

    // Register filesources
    options.AddFilesource("default", "aws.s3://...");
    options.AddFilesource("azure", "azure://...");
});

var app = builder.Build();

// Map MediaMatic API endpoints
app.MapMediaMaticEndpoints();

app.Run();
```

### 2. Use MediaMatic in Controllers/Endpoints

```csharp
app.MapPost("/upload-image", async (
    IFormFile file,
    IMediaStorage storage) =>
{
    using var stream = file.OpenReadStream();

    var result = await storage.UploadImageAsync(stream, $"uploads/{file.FileName}", opt =>
    {
        opt.GenerateThumbnails(new[] { 320, 640, 1280 });
        opt.GenerateFormats(ImageFormat.WebP);
        opt.OptimizeQuality(85);
    });

    return Results.Ok(new
    {
        message = "Upload successful",
        filesGenerated = result.FilesGenerated,
        originalPath = result.OriginalPath,
        thumbnails = result.Thumbnails
    });
});
```

### 3. Browser-Aware Image Serving

```csharp
app.MapGet("/images/{*path}", async (
    string path,
    HttpContext context,
    IMediaStorage storage) =>
{
    var optimizedImage = await storage.GetOptimizedImageAsync(
        path,
        userAgent: context.Request.Headers["User-Agent"].ToString(),
        accept: context.Request.Headers["Accept"].ToString()
    );

    return Results.File(optimizedImage.Stream, optimizedImage.ContentType);
});
```

## Next Steps

- [Learn about Architecture](architecture.md) - Understand MediaMatic's design
- [Image Processing Guide](image-processing.md) - Deep dive into image optimization
- [Video Processing Guide](video-processing.md) - Video processing features
- [Storage Providers](storage-providers.md) - Configure different storage backends
- [ASP.NET Core Integration](aspnetcore-integration.md) - Full web integration guide

## Common Scenarios

### E-Commerce Product Images

```csharp
// Upload product image with responsive variants
var result = await storage.UploadImageAsync(stream, $"products/{sku}/main.jpg", opt =>
{
    opt.GenerateThumbnails(new[] { 100, 300, 600, 1200 }); // Thumbnail, grid, detail, zoom
    opt.GenerateFormats(ImageFormat.WebP, ImageFormat.Avif);
    opt.OptimizeQuality(90); // Higher quality for products
    opt.PreserveExif(false); // Remove camera data
});
```

### User Avatar Upload

```csharp
// Upload avatar with square crop
var result = await storage.UploadImageAsync(stream, $"avatars/{userId}.jpg", opt =>
{
    opt.Crop(CropMode.Square); // Force square aspect ratio
    opt.GenerateThumbnails(new[] { 32, 64, 128, 256 });
    opt.GenerateFormats(ImageFormat.WebP);
    opt.OptimizeQuality(80);
    opt.PreserveExif(false);
});
```

### Video Tutorial Upload

```csharp
// Upload tutorial video with thumbnails
var result = await storage.ProcessVideoAsync(stream, $"tutorials/{id}/video.mp4", opt =>
{
    opt.GenerateThumbnail(TimeSpan.FromSeconds(5));
    opt.GeneratePosterImage(TimeSpan.FromSeconds(15));
    opt.TranscodeTo(VideoFormat.Mp4, resolution: 720); // Standardize to 720p
    opt.ExtractMetadata();
});
```

## Troubleshooting

### FFmpeg Not Found

If you get an error about FFmpeg not being found:

1. Install FFmpeg (see [FFmpeg Installation](#ffmpeg-installation))
2. Ensure FFmpeg is in your system PATH
3. Restart your application/terminal

### Memory Issues with Large Files

For very large files, use streaming:

```csharp
// Don't load entire file into memory
using var stream = File.OpenRead(largePath);
await storage.UploadImageAsync(stream, path, opt => { ... });
```

### Slow Processing

Image/video processing can be CPU-intensive. Consider:

1. **Parallel Processing** - Process multiple files concurrently
2. **Background Jobs** - Use Hangfire or similar for async processing
3. **Cloud Functions** - Offload processing to AWS Lambda, Azure Functions

## Getting Help

- [GitHub Issues](https://github.com/mjczone/MJCZone.MediaMatic/issues) - Bug reports
- [GitHub Discussions](https://github.com/mjczone/MJCZone.MediaMatic/discussions) - Questions
- [API Reference](../api/MJCZone.MediaMatic.md) - Detailed API documentation
