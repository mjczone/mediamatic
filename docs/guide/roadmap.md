# Roadmap

MediaMatic follows a phased development approach, building functionality incrementally.

## Current Status

**Version:** 0.1.x (Foundation)

## Development Phases

### Phase 1: Foundation ✅

- [x] Project structure and infrastructure
- [x] Directory.Build.props, .editorconfig
- [x] GitHub Actions CI/CD
- [x] Core interfaces and models

### Phase 2: Storage Providers ✅

- [x] FluentStorage integration
- [x] Local file system provider
- [x] In-memory provider (testing)
- [x] AWS S3 provider
- [x] Google Cloud Storage provider

### Phase 3: Image Processing ✅

- [x] SkiaSharp integration
- [x] Image resizing with multiple modes
  - [x] Fit mode
  - [x] Cover mode
  - [x] Pad mode
  - [x] Stretch mode
- [x] Focal point cropping
- [x] Format conversion (JPEG, PNG, WebP)
- [x] Quality optimization

### Phase 4: Video Processing ✅

- [x] FFMpegCore integration
- [x] Thumbnail generation
  - [x] Multiple resize modes
  - [x] Focal point support
- [x] Video transcoding
- [x] Format conversion

### Phase 5: Metadata Extraction 🚧

- [x] MimeDetective integration
- [x] Basic metadata extraction
- [ ] MetadataExtractor integration (EXIF)
- [ ] TagLibSharp integration (audio/video tags)

### Phase 6: ASP.NET Core Integration 📋

- [ ] Dependency injection extensions
- [ ] DeviceDetector.NET integration
- [ ] Format negotiation middleware
- [ ] Minimal API endpoints
- [ ] Browser-aware image serving

### Phase 7: Documentation 🚧

- [x] VitePress site structure
- [x] Getting started guide
- [x] Core guides
- [ ] API reference auto-generation
- [ ] Code examples

### Phase 8: Testing & Polish 📋

- [x] Unit tests for processors
- [x] Integration tests with Testcontainers
- [ ] Increase test coverage to 90%+
- [ ] Performance benchmarks
- [ ] API stabilization

## Version 1.0 Goals

- Complete ASP.NET Core integration
- Comprehensive documentation
- 90%+ test coverage
- Stable public API
- Performance benchmarks published

## Future Considerations

### Batch Processing Utility (Planned)

A comprehensive batch processing system for performing operations on multiple files:

**Thumbnail Generation**
- Automatically generate thumbnails for all images in a folder
- Generate video thumbnails for all videos
- Support for recursive folder processing
- Configurable output paths and naming conventions

**Transformation Application**
- Apply transformations to all images in a folder
- Example: Bulk resize, format conversion, quality optimization
- Support for filtering by file extension or MIME type
- Parallel processing for improved performance

**Format Conversion**
- Bulk convert images between formats (JPEG → WebP, PNG → AVIF)
- Batch video transcoding
- PDF conversion and thumbnail generation
- Preserve or strip metadata options

**Implementation Details**
```
POST /fs/{filesourceId}/batch/thumbnails/{*folderPath}
POST /fs/{filesourceId}/batch/transform/{*folderPath}
POST /fs/{filesourceId}/batch/convert/{*folderPath}
GET  /fs/{filesourceId}/jobs/{jobId}
```

**Request Example**:
```json
{
  "transformation": "w_800,h_600,c_fill,q_80,f_webp",
  "filter": {
    "extensions": [".jpg", ".png"],
    "recursive": true
  },
  "outputPath": "optimized/",
  "parallel": 4
}
```

**Features**:
- Background job processing with status tracking
- Progress reporting (files processed / total files)
- Error handling and retry logic
- Configurable parallelism
- Filtering by file type, size, or date

### Potential Features

- AVIF format support
- HEIC format support (Apple)
- AI-powered focal point detection
- Face detection for smart cropping
- CDN integration helpers
- Watermarking
- PDF thumbnail generation

### Community Requests

We welcome feature requests! Please submit ideas via [GitHub Issues](https://github.com/mjczone/mediamatic/issues).

## Contributing

See the [GitHub repository](https://github.com/mjczone/mediamatic) for contribution guidelines.

## Related

- [PROJECT_ROADMAP.md](https://github.com/mjczone/mediamatic/blob/main/PROJECT_ROADMAP.md) - Detailed technical roadmap
