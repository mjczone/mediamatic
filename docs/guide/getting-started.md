# Getting Started

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

### 1. Create a VFS Connection

MediaMatic uses a Virtual File System (VFS) abstraction for storage:

```csharp
using MJCZone.MediaMatic;
using MJCZone.MediaMatic.Models;

// AWS S3
using var s3 = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=YOUR_KEY;key=YOUR_SECRET;bucket=my-bucket;region=us-east-1"
);

// MinIO (S3-compatible)
using var minio = VfsConnection.Create(
    VfsProviderType.Minio,
    "minio://endpoint=localhost:9000;accessKey=minioadmin;secretKey=minioadmin;bucket=my-bucket"
);

// Local File System
using var local = VfsConnection.Create(
    VfsProviderType.Local,
    "/path/to/storage"
);

// In-Memory (for testing)
using var memory = VfsConnection.Create(
    VfsProviderType.Memory,
    "memory://name=test"
);
```

### 2. Upload and Optimize an Image

```csharp
using var fileStream = File.OpenRead("photo.jpg");

var result = await vfs.UploadImageAsync(fileStream, "gallery/photo.jpg", new ImageUploadOptions
{
    // Generate multiple sizes
    GenerateThumbnails = true,
    ThumbnailSizes = [320, 640, 960, 1280, 1920],

    // Generate modern formats
    GenerateFormats = true,
    Formats = [ImageFormat.WebP, ImageFormat.Avif],

    // Optimize quality (1-100)
    JpegQuality = 85,
    WebPQuality = 80,

    // Strip EXIF data for privacy
    PreserveExif = false,
});

Console.WriteLine($"Uploaded: {result.Path}");
Console.WriteLine($"Dimensions: {result.Width}x{result.Height}");
Console.WriteLine($"File Size: {result.FileSize} bytes");
Console.WriteLine($"Generated {result.Variants.Count} variants");
```

### 3. Process a Video

```csharp
using var videoStream = File.OpenRead("video.mp4");

var result = await vfs.UploadVideoAsync(videoStream, "videos/demo.mp4", new VideoUploadOptions
{
    // Generate thumbnails
    GenerateThumbnails = true,
    ThumbnailCount = 3,

    // Extract metadata
    ExtractMetadata = true,
});

Console.WriteLine($"Duration: {result.Duration} seconds");
Console.WriteLine($"Dimensions: {result.Width}x{result.Height}");
Console.WriteLine($"Generated {result.Thumbnails?.Count ?? 0} thumbnails");
```

### 4. Extract Metadata

```csharp
// Get file metadata
var metadata = await vfs.GetMetadataAsync("gallery/photo.jpg");

Console.WriteLine($"MIME Type: {metadata.MimeType}");
Console.WriteLine($"Dimensions: {metadata.Width}x{metadata.Height}");
Console.WriteLine($"File Size: {metadata.Size} bytes");
Console.WriteLine($"Provider: {metadata.Provider}");

// EXIF data (if available)
if (metadata.CameraMake != null)
{
    Console.WriteLine($"Camera: {metadata.CameraMake} {metadata.CameraModel}");
}

if (metadata.DateTimeOriginal != null)
{
    Console.WriteLine($"Date Taken: {metadata.DateTimeOriginal}");
}

if (metadata.Latitude != null && metadata.Longitude != null)
{
    Console.WriteLine($"Location: {metadata.Latitude}, {metadata.Longitude}");
}
```

### 5. Process Images

```csharp
// Resize an existing image
var resizeResult = await vfs.ProcessImageAsync(
    "gallery/photo.jpg",
    "gallery/photo_thumb.jpg",
    new ImageProcessingOptions
    {
        Width = 300,
        Height = 200,
        Format = ImageFormat.WebP,
        Quality = 80,
    }
);

Console.WriteLine($"Resized to: {resizeResult.Width}x{resizeResult.Height}");
```

## ASP.NET Core Integration

### 1. Register MediaMatic Services

