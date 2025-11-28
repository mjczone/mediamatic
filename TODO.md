# MediaMatic - TODO

**Last Updated:** 2025-11-27

## 📊 Current Test Status

| Test Suite | Tests | Status |
|------------|-------|--------|
| ASP.NET Core Integration | 66 | ✅ All passing |
| Core Library | 252 | ✅ All passing |
| **Total** | **318** | ✅ All passing |

---

## 🎯 v0.1.0 Release Status

**Ready for Release!** All core functionality is implemented and tested.

### ✅ Included in v0.1.0

#### Core VFS Operations
- ✅ File operations (upload, download, delete, exists, overwrite)
- ✅ Folder operations (list, delete, nested folders)
- ✅ Browse with recursive listing support (via `?recursive=true`)
- ✅ Provider support: Memory, Local, S3, Minio, SFTP, FTP, Zip

#### ASP.NET Core Endpoints
- ✅ FilesourceEndpoints - CRUD for filesource management
- ✅ FileEndpoints - File upload/download/delete/exists
- ✅ FolderEndpoints - Folder listing and deletion
- ✅ BrowseEndpoints - Rich file/folder listing with filtering
- ✅ TransformationEndpoints - Image resize, crop, format conversion
- ✅ MetadataEndpoints - File metadata extraction (MIME, dimensions, etc.)
- ✅ ArchiveEndpoints - Create, list, download, delete archives

#### Image Processing (via SkiaSharp)
- ✅ Resize (width, height, or both)
- ✅ Crop modes (fit, cover, pad, stretch)
- ✅ Format conversion (JPEG, PNG, WebP)
- ✅ Quality settings
- ✅ Focal point cropping

#### Archive Operations
- ✅ Create archive from folder (recursive - includes all nested files)
- ✅ Create archive from file list
- ✅ Multiple formats: zip (default), tar, tar.gz
- ✅ List archives
- ✅ Download archives
- ✅ Delete archives
- Note: v0.1.0 uses synchronous archive creation (no background jobs)

#### Metadata Extraction
- ✅ Image metadata (dimensions, format, EXIF)
- ✅ Video metadata (duration, dimensions, codec) - requires FFmpeg
- ✅ Audio metadata (duration, format)
- ✅ Non-media file support (size, MIME type)

### ❌ Not Included in v0.1.0

- Statistics endpoints (GetFolderStats, GetFilesourceStats) - deferred to future release
- Background job system for async archive creation - deferred to future release
- Video transcoding - deferred to future release
- Auto-delete archives after duration (DeleteAfter) - requires background jobs

---

## ✅ Completed (Recent Sessions)

### Archive Implementation (v0.1.0)
- ✅ Implemented synchronous archive operations in `MediaMaticService.Archives.cs`
  - CreateFolderArchiveAsync - archive entire folder (recursive)
  - CreateFileListArchiveAsync - archive specific files/folders
  - ListArchivesAsync - list archives in `__archives` folder
  - DownloadArchiveAsync - stream archive content
  - DeleteArchiveAsync - remove archive
- ✅ Multiple compression formats: zip, tar, tar.gz (native .NET 7+ support)
- ✅ Removed job status endpoint (not needed for synchronous operations)
- ✅ Simplified ArchiveRequest model (removed unused Recursive and DeleteAfter properties)
- ✅ Simplified ArchiveResponse model (removed JobId and Status)

### Statistics Removal
- ✅ Removed `StatsEndpoints.cs`
- ✅ Removed `FolderStatsResponse.cs` and `FilesourceStatsResponse.cs`
- ✅ Removed `MediaMaticService.Stats.cs`
- ✅ Removed stats methods from `IMediaMaticService.cs`

### SFTP Fix
- ✅ Fixed flaky `DeleteFolderAsync` test
  - Issue: SFTP servers auto-delete empty parent folders
  - Fix: Wrapped folder deletion in try-catch to ignore "not found" errors

### Metadata Endpoint Tests
- ✅ MetadataEndpointsTests: 8/8 tests passing
  - JPEG/PNG image metadata extraction (dimensions, MIME type)
  - Nested path and bucket support
  - Error handling (non-existent file/filesource)
  - Non-media file support (text files return basic metadata)
  - Provider identification in metadata response
