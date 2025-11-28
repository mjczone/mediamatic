# MediaMatic - Project Roadmap & Status

## Project Vision

MediaMatic is a unified media and file storage management library that combines:

1. **Multi-provider file storage** - CRUD operations across 14+ storage providers via FluentStorage
2. **Intelligent media processing** - Image optimization, video transcoding, thumbnail generation
3. **Metadata extraction** - Comprehensive EXIF, GPS, video/audio metadata
4. **Simple, consistent API** - Same interface regardless of storage backend

## Core Value Proposition

Instead of managing multiple libraries and dealing with provider-specific APIs, MediaMatic provides:

- One API for all storage providers (Local disk, S3, B2, Minio, SFTP, GCP, etc.)
- Built-in image processing (resize, convert, optimize, variants)
- Built-in video processing (thumbnails, transcoding)
- Automatic metadata extraction
- Modern format support (WebP, AVIF)
- Intelligent optimization based on content type

---

## Storage Provider Support

### Via FluentStorage (CRUD Operations)

MediaMatic leverages **FluentStorage 6.x** for multi-provider file operations:

#### ✅ Cloud Storage
- **AWS S3** - Amazon S3 and S3-compatible services
- **Backblaze B2** - S3-compatible API
- **Minio** - S3-compatible API
- **Google Cloud Storage (GCP)** - Google Cloud Platform

#### ✅ Local/Network Storage
- **Local Disk** - File system storage
- **SFTP** - SSH File Transfer Protocol
- **ZipFile Archives** - Read/write to ZIP files

#### ✅ Additional Providers
- **In-Memory** - Ephemeral storage for testing/caching
- **HTTP** - Read-only HTTP/HTTPS access

### Unified Operations

All providers support the same operations via `IVfsMethods`:

**Basic CRUD:**
- ✅ `UploadAsync` - Upload files from stream
- ✅ `DownloadAsync` - Download files to stream
- ✅ `DeleteAsync` - Delete single file
- ✅ `DeleteFolderAsync` - Delete folder recursively
- ✅ `CreateFolderAsync` - Create folder
- ✅ `ExistsAsync` - Check if file exists
- ✅ `ListAsync` - List files and folders with rich metadata
- ✅ `ListFilesAsync` - List files in folder
- ✅ `ListFoldersAsync` - List subfolders

**Media Operations:**
- ✅ `UploadImageAsync` - Upload + process images
- ✅ `ProcessImageAsync` - Process existing images
- ✅ `UploadVideoAsync` - Upload + process videos
- ✅ `GenerateThumbnailsAsync` - Generate video thumbnails
- ✅ `TranscodeVideoAsync` - Transcode videos
- ✅ `GetMetadataAsync` - Extract metadata
- ✅ `TransformImageAsync` - Transform image with URL-style parameters
- ✅ `TransformBatchAsync` - Batch transform multiple image variants

**REST API Endpoints (ASP.NET Core):**
- ✅ `GET/POST/PUT/DELETE /files/{*path}` - File operations
- ✅ `POST/DELETE /folders/{*path}` - Folder operations
- ✅ `GET /browse/{*path}` - Unified browsing with filters
- ✅ `GET /transform/{transformations}/{*path}` - On-the-fly image transformation
- ✅ `POST /transform/{transformations}/{*path}` - Transform and save (CMS pre-generation)
- ✅ `POST /transform-batch/` - Batch transform multiple variants
- ✅ `GET /stats/folders/{*path}` - Folder statistics
- ✅ `GET /archive/folders/{*path}` - Archive operations

**Legend:** ✅ Implemented | 🔨 In Progress | ⏳ Planned

---

## Media Processing Capabilities

### Image Processing (via SkiaSharp 3.119.1)

**Supported Operations:**
- ✅ Resize (width/height with aspect ratio preservation)
- ✅ Resize modes (Fit, Cover, Pad, Stretch)
- ✅ Smart cropping with focal points
- ✅ Format conversion (JPEG, PNG, WebP, AVIF, BMP, GIF)
- ✅ Quality optimization
- ✅ Variant generation (multiple sizes/formats)
- ⏳ Watermarking
- ⏳ Filters and effects

**Supported Formats:**
- **Input:** JPEG, PNG, WebP, AVIF, BMP, GIF, ICO, WBMP
- **Output:** JPEG, PNG, WebP, AVIF, BMP, GIF

