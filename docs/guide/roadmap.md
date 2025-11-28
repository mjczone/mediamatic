# Roadmap

MediaMatic follows a phased development approach, building functionality incrementally.

## Current Status

**Version:** 0.1.x (Foundation)
**Tests:** 318 passing (66 ASP.NET Core + 252 Core)

## Development Phases

### Phase 1: Foundation ✅

- [x] Project structure and infrastructure
- [x] Directory.Build.props, .editorconfig
- [x] GitHub Actions CI/CD
- [x] Core interfaces and models

### Phase 2: Storage Providers ✅

- [x] FluentStorage integration
- [x] Virtual File System (VFS) abstraction
- [x] Local file system provider
- [x] In-memory provider (testing)
- [x] AWS S3 provider
- [x] MinIO provider
- [x] Google Cloud Storage provider
- [x] Backblaze B2 provider
- [x] SFTP provider
- [x] Zip file provider

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
- [x] Thumbnail generation

### Phase 4: Video Processing ✅

- [x] FFMpegCore integration
- [x] Thumbnail generation
  - [x] Multiple resize modes
  - [x] Focal point support
- [x] Video metadata extraction
- [x] Format conversion

### Phase 5: Metadata Extraction ✅

- [x] MimeDetective integration (content-based MIME detection)
- [x] MetadataExtractor integration (image EXIF)
- [x] Basic metadata extraction for all file types
- [x] Image dimensions, format, quality
- [x] Video duration, dimensions, codec info

### Phase 6: ASP.NET Core Integration ✅

- [x] Dependency injection extensions
- [x] Filesource repository pattern
- [x] REST API endpoints
  - [x] Filesource CRUD (`/api/mm/fs`)
  - [x] File operations (`/api/mm/fs/{id}/files/{path}`)
  - [x] Folder operations (`/api/mm/fs/{id}/folders/{path}`)
  - [x] Browse operations (`/api/mm/fs/{id}/browse/{path}`)
  - [x] Image transformations (`/api/mm/fs/{id}/transform/{params}/{path}`)
  - [x] Metadata extraction (`/api/mm/fs/{id}/metadata/{path}`)
  - [x] Archive operations (`/api/mm/fs/{id}/archive/...`)
- [x] Bucket support for multi-tenant storage
- [x] Caching headers (ETag, Cache-Control)
- [x] Browser-aware format serving (`f_auto` with DeviceDetector.NET)

### Phase 7: Archive Operations ✅

- [x] Create folder archives (recursive - all nested files included)
- [x] Create file list archives
- [x] Multiple compression formats: zip (default), tar, tar.gz
- [x] List archives
- [x] Download archives
- [x] Delete archives

### Phase 8: Testing ✅

- [x] Unit tests for processors (252 tests)
- [x] Integration tests with Testcontainers
- [x] ASP.NET Core endpoint tests (66 tests)
- [x] LocalStack for S3 testing
- [x] MinIO container for S3-compatible testing
- [x] SFTP container for SFTP testing
- [x] Test isolation with unique memory storage names

### Phase 9: Documentation ✅

- [x] VitePress site structure
- [x] Getting started guide
- [x] Storage providers guide
- [x] ASP.NET Core integration guide
- [x] API reference auto-generation
- [x] Code examples in guides

## v0.1.0 Release Ready ✅

The library is ready for v0.1.0 release with:

- All core VFS operations working
- Image transformation endpoints
- Metadata extraction endpoints
- Archive creation/management (zip, tar, tar.gz)
- Comprehensive test coverage (318 tests)
- Complete documentation

## Remaining Work (Post v0.1.0)

### v0.2.0 - Background Jobs & Statistics

- [ ] Background job system (Hangfire or similar) for async archive creation
- [ ] Add `background=true` parameter to archive endpoints for async creation
- [ ] Statistics endpoints (folder stats, filesource stats)
- [ ] Archive auto-deletion (DeleteAfter support)

### v0.3.0 - Enhanced Media Processing

- [ ] Video transcoding via FFMpegCore
- [ ] Thumbnail generation for videos
- [ ] AVIF image format support
- [ ] Responsive image set generation

### v1.0.0 - Production Ready

- [ ] Performance benchmarks
- [ ] Security audit
- [ ] API stabilization
- [ ] Additional code examples

## Future Considerations

### Batch Processing

A batch processing system for performing operations on multiple files:

- Bulk thumbnail generation
- Batch format conversion
- Parallel processing with progress tracking
- Background job support

### Potential Features

- AI-powered focal point detection
- Face detection for smart cropping
- CDN integration helpers
- Watermarking
- PDF thumbnail generation
- HEIC format support (Apple)

### Community Requests

We welcome feature requests! Please submit ideas via [GitHub Issues](https://github.com/mjczone/mediamatic/issues).

## Contributing

See the [GitHub repository](https://github.com/mjczone/mediamatic) for contribution guidelines.

## Related

- [TODO.md](https://github.com/mjczone/mediamatic/blob/main/TODO.md) - Detailed session notes and task tracking
