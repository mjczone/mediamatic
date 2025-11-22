---
layout: home

hero:
  name: "MediaMatic"
  text: "Intelligent Media Storage & Optimization"
  tagline: A comprehensive .NET library for media storage, optimization, and processing
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/mjczone/mediamatic

features:
  - icon: 🖼️
    title: Image Processing
    details: Resize, convert, and optimize images with SkiaSharp. Support for JPEG, PNG, WebP, AVIF with multiple resize modes and focal point cropping.
  - icon: 🎬
    title: Video Processing
    details: Generate thumbnails, transcode videos, and extract metadata using FFMpegCore. Support for multiple formats and resolutions.
  - icon: ☁️
    title: Multi-Provider Storage
    details: Built on FluentStorage with 10+ storage providers including AWS S3, Google Cloud, MinIO, Backblaze B2, SFTP, and local file system.
  - icon: 🔍
    title: Intelligent Metadata
    details: Auto-detect MIME types with MimeDetective, extract EXIF data with MetadataExtractor, and read audio/video tags with TagLibSharp.
  - icon: 🌐
    title: Browser-Aware Serving
    details: Serve optimal image formats based on browser capabilities using DeviceDetector.NET for intelligent format negotiation.
  - icon: ⚡
    title: ASP.NET Core Integration
    details: Ready-to-use middleware, DI integration, and minimal API endpoints for web applications.
---

## Why MediaMatic?

MediaMatic integrates **6 best-in-class .NET libraries** into a unified, fluent API:

| Library | Purpose | License |
|---------|---------|---------|
| [FluentStorage](https://github.com/robinrodricks/FluentStorage) | Storage abstraction (13+ providers) | MIT |
| [SkiaSharp](https://github.com/mono/SkiaSharp) | Image processing | MIT |
| [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore) | Video processing | MIT |
| [MimeDetective](https://github.com/MediatedCommunications/Mime-Detective) | Content-based MIME detection | MIT |
| [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) | EXIF/metadata extraction | Apache-2.0 |
| [DeviceDetector.NET](https://github.com/totpero/DeviceDetector.NET) | Browser/device detection | Apache-2.0 |

## Quick Example

```csharp
using FluentStorage;
using MJCZone.MediaMatic;

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

## Related Projects

MediaMatic is part of the **MJCZone *Matic** family:

- **[DapperMatic](https://github.com/mjczone/MJCZone.DapperMatic)** - Database schema management for Dapper

## License

Licensed under [LGPL-3.0-or-later](https://github.com/mjczone/mediamatic/blob/main/LICENSE)