- ✅ Fixed VfsMethodsBase.GetMetadataAsync
  - Stream position reset after MIME detection
  - MimeType now included in image/video metadata response
  - File size included for non-media files

### Image Transformation
- ✅ Implemented `MediaMaticService.Files.TransformImageAsync`
  - Wired up to existing `ImageProcessor` (SkiaSharp)
  - Supports resize, crop modes (fit, cover, pad, stretch), format conversion
  - Quality settings, focal point cropping
- ✅ TransformationEndpoints: 13/13 tests passing
  - Basic resize (width, height, both dimensions)
  - Format conversion (WebP, PNG)
  - Resize modes (cover/fill, pad)
  - Combined transformations
  - Error handling, caching headers
- ✅ Registered `IImageProcessor` in DI container
- ✅ Fixed MemoryProviderImageTests test isolation (unique file names per test)

### Core VFS Provider Tests
- ✅ Created VfsProviderTestsBase with 13 tests covering:
  - Basic file operations (upload, download, delete, exists)
  - File listing (root, nested paths)
  - Folder operations (list, delete, nested folders)
- ✅ Memory provider: 13 base tests passing
- ✅ Local provider: 16 tests passing (13 base + 3 provider-specific)
- ✅ S3 provider: 14 tests passing (13 base + 1 AWS SDK verification) - via LocalStack
- ✅ Minio provider: 15 tests passing (13 base + 2 provider-specific) - via Testcontainers
- ✅ SFTP provider: 11 tests passing - via Testcontainers

### Critical Bug Fixes
- ✅ Fixed Memory provider isolation issue (shared static storage by connection string)
- ✅ Fixed GetMetadataAsync to support non-media files (was throwing "Unsupported file type")
- ✅ Fixed FileEndpoints synchronous I/O issue (request.Form → request.ReadFormAsync)
- ✅ Fixed folder listing to return 404 when folder doesn't exist (was returning 200 with empty array)
- ✅ Fixed folder deletion to return 404 when folder doesn't exist
- ✅ Fixed SFTP DeleteFolderAsync to handle auto-deleted empty folders

### ASP.NET Core Integration Tests
- ✅ FilesourceEndpoints: 8/8 tests passing
  - Complete CRUD workflow, filtering, search
  - Auto-generated IDs, provider-specific tests
  - Connectivity testing, error scenarios
- ✅ FileEndpoints: 11/11 tests passing
  - Upload/download (root, nested, buckets)
  - Delete, exists (HEAD), list
  - Overwrite with PUT
  - Error scenarios
- ✅ FolderEndpoints: 11/11 tests passing
  - List folders/files (root, nested, buckets)
  - Delete folders (nested, buckets)
  - Error scenarios
- ✅ BrowseEndpointsTests: 15/15 tests passing
  - Browse with recursive support

---

## 🔮 Post-v0.1.0 Roadmap

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
- [ ] Complete API documentation
- [ ] Performance benchmarks
- [ ] Security audit
- [ ] API stabilization

---

## 📝 Technical Notes

### Recursive Listing
- **Browse endpoints** (`/api/mm/fs/{id}/browse/`) support `?recursive=true` via FluentStorage's `ListOptions.Recurse`
- **Folder archives** are always recursive - all nested files are included
- **ListFilesAsync service method** has recursive parameter but not yet wired to VFS layer - deferred for post-v0.1.0

### Archive Storage
- Archives stored in `__archives` folder within each filesource/bucket
- Supported formats:
  - `zip` (default) - System.IO.Compression.ZipArchive
  - `tar` - System.Formats.Tar.TarWriter (native .NET 7+)
  - `tar.gz` - TarWriter with GZipStream compression
- Synchronous creation in v0.1.0 (no background jobs)
- ArchiveRequest properties:
  - `Name` - custom archive name (optional, defaults to timestamp)
  - `Paths` - list of files/folders for file list archives
  - `Compression` - format: "zip", "tar", or "tar.gz"

### Test Infrastructure
- ✅ WebApplicationFactory pattern for integration tests
- ✅ In-memory repository for test isolation
- ✅ Base test classes for consistent provider coverage
- ✅ Testcontainers for S3/Minio/SFTP
- ✅ FFmpeg skip attributes for graceful video test skipping
