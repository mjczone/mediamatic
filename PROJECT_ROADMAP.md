# MediaMatic - Project Roadmap & Status

## Project Vision

MediaMatic is a unified media and file storage management library that combines:

1. **Multi-provider file storage** - CRUD operations across 14+ storage providers via FluentStorage
2. **Intelligent media processing** - Image optimization, video transcoding, thumbnail generation
3. **Metadata extraction** - Comprehensive EXIF, GPS, video/audio metadata
4. **Simple, consistent API** - Same interface regardless of storage backend

## Core Value Proposition

Instead of managing multiple libraries and dealing with provider-specific APIs, MediaMatic provides:

- One API for all storage providers (Local disk, S3, Azure, B2, Minio, SFTP, GCP, etc.)
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
- **Azure Blob Storage** - Microsoft Azure
- **Google Cloud Storage (GCP)** - Google Cloud Platform

#### ✅ Local/Network Storage
- **Local Disk** - File system storage
- **SFTP** - SSH File Transfer Protocol
- **Zip Archives** - Read/write to ZIP files

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
- ✅ `ExistsAsync` - Check if file exists
- ✅ `ListFilesAsync` - List files in folder
- ✅ `ListFoldersAsync` - List subfolders

**Media Operations:**
- 🔨 `UploadImageAsync` - Upload + process images
- 🔨 `ProcessImageAsync` - Process existing images
- 🔨 `UploadVideoAsync` - Upload + process videos
- 🔨 `GenerateThumbnailsAsync` - Generate video thumbnails
- 🔨 `TranscodeVideoAsync` - Transcode videos
- 🔨 `GetMetadataAsync` - Extract metadata

**Legend:** ✅ Implemented | 🔨 In Progress | ⏳ Planned

---

## Media Processing Capabilities

### Image Processing (via SkiaSharp 3.119.1)

**Supported Operations:**
- 🔨 Resize (width/height with aspect ratio preservation)
- 🔨 Format conversion (JPEG, PNG, WebP, AVIF, BMP, GIF)
- 🔨 Quality optimization
- 🔨 Variant generation (multiple sizes/formats)
- ⏳ Smart cropping with focal points
- ⏳ Watermarking
- ⏳ Filters and effects

**Supported Formats:**
- **Input:** JPEG, PNG, WebP, AVIF, BMP, GIF, ICO, WBMP
- **Output:** JPEG, PNG, WebP, AVIF, BMP, GIF

### Video Processing (via FFMpegCore 5.4.0)

**Supported Operations:**
- 🔨 Thumbnail generation (single/multiple frames)
- 🔨 Transcoding (format conversion)
- 🔨 Resolution/bitrate adjustment
- 🔨 Audio extraction
- ⏳ Clip extraction
- ⏳ Concatenation

**Supported Formats:**
- All formats supported by FFmpeg (MP4, MOV, AVI, MKV, WebM, etc.)

### Metadata Extraction

**Image Metadata (via MetadataExtractor 2.9.0):**
- 🔨 EXIF data (camera, lens, exposure settings)
- 🔨 GPS coordinates (latitude, longitude, altitude)
- 🔨 Timestamps (original, digitized)
- 🔨 Image dimensions and color space

**Video Metadata (via FFMpegCore 5.4.0):**
- 🔨 Duration, resolution, frame rate
- 🔨 Video/audio codecs
- 🔨 Bitrates (video/audio/total)
- 🔨 Container format

**Audio Metadata (via TagLibSharp 2.3.0):**
- ⏳ ID3 tags (artist, album, title, genre)
- ⏳ Track number, year, copyright
- ⏳ Album art extraction

### MIME Type Detection (via Mime-Detective 25.8.1)

- 🔨 Content-based MIME type detection
- 🔨 Magic number inspection
- 🔨 Works with any file format

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

### Phase 2: Core Processors 🔨 IN PROGRESS

#### MimeTypeDetector
- [ ] Implement `DetectMimeTypeAsync` using Mime-Detective
  - Note: API uses `ReadOnlySpan<byte>` instead of Stream
  - Need to read initial bytes from stream

#### MetadataReader
- [ ] Implement `ExtractImageMetadataAsync` using MetadataExtractor
  - Extract EXIF, GPS, camera settings
  - Parse timestamps
  - Handle orientation
- [ ] Implement `ExtractVideoMetadataAsync` using FFMpegCore
  - Get duration, resolution, codecs
  - Extract bitrate information
  - Read container metadata

#### ImageProcessor
- [ ] Implement `ResizeAsync` using SkiaSharp
  - Respect aspect ratio
  - Use modern SKSamplingOptions (not deprecated SKFilterQuality)
  - Support upscaling/downscaling
- [ ] Implement `ConvertFormatAsync` using SkiaSharp
  - Support all output formats (JPEG, PNG, WebP, AVIF)
  - Quality settings
  - Preserve metadata option
- [ ] Implement `GenerateVariantsAsync` using SkiaSharp
  - Generate multiple sizes/formats in one pass
  - Return list of processed images

#### VideoProcessor
- [ ] Implement `GenerateThumbnailsAsync` using FFMpegCore
  - Support time-based or count-based extraction
  - Return list of thumbnail paths
  - Quality settings
