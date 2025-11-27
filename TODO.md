# MediaMatic - TODO

**Last Updated:** 2025-11-27

## 📊 Current Test Status

| Test Suite | Tests | Status |
|------------|-------|--------|
| ASP.NET Core Integration | 51 | ✅ All passing |
| Core Library | 252 | ✅ All passing |
| **Total** | **303** | ✅ All passing |

---

## ✅ Completed (Recent Sessions)

### Metadata Endpoint Tests (Latest)
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
- ✅ Created VfsProviderTestsBase with 13  tests covering:
  - Basic file operations (upload, download, delete, exists)
  - File listing (root, nested paths)
  - Folder operations (list, delete, nested folders)
- ✅ Memory provider: 13 base tests passing
- ✅ Local provider: 16 tests passing (13 base + 3 provider-specific)
- ✅ S3 provider: 14 tests passing (13 base + 1 AWS SDK verification) - via LocalStack
- ✅ Minio provider: 15 tests passing (13 base + 2 provider-specific) - via Testcontainers

### Critical Bug Fixes
- ✅ Fixed Memory provider isolation issue (shared static storage by connection string)
- ✅ Fixed GetMetadataAsync to support non-media files (was throwing "Unsupported file type")
- ✅ Fixed FileEndpoints synchronous I/O issue (request.Form → request.ReadFormAsync)
- ✅ Fixed folder listing to return 404 when folder doesn't exist (was returning 200 with empty array)
- ✅ Fixed folder deletion to return 404 when folder doesn't exist

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

---

## 🚧 In Progress / Remaining Work

### Endpoint Tests (2 remaining)
- ⏸️ ArchiveEndpoints - needs implementation first
- ⏸️ StatsEndpoints - needs implementation first

### Service Implementation (TODOs marked in code)
**High Priority:**
- [ ] Add recursive parameter support to VFS `ListFilesAsync` method
  - Location: `src/MJCZone.MediaMatic.AspNetCore/Services/MediaMaticService.Files.cs:115`
  - Note: FluentStorage extension may not support recursive listing yet

**Medium Priority:**
- [ ] Implement archive creation in `IVfsMethods`
  - Needs: Zip/tar archive generation from folder contents

- [ ] Implement archive job tracking system
  - Needs: Background job infrastructure (consider Hangfire or similar)

**Lower Priority:**
- [ ] Implement folder statistics calculation in `IVfsMethods`
  - Location: `src/MJCZone.MediaMatic.AspNetCore/Services/MediaMaticService.Stats.cs:41`
  - Needs: Recursive file size/count aggregation

- [ ] Implement filesource statistics calculation in `IVfsMethods`
  - Location: `src/MJCZone.MediaMatic.AspNetCore/Services/MediaMaticService.Stats.cs:75`
  - Needs: Full filesource scanning and aggregation

### Additional Testing
- [ ] MediaMaticService unit tests (isolated from VFS/infrastructure)
- [ ] Review OperationContextInitializer route patterns (line 248) - minor code review TODO

---

## 📝 Notes for Next Session

### Current State
- **All core VFS operations working** across Memory, Local, S3, Minio providers
- **All ASP.NET Core endpoints tested** for filesources, files, folders, and transformations
- **Image transformations fully working** - resize, crop, format conversion via URL parameters
- **Memory provider uses shared storage** - connection strings with same name share storage instances
- **Non-media file support** - text files, PDFs, etc. now return basic metadata instead of throwing
- **Metadata endpoint fully tested** - 8 tests covering images, non-media files, error handling
- **303 total tests passing** (51 ASP.NET Core + 252 Core library)

### Recommended Next Steps (Priority Order)
1. **Implement recursive file listing**
   - Add recursive parameter support to `ListFilesAsync`
   - May need VFS layer changes

2. **Consider archive functionality**
   - Design API for creating archives (sync vs async?)
   - Implement background job tracking if needed (Hangfire?)

3. **Documentation** (Phase 5 per PROJECT_ROADMAP.md)
   - XML documentation for public APIs
   - Update VitePress docs site

### Known Limitations
- Archive operations: stubs only (not implemented)
- Statistics operations: stubs only (not implemented)
- Recursive file listing: parameter exists but not fully implemented in VFS layer

### Code TODOs in Source
| File | Line | Description |
|------|------|-------------|
| `MediaMaticService.Files.cs` | 115 | Implement recursive listing |
| `MediaMaticService.Stats.cs` | 41 | Implement folder stats |
| `MediaMaticService.Stats.cs` | 75 | Implement filesource stats |
| `MediaMaticService.Archives.cs` | 42, 75, 109, 137, 166, 193 | Archive operations |
| `OperationContextInitializer.cs` | 248 | Review route pattern segments |

### Test Infrastructure
The testing infrastructure is fully in place and follows the exact pattern used in DapperMatic:
- ✅ WebApplicationFactory pattern for integration tests
- ✅ In-memory repository for test isolation
- ✅ Base test classes for consistent provider coverage
- ✅ Testcontainers ready for S3/Minio (LocalStack, Minio fixtures working)
- ✅ FFmpeg skip attributes for graceful video test skipping

Adding new endpoint tests should be straightforward by following the File/Folder/Transformation endpoint test patterns.
