---
layout: home

hero:
  name: "MediaMatic"
  text: "Intelligent Media Storage & Optimization"
  tagline: C# library and REST API for media storage, optimization, and processing across multiple storage providers
  announcement:
    title: "⚠️ Under Development"
    content: "This library is in active development (v0.x.x). Breaking changes may occur. Not recommended for production use until v1.0.0."
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/mjczone/mediamatic
    - theme: none
      text: Version VERSION_NUMBER

features:
  - icon: 🖼️
    title: Image Processing
    details: Resize, convert, and optimize images with SkiaSharp. Support for JPEG, PNG, WebP, AVIF with multiple resize modes and focal point cropping.
  - icon: 🎬
    title: Video Processing
    details: Generate thumbnails, transcode videos, and extract metadata using FFMpegCore. Support for multiple formats and resolutions.
  - icon: ☁️
    title: Multi-Provider Storage
    details: Virtual File System abstraction with 8+ storage providers including AWS S3, Google Cloud, MinIO, Backblaze B2, SFTP, and local file system.
  - icon: 🔍
    title: Intelligent Metadata
    details: Auto-detect MIME types with MimeDetective, extract EXIF data with MetadataExtractor, and read audio/video tags with TagLibSharp.
  - icon: 🧪
    title: Thoroughly Tested
    details: Comprehensive test suite with 300+ tests ensures reliability across all supported providers using Testcontainers.
  - icon: 📦
    title: NuGet Package
    details: Easy installation via NuGet with minimal dependencies - just add to your .NET project.
---

## Choose Your Development Path

MediaMatic offers two powerful ways to manage your media storage. Choose the approach that best fits your project:

<div class="vp-feature-grid">
  <div class="vp-feature-item">
    <div class="vp-feature-icon">🛠️</div>
    <h3>.NET Library Development</h3>
    <p>Use MediaMatic directly in your .NET applications for media storage and processing</p>
    <ul>
      <li><strong>Best for:</strong> Console apps, desktop apps, microservices, custom tooling</li>
      <li><strong>Package:</strong> MJCZone.MediaMatic</li>
      <li><strong>Usage:</strong> Direct IVfsConnection extensions</li>
    </ul>
    <div class="vp-feature-actions">
      <a href="/guide/getting-started" class="vp-button vp-button-brand">Library Quick Start</a>
    </div>
  </div>

  <div class="vp-feature-item">
    <div class="vp-feature-icon">🌐</div>
    <h3>Web API Integration</h3>
    <p>Add REST endpoints to your ASP.NET Core applications for media management</p>
    <ul>
      <li><strong>Best for:</strong> Web applications, admin panels, media management tools</li>
      <li><strong>Package:</strong> MJCZone.MediaMatic.AspNetCore</li>
      <li><strong>Usage:</strong> HTTP REST API endpoints</li>
    </ul>
    <div class="vp-feature-actions">
      <a href="/guide/aspnetcore-integration" class="vp-button vp-button-alt">Web API Quick Start</a>
    </div>
  </div>
</div>

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

## Related Projects

MediaMatic is part of the **MJCZone *Matic** family:

- **[DapperMatic](https://github.com/mjczone/dappermatic)** - Database schema management for Dapper

<div style="text-align: center; margin-top: 2rem; padding-top: 2rem; border-top: 1px solid var(--vp-c-divider); color: var(--vp-c-text-2); font-size: 0.9em;">
  <p>Made with ❤️ by <a href="https://www.mjczone.com" target="_blank">MJCZone Inc.</a></p>
  <p>Released under the <a href="/guide/license">LGPL v3 License</a></p>
</div>