```csharp
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add MediaMatic services
builder.Services.AddMediaMatic();

var app = builder.Build();

// Map MediaMatic REST API endpoints
app.MapMediaMaticFilesourceEndpoints();  // Filesource CRUD
app.MapMediaMaticFileEndpoints();         // File upload/download
app.MapMediaMaticFolderEndpoints();       // Folder operations
app.MapMediaMaticTransformationEndpoints(); // Image transformations
app.MapMediaMaticMetadataEndpoints();     // Metadata extraction

app.Run();
```

### 2. Configure Filesources

Filesources define storage backends for your application:

```csharp
builder.Services.AddMediaMatic(options =>
{
    // Use in-memory repository (for development/testing)
    options.UseInMemoryFilesourceRepository();
});
```

Or configure via the REST API:

```bash
# Create a filesource
curl -X POST http://localhost:5000/api/mm/fs \
  -H "Content-Type: application/json" \
  -d '{
    "id": "images",
    "provider": "S3",
    "connectionString": "s3://keyId=...;key=...;bucket=my-bucket",
    "displayName": "Image Storage",
    "isEnabled": true
  }'
```

### 3. Use the REST API

```bash
# Upload a file
curl -X POST http://localhost:5000/api/mm/fs/images/files/products/shoe.jpg \
  --data-binary @shoe.jpg

# Download a file
curl http://localhost:5000/api/mm/fs/images/files/products/shoe.jpg -o shoe.jpg

# Get file metadata
curl http://localhost:5000/api/mm/fs/images/metadata/products/shoe.jpg

# Transform an image (resize to 400px width, convert to WebP)
curl http://localhost:5000/api/mm/fs/images/transform/w_400,f_webp/products/shoe.jpg -o shoe_thumb.webp

# Browse files in a folder
curl http://localhost:5000/api/mm/fs/images/browse/products
```

## Next Steps

- [Image Processing Guide](image-processing.md) - Deep dive into image optimization
- [Video Processing Guide](video-processing.md) - Video processing features
- [Storage Providers](storage-providers.md) - Configure different storage backends
- [ASP.NET Core Integration](aspnetcore-integration.md) - Full web integration guide

## Common Scenarios

### E-Commerce Product Images

```csharp
// Upload product image with responsive variants
var result = await vfs.UploadImageAsync(stream, $"products/{sku}/main.jpg", new ImageUploadOptions
{
    ThumbnailSizes = [100, 300, 600, 1200], // Thumbnail, grid, detail, zoom
    Formats = [ImageFormat.WebP, ImageFormat.Avif],
    JpegQuality = 90, // Higher quality for products
    PreserveExif = false, // Remove camera data
});
```

### User Avatar Upload

```csharp
// Upload avatar
var result = await vfs.UploadImageAsync(stream, $"avatars/{userId}.jpg", new ImageUploadOptions
{
    ThumbnailSizes = [32, 64, 128, 256],
    Formats = [ImageFormat.WebP],
    JpegQuality = 80,
    PreserveExif = false,
    MaxWidth = 512, // Limit size
    MaxHeight = 512,
});
```

### Video Tutorial Upload

```csharp
// Upload tutorial video with thumbnails
var result = await vfs.UploadVideoAsync(stream, $"tutorials/{id}/video.mp4", new VideoUploadOptions
{
    GenerateThumbnails = true,
    ThumbnailCount = 5,
    ExtractMetadata = true,
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
await vfs.UploadImageAsync(stream, path, options);
```

### Slow Processing

Image/video processing can be CPU-intensive. Consider:

1. **Parallel Processing** - Process multiple files concurrently
2. **Background Jobs** - Use Hangfire or similar for async processing
3. **Cloud Functions** - Offload processing to AWS Lambda, Google Cloud Functions

## Getting Help

- [GitHub Issues](https://github.com/mjczone/mediamatic/issues) - Bug reports
- [GitHub Discussions](https://github.com/mjczone/mediamatic/discussions) - Questions
- [API Reference](../api/MJCZone.MediaMatic.md) - Detailed API documentation