- [ ] Implement `TranscodeAsync` using FFMpegCore
  - Format conversion
  - Resolution/bitrate adjustment
  - Audio codec options

### Phase 3: VFS Method Orchestrations 🔨 IN PROGRESS

These methods coordinate the processors and storage operations:

- [ ] `UploadImageAsync` - Upload + detect + extract metadata + generate variants
  - Detect MIME type from stream
  - Upload original to VFS
  - Extract metadata
  - Generate variants (if requested)
  - Upload variants to VFS
  - Return result with all paths and metadata

- [ ] `ProcessImageAsync` - Process existing image from VFS
  - Download from VFS
  - Process (resize/convert)
  - Upload result to VFS
  - Return processing result

- [ ] `UploadVideoAsync` - Upload + detect + extract metadata + generate thumbnails
  - Detect MIME type from stream
  - Upload original to VFS (may need temp file for FFmpeg)
  - Extract metadata
  - Generate thumbnails (if requested)
  - Upload thumbnails to VFS
  - Return result with paths and metadata

- [ ] `GenerateThumbnailsAsync` - Generate thumbnails for existing video
  - Download video from VFS (or use path if Local provider)
  - Generate thumbnails using VideoProcessor
  - Upload thumbnails to VFS
  - Return thumbnail results

- [ ] `TranscodeVideoAsync` - Transcode existing video in VFS
  - Download source from VFS
  - Transcode using VideoProcessor
  - Upload result to VFS
  - Return processing result

- [ ] `GetMetadataAsync` - Extract metadata for existing media
  - Download from VFS
  - Detect MIME type
  - Call appropriate metadata extractor
  - Return metadata

### Phase 4: Testing 🔨 IN PROGRESS

- [ ] Unit tests for processors
  - MimeTypeDetector tests
  - ImageProcessor tests (resize, convert, variants)
  - VideoProcessor tests (thumbnails, transcode)
  - MetadataReader tests (image, video)

- [x] Test infrastructure
  - ✅ TestDataHelper for programmatic image generation (SkiaSharp)
  - ✅ SkiaSharp.NativeAssets.Linux.NoDependencies package added
  - ✅ Test fixtures for temporary directories

- [x] Integration tests for VFS methods - Image Processing
  - ✅ Local provider image tests (13 tests, all passing)
    - ✅ Basic image upload
    - ✅ WebP/AVIF variant generation (platform-tolerant)
    - ✅ Thumbnail generation at multiple sizes
    - ✅ Image resizing with MaxWidth/MaxHeight
    - ✅ PNG format handling
    - ✅ ProcessImageAsync (resize and format conversion)
    - ✅ Metadata extraction
    - ✅ Various image dimensions
    - ✅ Edge cases (thumbnails larger than original)

- [ ] Integration tests for VFS methods - Video Processing
  - [ ] UploadVideoAsync with thumbnails
  - [ ] Video processing operations
  - [ ] Metadata extraction for videos

- [ ] Provider-specific tests
  - [x] Local disk storage (image operations)
  - [ ] S3/Minio (using LocalStack)
  - [ ] Azure (using Azurite)
  - [ ] SFTP (using test servers)

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

- [ ] Smart image cropping with focal points
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

## Current Sprint: Phase 4 - Testing

### Current Focus: Integration Testing

**Completed:**
1. ✅ Test infrastructure setup (TestDataHelper, fixtures)
2. ✅ Local provider image tests (13 tests passing)
3. ✅ Platform compatibility fixes (AVIF, stream management)
4. ✅ Error handling improvements

**Next Steps:**
1. Local provider video tests (upload, thumbnails, metadata)
2. Memory provider tests (images and videos)
3. Unit tests for individual processors
4. S3/Minio provider tests (using LocalStack container)

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

**Phase 4 - Testing Goals:**
- ✅ Test infrastructure working (TestDataHelper, fixtures)
- ✅ Local provider image tests passing (13/13)
- [ ] Local provider video tests passing
- [ ] Memory provider tests passing
- [ ] S3/Minio provider tests passing (via containers)
- [ ] Unit tests for all processors

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
│  Local  │ │    S3    │ │  Azure   │ ...     Storage providers
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

- **LocalProviderImageTests**: 13/13 passing ✅
  - All image processing operations validated
  - Platform-tolerant (handles missing AVIF support gracefully)
  - Stream management working correctly
  - Default options tested (thumbnails + format variants)

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
- Local, S3, and Azure providers tested
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

### Phase 4 Success (Current)
- [x] Test infrastructure established
- [x] Local provider image tests (13/13 passing)
- [ ] Local provider video tests
- [ ] Memory provider tests
- [ ] S3/Minio provider tests (via containers)
- [ ] Unit tests for individual processors
- [ ] 80%+ code coverage

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

**Last Updated:** 2025-11-18
**Current Phase:** Phase 4 - Testing
**Next Milestone:** Complete Local provider video tests, expand to other providers
**Recent Achievements:**
- ✅ All core processors implemented (Phase 2 complete)
- ✅ Test infrastructure established
- ✅ 13/13 Local provider image tests passing
- ✅ Platform compatibility issues resolved