### Video Processing (via FFMpegCore 5.4.0)

**Supported Operations:**
- ✅ Thumbnail generation (single/multiple frames)
- ✅ Thumbnail resize modes (Fit, Cover, Pad, Stretch)
- ✅ Thumbnail focal point cropping
- ✅ Transcoding (format conversion)
- ✅ Resolution/bitrate adjustment
- ✅ Audio extraction
- ⏳ Clip extraction
- ⏳ Concatenation

**Supported Formats:**
- All formats supported by FFmpeg (MP4, MOV, AVI, MKV, WebM, etc.)

### Metadata Extraction

**Image Metadata (via MetadataExtractor 2.9.0):**
- ✅ EXIF data (camera, lens, exposure settings)
- ✅ GPS coordinates (latitude, longitude, altitude)
- ✅ Timestamps (original, digitized)
- ✅ Image dimensions and color space

**Video Metadata (via FFMpegCore 5.4.0):**
- ✅ Duration, resolution, frame rate
- ✅ Video/audio codecs
- ✅ Bitrates (video/audio/total)
- ✅ Container format

**Audio Metadata (via TagLibSharp 2.3.0):**
- ⏳ ID3 tags (artist, album, title, genre)
- ⏳ Track number, year, copyright
- ⏳ Album art extraction

### MIME Type Detection (via Mime-Detective 25.8.1)

- ✅ Content-based MIME type detection
- ✅ Magic number inspection
- ✅ Works with any file format

---

## Implementation Status

### Phase 1: Foundation ✅

- [x] Project structure
- [x] FluentStorage integration
- [x] Core interfaces (`IVfsMethods`, `IVfsConnection`)
- [x] Base provider implementation (`VfsMethodsBase`)
- [x] CRUD operations (upload, download, delete, list)
- [x] Model definitions (all media models)
- [x] Processor interfaces

### Phase 2: Core Processors ✅ COMPLETE

#### MimeTypeDetector
- [x] Implement `DetectMimeTypeAsync` using Mime-Detective
  - Note: API uses `ReadOnlySpan<byte>` instead of Stream
  - Need to read initial bytes from stream

#### MetadataReader
- [x] Implement `ExtractImageMetadataAsync` using MetadataExtractor
  - Extract EXIF, GPS, camera settings
  - Parse timestamps
  - Handle orientation
- [x] Implement `ExtractVideoMetadataAsync` using FFMpegCore
  - Get duration, resolution, codecs
  - Extract bitrate information
  - Read container metadata

#### ImageProcessor
- [x] Implement `ResizeAsync` using SkiaSharp
  - Respect aspect ratio
  - Use modern SKSamplingOptions (not deprecated SKFilterQuality)
  - Support upscaling/downscaling
- [x] Implement `ConvertFormatAsync` using SkiaSharp
  - Support all output formats (JPEG, PNG, WebP, AVIF)
  - Quality settings
  - Preserve metadata option
- [x] Implement `GenerateVariantsAsync` using SkiaSharp
  - Generate multiple sizes/formats in one pass
  - Return list of processed images

#### VideoProcessor
- [x] Implement `GenerateThumbnailsAsync` using FFMpegCore
  - Support time-based or count-based extraction
  - Return list of thumbnail paths
  - Quality settings
- [x] Implement `TranscodeAsync` using FFMpegCore
  - Format conversion
  - Resolution/bitrate adjustment
  - Audio codec options

### Phase 3: VFS Method Orchestrations ✅ COMPLETE

These methods coordinate the processors and storage operations:

- [x] `UploadImageAsync` - Upload + detect + extract metadata + generate variants
  - Detect MIME type from stream
  - Upload original to VFS
  - Extract metadata
  - Generate variants (if requested)
  - Upload variants to VFS
  - Return result with all paths and metadata

- [x] `ProcessImageAsync` - Process existing image from VFS
  - Download from VFS
  - Process (resize/convert)
  - Upload result to VFS
  - Return processing result

- [x] `UploadVideoAsync` - Upload + detect + extract metadata + generate thumbnails
  - Detect MIME type from stream
  - Upload original to VFS (may need temp file for FFmpeg)
  - Extract metadata
  - Generate thumbnails (if requested)
  - Upload thumbnails to VFS
  - Return result with paths and metadata

