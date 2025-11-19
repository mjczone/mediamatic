# MediaMatic Documentation

**Intelligent Media Storage & Optimization for .NET**

Welcome to the MediaMatic documentation! MediaMatic is a comprehensive file storage and media processing library that integrates 6 best-in-class .NET libraries to provide intelligent image optimization, video processing, and browser-aware format serving.

## What is MediaMatic?

MediaMatic builds on [FluentStorage](https://github.com/robinrodricks/FluentStorage) to add:

- **Intelligent Metadata Management** - Auto-detect MIME types, extract EXIF/video metadata
- **Automatic Image Optimization** - Upload JPEG → auto-generate WebP/AVIF + responsive sizes
- **Video Processing** - Thumbnail generation, transcoding, format standardization
- **Browser-Aware Serving** - Serve optimal format based on browser capabilities
- **Unified API** - One fluent interface instead of learning 6 different libraries

## Quick Start

```bash
# Install core library
dotnet add package MJCZone.MediaMatic

# Install ASP.NET Core integration (optional)
dotnet add package MJCZone.MediaMatic.AspNetCore
```

```csharp
using MJCZone.MediaMatic;
using FluentStorage;

// Create storage provider
var storage = StorageFactory.Blobs.FromConnectionString(
    "aws.s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1"
);

// Upload with auto-optimization
var result = await storage.UploadImageAsync(stream, "products/shoe.jpg", options =>
{
    options.GenerateThumbnails(new[] { 100, 300, 600, 1200 });
    options.GenerateFormats(ImageFormat.WebP, ImageFormat.Avif);
    options.OptimizeQuality(85);
    options.ExtractMetadata();
});
```

## Key Features

### 🖼️ Image Processing
- Format conversion (JPEG, PNG, WebP, AVIF)
- Responsive image generation (multiple sizes)
- Quality optimization
- EXIF metadata extraction and preservation

### 🎬 Video Processing
- Thumbnail and poster frame generation
- Video transcoding and format conversion
- Resolution scaling
- Metadata extraction

### 🌐 Browser-Aware Optimization
- Automatic format negotiation (AVIF/WebP/JPEG)
- User-Agent detection
- Accept header parsing
- Optimal format serving per browser

### ☁️ Multi-Provider Storage
- AWS S3, Azure Blob, Google Cloud Storage
- MinIO, DigitalOcean Spaces, Backblaze B2
- Local file system, SFTP
- 13 storage providers supported

## Documentation Sections

### Getting Started
- [Installation & Setup](guide/getting-started.md)
- [Architecture Overview](guide/architecture.md)
- [Storage Providers](guide/storage-providers.md)

### Guides
- [Image Processing](guide/image-processing.md)
- [Video Processing](guide/video-processing.md)
- [Metadata Extraction](guide/metadata-extraction.md)
- [Browser Detection](guide/browser-detection.md)
- [ASP.NET Core Integration](guide/aspnetcore-integration.md)

### API Reference
- [Core Library API](api/MJCZone.MediaMatic.md)
- [ASP.NET Core API](api/MJCZone.MediaMatic.AspNetCore.md)

## Related Libraries

MediaMatic is part of the **MJCZone *Matic** family:

- **[DapperMatic](https://github.com/mjczone/MJCZone.DapperMatic)** - Database schema management

## Support

- [GitHub Issues](https://github.com/mjczone/MJCZone.MediaMatic/issues) - Bug reports and feature requests
- [GitHub Discussions](https://github.com/mjczone/MJCZone.MediaMatic/discussions) - Questions and community chat

## License

Licensed under [LGPL-3.0-or-later](https://github.com/mjczone/MJCZone.MediaMatic/blob/main/LICENSE)
