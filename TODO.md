# MediaMatic - TODO

## ✅ Completed (Recent Session)

### Core VFS Provider Tests
- ✅ Created VfsProviderTestsBase with 13 comprehensive tests covering:
  - Basic file operations (upload, download, delete, exists)
  - File listing (root, nested paths)
  - Folder operations (list, delete, nested folders)
- ✅ Memory provider: 13 base tests passing
- ✅ Local provider: 16 tests passing (13 base + 3 provider-specific)
- ✅ S3 provider: 14 tests passing (13 base + 1 AWS SDK verification)
- ✅ Minio provider: 15 tests passing (13 base + 2 provider-specific)

### Critical Bug Fixes
- ✅ Fixed Memory provider isolation issue (shared static storage by connection string)
- ✅ Fixed GetMetadataAsync to support non-media files (was throwing "Unsupported file type")
- ✅ Fixed FileEndpoints synchronous I/O issue (request.Form → request.ReadFormAsync)
- ✅ Fixed folder listing to return 404 when folder doesn't exist (was returning 200 with empty array)
- ✅ Fixed folder deletion to return 404 when folder doesn't exist

### ASP.NET Core Integration Tests
- ✅ FilesourceEndpoints: 4/4 tests passing
- ✅ FileEndpoints: 13/13 tests passing
  - Upload/download (root, nested, buckets)
  - Delete, exists (HEAD), list
  - Overwrite with PUT
  - Error scenarios
- ✅ FolderEndpoints: 11/11 tests passing
  - List folders/files (root, nested, buckets)
  - Delete folders (nested, buckets)
  - Error scenarios

**All 30 ASP.NET Core integration tests passing! 🎉**

---

## 🚧 In Progress / Remaining Work

### Endpoint Tests (4 remaining)
- ⏸️ ArchiveEndpoints - needs implementation first
- ⏸️ StatsEndpoints - needs implementation first
- ⏸️ MetadataEndpoints - basic structure exists, needs tests
- ⏸️ TransformationEndpoints - needs implementation first

### Service Implementation (TODOs marked in code)
**High Priority:**
- [ ] Implement image transformation in `MediaMaticService.Files.TransformImageAsync`
  - Location: `src/MJCZone.MediaMatic.AspNetCore/Services/MediaMaticService.Files.cs:87`
  - Needs: SkiaSharp integration for resize, crop, format conversion

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
- [ ] Review OperationContextInitializer route patterns (line 248)
- [ ] Consider migrating ASP.NET integration tests from Memory to Local/Minio storage
  - Note: Memory is fine for quick tests, but Local/Minio are more realistic for long-running tests

---

## 📝 Notes for Next Session

### Current State
- **All core VFS operations working** across Memory, Local, S3, Minio providers
- **All ASP.NET Core endpoints tested** for filesources, files, and folders
- **Memory provider now uses shared storage** - connection strings with same name share storage instances
- **Non-media file support** - text files, PDFs, etc. now return basic metadata instead of throwing

### Recommended Next Steps
1. **Implement image transformation** (most requested feature)
   - Use SkiaSharp for resize, crop, format conversion
   - Add transformation endpoint tests

2. **Implement metadata extraction endpoint tests**
   - Test image EXIF extraction
   - Test video metadata extraction
   - Test MIME type detection

3. **Consider archive functionality**
   - Design API for creating archives (sync vs async?)
   - Implement background job tracking if needed

### Known Limitations
- Archive operations: stubs only (not implemented)
- Statistics operations: stubs only (not implemented)
- Image transformations: stub only (not implemented)
- Recursive file listing: parameter exists but not fully implemented in VFS layer

### Test Infrastructure
The testing infrastructure is fully in place and follows the exact pattern used in DapperMatic:
- ✅ WebApplicationFactory pattern for integration tests
- ✅ In-memory repository for test isolation
- ✅ Base test classes for consistent provider coverage
- ✅ Testcontainers ready for S3/Minio/SFTP (fixtures in place)

Adding new endpoint tests should be straightforward by following the File/Folder endpoint test patterns.