- [x] `GenerateThumbnailsAsync` - Generate thumbnails for existing video
  - Download video from VFS (or use path if Local provider)
  - Generate thumbnails using VideoProcessor
  - Upload thumbnails to VFS
  - Return thumbnail results

- [x] `TranscodeVideoAsync` - Transcode existing video in VFS
  - Download source from VFS
  - Transcode using VideoProcessor
  - Upload result to VFS
  - Return processing result

- [x] `GetMetadataAsync` - Extract metadata for existing media
  - Download from VFS
  - Detect MIME type
  - Call appropriate metadata extractor
  - Return metadata

### Phase 4: Testing ✅ COMPLETE

- [x] Unit tests for processors (71 tests)
  - ✅ MimeTypeDetector tests (9 tests)
  - ✅ ImageProcessor tests - resize, convert (24 tests)
  - ✅ VideoProcessor tests - thumbnails, transcode (18 tests)
  - ✅ MetadataReader tests - image, video (20 tests)

- [x] Test infrastructure
  - ✅ TestDataHelper for programmatic image generation (SkiaSharp)
  - ✅ TestDataHelper for programmatic video generation (FFmpeg)
  - ✅ SkiaSharp.NativeAssets.Linux.NoDependencies package added
  - ✅ Test fixtures for temporary directories
  - ✅ SkipIfNoFfmpegFact/Theory attributes for graceful test skipping

- [x] Integration tests for VFS methods - Image Processing
  - ✅ Local provider image tests (13 tests)
    - ✅ Basic image upload
    - ✅ WebP/AVIF variant generation (platform-tolerant)
    - ✅ Thumbnail generation at multiple sizes
    - ✅ Image resizing with MaxWidth/MaxHeight
    - ✅ PNG format handling
    - ✅ ProcessImageAsync (resize and format conversion)
    - ✅ Metadata extraction
    - ✅ Various image dimensions
    - ✅ Edge cases (thumbnails larger than original)
  - ✅ Memory provider image tests (15 tests)

- [x] Integration tests for VFS methods - Video Processing
  - ✅ Local provider video tests (27 tests)
    - ✅ UploadVideoAsync with thumbnails
    - ✅ Video processing operations
    - ✅ Metadata extraction for videos
    - ✅ Transcoding (format, codec, bitrate, framerate)
    - ✅ Edge cases (no thumbnails, short videos)
  - ✅ Memory provider video tests (19 tests)

- [x] Provider-specific tests
  - ✅ Local disk storage (images and videos)
  - ✅ Memory storage (images and videos)
  - ⏳ S3/Minio (using LocalStack) - skipped (provider-agnostic architecture)
  - ⏳ SFTP (using test servers) - skipped (provider-agnostic architecture)

- [ ] Performance benchmarks
  - [ ] Large file uploads
  - [ ] Batch image processing
  - [ ] Video transcoding performance
  - [ ] Memory usage profiling

### Phase 5: Documentation ⏳ PLANNED

- [ ] API documentation (XML comments)
- [ ] README with examples
- [ ] Quick start guide
- [ ] Provider setup guides
- [ ] Image processing cookbook
- [ ] Video processing cookbook
- [ ] Migration guides
- [ ] Performance tuning guide

### Phase 6: Advanced Features ⏳ PLANNED

- [ ] Watermarking support
- [ ] Image filters and effects
- [ ] Video clip extraction
- [ ] Video concatenation
- [ ] Audio metadata extraction (TagLibSharp)
- [ ] Batch processing utilities
- [ ] Progress reporting for long operations
- [ ] Retry logic with exponential backoff
- [ ] Caching layer for metadata

---

## Current Sprint: Phase 5 - Documentation

### Current Focus: API Documentation & Guides

**Completed (Phase 4):**
1. ✅ Test infrastructure setup (TestDataHelper, fixtures, skip attributes)
2. ✅ Local provider image tests (13 tests)
3. ✅ Local provider video tests (27 tests)
4. ✅ Memory provider tests (34 tests - images and videos)
5. ✅ Processor unit tests (71 tests)
6. ✅ 204 total tests passing
7. ✅ Platform compatibility fixes (AVIF, stream management)
8. ✅ Provider-agnostic architecture validated

