# Installation

This guide covers the complete installation process for MediaMatic.

## Prerequisites

### .NET SDK

MediaMatic requires **.NET 8.0** or later.

```bash
# Check your .NET version
dotnet --version
```

### FFmpeg (Optional)

FFmpeg is required only for video processing features. Skip this if you only need image processing.

::: tip
If you don't install FFmpeg, all image processing features will work normally. Video processing methods will throw an exception if FFmpeg is not available.
:::

#### Ubuntu/Debian
```bash
sudo apt-get update
sudo apt-get install -y ffmpeg
```

#### macOS (Homebrew)
```bash
brew install ffmpeg
```

#### Windows

**Chocolatey:**
```bash
choco install ffmpeg
```

**Scoop:**
```bash
scoop install ffmpeg
```

**Manual Installation:**
1. Download from [ffmpeg.org](https://ffmpeg.org/download.html)
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add to PATH: `C:\ffmpeg\bin`

#### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*
```

#### Verify Installation
```bash
ffmpeg -version
ffprobe -version
```

## NuGet Packages

### Core Library

The core library provides all media processing functionality:

```bash
dotnet add package MJCZone.MediaMatic
```

Or via Package Manager:
```powershell
Install-Package MJCZone.MediaMatic
```

### ASP.NET Core Integration

For web applications, add the ASP.NET Core integration package:

```bash
dotnet add package MJCZone.MediaMatic.AspNetCore
```

This package includes:
- Dependency injection extensions
- Browser detection middleware
- Minimal API endpoints
- Format negotiation

### Storage Provider Packages

MediaMatic uses FluentStorage for storage abstraction. Install the providers you need:

```bash
# AWS S3 (also covers MinIO, B2, DigitalOcean Spaces)
dotnet add package FluentStorage.AWS

# Google Cloud Storage
dotnet add package FluentStorage.GCP

# SFTP
dotnet add package FluentStorage.SFTP
```

## Package References

For SDK-style projects, add to your `.csproj`:

```xml
<ItemGroup>
  <!-- Core library -->
  <PackageReference Include="MJCZone.MediaMatic" Version="0.1.*" />

  <!-- ASP.NET Core integration (optional) -->
  <PackageReference Include="MJCZone.MediaMatic.AspNetCore" Version="0.1.*" />

  <!-- Storage providers -->
  <PackageReference Include="FluentStorage.AWS" Version="5.6.*" />
</ItemGroup>
```

## Platform Support

### Operating Systems

| OS | Image Processing | Video Processing |
|----|------------------|------------------|
| Windows x64 | ✅ | ✅ |
| Linux x64 | ✅ | ✅ |
| macOS x64 | ✅ | ✅ |
| macOS ARM64 | ✅ | ✅ |
| Linux ARM64 | ✅ | ✅ |

### SkiaSharp Native Dependencies

SkiaSharp requires native libraries that are automatically included via NuGet. If you encounter issues:

**Linux:**
```bash
# Install dependencies
sudo apt-get install -y libfontconfig1 libfreetype6
```

**Alpine Linux:**
```dockerfile
RUN apk add --no-cache fontconfig freetype
```

## Verifying Installation

Create a simple test to verify everything is working:

```csharp
using MJCZone.MediaMatic.Processors;

// Test image processing
var imageProcessor = new ImageProcessor();
Console.WriteLine("Image processor initialized successfully");

// Test video processing (requires FFmpeg)
try
{
    var videoProcessor = new VideoProcessor();
    Console.WriteLine("Video processor initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Video processor requires FFmpeg: {ex.Message}");
}
```

## Troubleshooting

### SkiaSharp Initialization Errors

If you see errors about `libSkiaSharp`:

1. Ensure you have the correct runtime package for your platform
2. On Linux, install `libfontconfig1` and `libfreetype6`
3. On Docker, use a base image with these libraries

### FFmpeg Not Found

If video processing fails with "FFmpeg not found":

1. Verify FFmpeg is installed: `ffmpeg -version`
2. Ensure it's in your PATH
3. Restart your application after installing
4. On Windows, you may need to restart your terminal

### Memory Issues

For large media files, ensure adequate memory:

```csharp
// Use streaming to avoid loading entire files into memory
using var stream = File.OpenRead(path);
var result = await processor.ProcessAsync(stream, options);
```

## Next Steps

- [Getting Started](getting-started.md) - Quick start guide
- [Configuration](configuration.md) - Configure MediaMatic options
- [Storage Providers](storage-providers.md) - Set up storage backends
