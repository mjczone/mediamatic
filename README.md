# MediaMatic

> ⚠️ **UNDER DEVELOPMENT - BREAKING CHANGES EXPECTED**
>
> This library is in active development (v0.x.x). The API is not yet stable and breaking changes may occur between releases. Do not use in production until v1.0.0 is released.

**Intelligent Media Storage & Optimization Library for .NET**

MediaMatic is a file storage and media processing library that provides a unified Virtual File System (VFS) abstraction with intelligent image optimization, video processing, and metadata extraction.

[![.github/workflows/build-and-test.yml](https://github.com/mjczone/mediamatic/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mjczone/mediamatic/actions/workflows/build-and-test.yml)
[![.github/workflows/release.yml](https://github.com/mjczone/mediamatic/actions/workflows/release.yml/badge.svg)](https://github.com/mjczone/mediamatic/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/MJCZone.MediaMatic.svg?label=NuGet)](https://www.nuget.org/packages/MJCZone.MediaMatic/)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/download)
[![License: LGPL v3](https://img.shields.io/badge/License-LGPL_v3-blue.svg)](LICENSE)

📚 **[Full Documentation](https://mediamatic.mjczone.com/)**

---

## Why MediaMatic?

MediaMatic integrates 6 best-in-class .NET libraries into a unified, fluent API:

| Library | Purpose | License |
|---------|---------|---------|
| [FluentStorage](https://github.com/robinrodricks/FluentStorage) | Storage abstraction (13+ providers) | MIT |
| [SkiaSharp](https://github.com/mono/SkiaSharp) | Image processing | MIT |
| [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore) | Video processing | MIT |
| [MimeDetective](https://github.com/MediatedCommunications/Mime-Detective) | Content-based MIME detection | MIT |
| [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) | EXIF/metadata extraction | Apache-2.0 |
| [DeviceDetector.NET](https://github.com/totpero/DeviceDetector.NET) | Browser/device detection | Apache-2.0 |

### Key Features

- **Multi-Provider Storage** - AWS S3, Google Cloud, MinIO, Backblaze B2, SFTP, local disk, and more
- **Image Processing** - Resize, convert (JPEG/PNG/WebP/AVIF), optimize with focal point cropping
- **Video Processing** - Thumbnail generation, transcoding, metadata extraction
- **Intelligent Metadata** - Auto-detect MIME types, extract EXIF/GPS data
- **REST API** - Ready-to-use ASP.NET Core endpoints for file management
- **URL-based Transformations** - Cloudinary-style image transformations via URL parameters

---

## Installation

### Core Library
```bash
dotnet add package MJCZone.MediaMatic
```

### ASP.NET Core Integration
```bash
dotnet add package MJCZone.MediaMatic.AspNetCore
```

### Prerequisites

- **.NET 8.0** or later
- **FFmpeg** (for video processing) - [Installation instructions](https://ffmpeg.org/download.html)

---

## Supported Storage Providers

- In-Memory (for testing)
- Local File System
- SFTP
- AWS S3
- MinIO (S3-compatible)
- Backblaze B2 (S3-compatible)
- Google Cloud Storage

MediaMatic supports multiple storage providers via FluentStorage:

```csharp
// In-Memory (for testing)
using var memory = VfsConnection.Create(
    VfsProviderType.Memory,
    "memory://name=test"
);

// Local File System
using var local = VfsConnection.Create(
    VfsProviderType.Local,
    "/var/media"
);

// SFTP
using var sftp = VfsConnection.Create(
    VfsProviderType.SFTP,
    "sftp://host=sftp.example.com;username=user;password=pass;root=/uploads"
);

// AWS S3
using var s3 = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1"
);

// MinIO (S3-compatible)
using var minio = VfsConnection.Create(
    VfsProviderType.Minio,
    "minio://endpoint=localhost:9000;accessKey=...;secretKey=...;bucket=my-bucket"
);

// Backblaze B2
using var b2 = VfsConnection.Create(
    VfsProviderType.B2,
    "b2://keyId=...;applicationKey=...;bucket=my-bucket"
);

// Google Cloud Storage
using var gcp = VfsConnection.Create(
    VfsProviderType.GCP,
    "gcp://project=my-project;bucket=my-bucket;jsonKeyPath=/path/to/key.json"
);
```

---

## Quick Start

### Core Library Usage

```csharp
using MJCZone.MediaMatic;
using MJCZone.MediaMatic.Models;

// Create a VFS connection to your storage provider
using var vfs = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=YOUR_KEY;key=YOUR_SECRET;bucket=my-bucket;region=us-east-1"
);

// Or use local storage
using var local = VfsConnection.Create(
    VfsProviderType.Local,
    "/path/to/storage"
);

// Upload and optimize an image
using var fileStream = File.OpenRead("photo.jpg");
var result = await vfs.UploadImageAsync(fileStream, "gallery/photo.jpg", new ImageUploadOptions
{
    GenerateThumbnails = true,
    ThumbnailSizes = [320, 640, 1280],
    GenerateFormats = true,
    Formats = [ImageFormat.WebP],
    JpegQuality = 85,
});

Console.WriteLine($"Uploaded: {result.Path}");
Console.WriteLine($"Dimensions: {result.Width}x{result.Height}");

// Extract metadata
var metadata = await vfs.GetMetadataAsync("gallery/photo.jpg");
Console.WriteLine($"MIME: {metadata.MimeType}");
```

### ASP.NET Core Integration

```csharp
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add MediaMatic services
builder.Services.AddMediaMatic();

var app = builder.Build();

// Configure MediaMatic (adds middleware and maps REST API endpoints)
app.UseMediaMatic();

app.Run();
```

**Advanced: Manual endpoint mapping** (if you need to control middleware separately):

```csharp
// Just map endpoints without middleware
app.MapMediaMaticEndpoints();
```

### REST API Examples

```bash
# Upload a file
curl -X POST http://localhost:5000/api/mm/fs/default/files/products/shoe.jpg \
  --data-binary @shoe.jpg

# Download a file
curl http://localhost:5000/api/mm/fs/default/files/products/shoe.jpg -o shoe.jpg

# Transform on-the-fly (resize to 400px width, convert to WebP)
curl http://localhost:5000/api/mm/fs/default/transform/w_400,f_webp/products/shoe.jpg -o thumb.webp

# Get metadata
curl http://localhost:5000/api/mm/fs/default/metadata/products/shoe.jpg

# Browse files
curl http://localhost:5000/api/mm/fs/default/browse/products
```

---

## Documentation

📖 **[Full Documentation](https://mediamatic.mjczone.com/)** - Comprehensive guides and API reference

- [Getting Started](https://mediamatic.mjczone.com/guide/getting-started.html)
- [Storage Providers](https://mediamatic.mjczone.com/guide/storage-providers.html)
- [Image Processing](https://mediamatic.mjczone.com/guide/image-processing.html)
- [Video Processing](https://mediamatic.mjczone.com/guide/video-processing.html)
- [ASP.NET Core Integration](https://mediamatic.mjczone.com/guide/aspnetcore-integration.html)
- [Transformation URL API](https://mediamatic.mjczone.com/guide/transformation-url-api.html)

---

## Related Projects

MediaMatic is part of the **MJCZone *Matic** family:

- **[DapperMatic](https://github.com/mjczone/dappermatic)** - Database schema management for Dapper

---

## License

Licensed under the [GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)](LICENSE).

**Integrated Libraries** (all with permissive licenses):
- FluentStorage (MIT)
- SkiaSharp (MIT)
- FFMpegCore (MIT)
- MimeDetective (MIT)
- MetadataExtractor (Apache-2.0)
- DeviceDetector.NET (Apache-2.0)
- TagLibSharp (LGPL-2.1)

---

## Support

- 🐛 **Bug Reports** - [GitHub Issues](https://github.com/mjczone/mediamatic/issues)
- 💬 **Discussions** - [GitHub Discussions](https://github.com/mjczone/mediamatic/discussions)
- 💻 **Contributing** - See [CONTRIBUTING.md](CONTRIBUTING.md) for current contribution guidelines

---

<div align="center">

**Built with ❤️ by MJCZone Inc.**

[Website](https://mjczone.com) • [GitHub](https://github.com/mjczone) • [NuGet](https://www.nuget.org/profiles/mjczone)

</div>