**Next Steps:**
1. API documentation (XML comments for all public members)
2. README with quick start examples
3. Provider setup guides (Local, S3, Minio, B2, SFTP, ZipFile, GCP)
4. Image processing cookbook
5. Video processing cookbook

### Previous Sprints

**Phase 2: Core Processors** ✅ COMPLETE

**Week 1-2: Basic Image Processing**
1. ✅ MimeTypeDetector
2. ✅ ImageProcessor.ResizeAsync
3. ✅ ImageProcessor.ConvertFormatAsync
4. ✅ ImageProcessor.GenerateVariantsAsync
5. ✅ MetadataReader.ExtractImageMetadataAsync
6. ✅ VfsMethodsBase.UploadImageAsync
7. ✅ VfsMethodsBase.ProcessImageAsync

**Week 3-4: Video Processing**
8. ✅ VideoProcessor.GenerateThumbnailsAsync
9. ✅ VideoProcessor.TranscodeAsync
10. ✅ MetadataReader.ExtractVideoMetadataAsync
11. ✅ VfsMethodsBase.UploadVideoAsync
12. ✅ VfsMethodsBase.GenerateThumbnailsAsync
13. ✅ VfsMethodsBase.TranscodeVideoAsync
14. ✅ VfsMethodsBase.GetMetadataAsync

### Success Criteria

**Phase 4 - Testing Goals:** ✅ ALL COMPLETE
- ✅ Test infrastructure working (TestDataHelper, fixtures, skip attributes)
- ✅ Local provider image tests passing (13/13)
- ✅ Local provider video tests passing (27/27)
- ✅ Memory provider image tests passing (15/15)
- ✅ Memory provider video tests passing (19/19)
- ✅ Unit tests for all processors (71/71)
- ✅ Total: 204 tests passing

---

## Technical Architecture

### Layer Structure

```
┌─────────────────────────────────────────────┐
│         User Application                    │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│      VfsConnectionExtensions                │  Extension methods
│      (Convenience API)                      │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│         IVfsMethods                         │  Core interface
│   (UploadImageAsync, ProcessImageAsync...)  │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│      VfsMethodsBase                         │  Base orchestration
│   (Coordinates processors + storage)        │
└────┬────────────┬────────────┬──────────────┘
     │            │            │
     ▼            ▼            ▼
┌─────────┐ ┌──────────┐ ┌──────────────┐
│  MIME   │ │ Metadata │ │   Image/     │      Processors
│Detector │ │  Reader  │ │   Video      │
│         │ │          │ │  Processor   │
└─────────┘ └──────────┘ └──────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│       FluentStorage (IBlobStorage)          │  Storage abstraction
└────┬────────────┬────────────┬──────────────┘
     │            │            │
     ▼            ▼            ▼
┌─────────┐ ┌──────────┐ ┌──────────┐
│  Local  │ │    S3    │ │   GCP    │ ...     Storage providers
│  Disk   │ │  Minio   │ │   SFTP   │
│         │ │    B2    │ │          │
└─────────┘ └──────────┘ └──────────┘
```

### Key Design Decisions

1. **Stream-based processing** - Efficient memory usage for large files
2. **Provider-agnostic** - Same API for all storage backends
3. **Processor isolation** - Each processor is independent and testable
4. **Orchestration in base class** - VfsMethodsBase coordinates everything
5. **Async/await throughout** - Modern async patterns
6. **CancellationToken support** - Proper cancellation handling
7. **Minimal dependencies** - Only essential libraries

### Library Dependencies

```
MJCZone.MediaMatic
├── FluentStorage 6.0.0 (file storage abstraction)
│   ├── FluentStorage.AWS (S3, Minio, B2)
│   ├── FluentStorage.SFTP (SFTP)
│   └── FluentStorage.GCP (Google Cloud Storage)
├── SkiaSharp 3.119.1 (image processing)
├── FFMpegCore 5.4.0 (video processing - requires FFmpeg binary)
├── MetadataExtractor 2.9.0 (EXIF/image metadata)
├── TagLibSharp 2.3.0 (audio metadata)
└── Mime-Detective 25.8.1 (MIME type detection)
```

**Note:** FFMpegCore requires FFmpeg to be installed on the system or provided as a binary.

---

## Testing Notes & Lessons Learned

### Platform Compatibility Issues (Fixed)

1. **AVIF Encoding on Linux**
   - **Issue**: SkiaSharp's `image.Encode()` returns `null` for AVIF on Linux (missing native libs)
   - **Fix**: Added null check in `ImageProcessor.EncodeImage()` with meaningful error
   - **Fix**: Wrapped format variant generation in try-catch to gracefully skip unsupported formats
   - **Result**: Image uploads succeed with supported formats (WebP), skip unsupported (AVIF)

2. **Stream Disposal by FluentStorage**
   - **Issue**: FluentStorage's `WriteAsync()` disposes streams, breaking variant generation
   - **Fix**: Create separate MemoryStream copies for each upload operation
   - **Fix**: Keep original stream for generating multiple variants and thumbnails
   - **Pattern**: Upload copy, generate from original, dispose when done

3. **Test Infrastructure**
   - **Created**: `TestDataHelper` for programmatic image generation using SkiaSharp
   - **Added**: `SkiaSharp.NativeAssets.Linux.NoDependencies` package for Linux support
   - **Pattern**: Generate test images in-memory rather than using physical files

### Test Coverage Summary

**Total: 234 tests passing** ✅

- **LocalProviderImageTests**: 13/13 passing ✅
  - All image processing operations validated
  - Platform-tolerant (handles missing AVIF support gracefully)
  - Stream management working correctly
  - Default options tested (thumbnails + format variants)

- **LocalProviderVideoTests**: 27/27 passing ✅
  - Video upload with metadata extraction
  - Thumbnail generation (single/multiple)
  - Transcoding (format, codec, resolution, bitrate, framerate)
  - Audio stripping and CRF quality
  - Uses SkipIfNoFfmpegFact for graceful skipping when FFmpeg unavailable

- **MemoryProviderImageTests**: 15/15 passing ✅
  - Same coverage as Local provider
  - Validates in-memory storage operations
  - Download verification tests

- **MemoryProviderVideoTests**: 19/19 passing ✅
  - Same coverage as Local provider
  - Validates in-memory storage for videos

- **ProcessorTests**: 101/101 passing ✅
  - MimeTypeDetector: 9 tests (JPEG, PNG, WebP detection, stream handling)
  - ImageProcessor: 38 tests (resize, format conversion, resize modes, focal points)
  - VideoProcessor: 32 tests (thumbnails, transcoding, resize modes, focal points)
  - MetadataReader: 20 tests (image dimensions, video duration/codec/bitrate)

- **Other Tests**: 59 passing ✅
  - Connection tests, model tests, etc.

---

## Known Limitations & Future Considerations

### Current Limitations

1. **FFmpeg Dependency** - VideoProcessor requires FFmpeg binary installed
2. **Temp Files for Video** - Video processing may require temp files (cloud providers)
3. **No Progress Reporting** - Long operations don't report progress yet
4. **No Retry Logic** - Network failures aren't automatically retried
5. **No Caching** - Metadata not cached, always re-extracted
6. **Synchronous Processing** - Video operations are sequential, not parallel
7. **Platform-Specific Format Support** - AVIF encoding may not work on all platforms (Linux requires additional native libraries)

### Future Enhancements

See [LARGE_FILE_UPLOADS.md](./LARGE_FILE_UPLOADS.md) for detailed plan on:
- Resumable uploads
- Chunked/multipart uploads
- Progress tracking
- Upload validation

Additional future work:
- Background job processing for long operations
- Webhook notifications for async operations
- CDN integration
- Image optimization profiles
- Video quality presets
- Batch processing utilities
- Admin dashboard for monitoring

---

## Contributing

### Development Setup

1. Clone repository
2. Install .NET 9 SDK (or compatible version)
3. Install FFmpeg (for video processing)
4. Restore packages: `dotnet restore`
5. Build: `dotnet build`
6. Run tests: `dotnet test`

### Code Standards

- Follow existing code style
- Add XML documentation comments
- Include unit tests for new features
- Update this roadmap when adding features
- Keep dependencies minimal

### Pull Request Process

1. Create feature branch from `develop`
2. Implement feature with tests
3. Update documentation
4. Submit PR to `develop` branch
5. Ensure CI passes
6. Request review

---

## Release Strategy

### Version 0.1.0 (Current - In Development)

**Target:** Q1 2025

**Scope:**
- All core processors implemented
- Basic image and video processing working
- Local, S3, and other providers tested
- Minimal documentation

**Not Included:**
- Advanced features (watermarking, filters, etc.)
- Resumable uploads
- Progress reporting
- Comprehensive test coverage

### Version 0.2.0 (Planned)

**Target:** Q2 2025

**Scope:**
- Comprehensive test coverage
- Additional providers tested (SFTP, GCP)
- Performance optimizations
- Better error handling and retry logic
- Complete API documentation

### Version 1.0.0 (Planned)

**Target:** Q3 2025

**Scope:**
- Production-ready
- Full documentation
- Performance benchmarks
- Migration guides
- Stable API (semantic versioning)

---

## Success Metrics

### Phase 2 Success ✅ COMPLETE
- [x] All NotImplementedException instances resolved
- [x] Can upload and process images (resize, convert, variants)
- [x] Can upload videos and generate thumbnails
- [x] Can extract metadata from images and videos
- [x] Basic integration tests pass with Local provider

### Phase 4 Success ✅ COMPLETE
- [x] Test infrastructure established
- [x] Local provider image tests (13/13 passing)
- [x] Local provider video tests (27/27 passing)
- [x] Memory provider image tests (15/15 passing)
- [x] Memory provider video tests (19/19 passing)
- [x] Unit tests for all processors (101/101 passing)
- [x] Resize modes (Fit, Cover, Pad, Stretch) implemented and tested
- [x] Focal point cropping implemented and tested
- [x] 234 total tests passing
- [ ] 80%+ code coverage (to be measured)

### Project Success (v1.0)
- Supports 8+ storage providers
- Processes 10+ image formats
- Processes 20+ video formats
- <100ms for metadata extraction
- <1s for image resize/convert (1920x1080)
- <5s for video thumbnail generation
- Comprehensive documentation
- 80%+ test coverage
- Used in production by 5+ projects

---

## Questions & Decisions Log

### Resolved
- ✅ Use FluentStorage for multi-provider abstraction
- ✅ Use SkiaSharp for image processing (vs ImageSharp)
- ✅ Use FFMpegCore for video processing
- ✅ Custom chunked upload implementation (vs TUS protocol)

### Pending
- ❓ How to handle FFmpeg binary distribution?
- ❓ Should we provide pre-built Docker images with FFmpeg?
- ❓ How to handle very large video files (>1GB)?
- ❓ Should we add a queueing system for async operations?
- ❓ Do we need a database for tracking operations?

---

## Resources

- [FluentStorage Documentation](https://github.com/robinrodricks/FluentStorage)
- [SkiaSharp Documentation](https://learn.microsoft.com/en-us/dotnet/api/skiasharp)
- [FFMpegCore Documentation](https://github.com/rosenbjerg/FFMpegCore)
- [MetadataExtractor Documentation](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [Large File Uploads Research](./LARGE_FILE_UPLOADS.md)

---

**Last Updated:** 2025-11-27
**Current Phase:** Phase 5 - Documentation
**Next Milestone:** Complete API documentation and user guides
**Recent Achievements:**
- ✅ Phase 2 complete - All core processors implemented
- ✅ Phase 3 complete - All VFS method orchestrations implemented
- ✅ Phase 4 complete - Comprehensive test coverage (234 tests)
- ✅ Resize modes implemented (Fit, Cover, Pad, Stretch) for images and video thumbnails
- ✅ Focal point cropping implemented for smart image/thumbnail cropping
- ✅ Test infrastructure with FFmpeg skip attributes for graceful degradation
- ✅ Provider-agnostic architecture validated (same code for all providers)
- ✅ Platform compatibility issues resolved (AVIF, stream management)
- ✅ **REST API Restructuring Complete:**
  - Renamed `/fi/` to `/files/` for file operations
  - Renamed `/fo/` to `/folders/` for folder operations
  - Added unified `/browse/` endpoint with type/filter/recursive/fields params
  - Added POST transform endpoints for CMS pre-generation workflows
  - Added batch transform endpoint for generating multiple variants
  - Added `?download=true` and `?saveTo=` query parameters for transforms
  - Added `CreateFolderAsync` method to core VFS interface
  - Added rich `ListAsync` method returning file/folder metadata with categories
