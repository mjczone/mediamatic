# Large File Upload Support - Research & Implementation Plan

## Executive Summary

This document contains comprehensive research on implementing large file upload support for MediaMatic, including analysis of the TUS protocol vs custom implementation approaches.

**Recommendation:** Implement a **custom chunked upload solution** rather than adopting the TUS protocol.

**Rationale:** While TUS is an excellent standardized protocol, MediaMatic's unique requirements as a storage abstraction library make a custom implementation more appropriate, allowing direct integration with native cloud provider APIs (S3 multipart, Azure blocks) without intermediate buffering.

---

## Current State: MediaMatic File Upload Architecture

### How Uploads Work Today

MediaMatic uses a **stream-based architecture** for file uploads:

1. **Core Upload Methods** (defined in `IVfsMethods`):
   - `UploadFileAsync(Stream stream, ...)` - Generic file upload
   - `UploadImageAsync(Stream stream, ...)` - Image-specific upload with processing
   - `UploadVideoAsync(Stream stream, ...)` - Video-specific upload with processing

2. **Upload Flow**:
   ```
   HTTP Request/Stream
       ↓
   VfsConnectionExtensions (convenience methods)
       ↓
   IVfsMethods.UploadFileAsync/UploadImageAsync/UploadVideoAsync
       ↓
   VfsMethodsBase (base implementation)
       ↓
   FluentStorage IBlobStorage.WriteAsync (streams to storage)
       ↓
   Storage Provider (S3, Azure, Local, FTP, etc.)
   ```

3. **Configuration**:
   - Upload options via `ImageUploadOptions` and `VideoUploadOptions`
   - Settings via `MediaMaticSettings` (currently minimal)

### Current Capabilities

✅ **What Works Well:**
- Stream-based design (memory efficient)
- Support for 14+ storage providers via FluentStorage
- No hard-coded file size limits
- Provider flexibility

⚠️ **What's Missing:**
- No resumable uploads (failures require full restart)
- No chunked/multipart upload API
- No progress tracking
- No configurable size limits or validation
- No upload timeout configuration
- Relies entirely on underlying storage provider capabilities

### Key Files

- `src/MJCZone.MediaMatic/Interfaces/IVfsMethods.cs` - Upload method definitions
- `src/MJCZone.MediaMatic/Models/ImageUploadOptions.cs` - Image upload configuration
- `src/MJCZone.MediaMatic/Models/VideoUploadOptions.cs` - Video upload configuration
- `src/MJCZone.MediaMatic/Providers/Base/VfsMethodsBase.cs` - Base upload implementation
- `src/MJCZone.MediaMatic/VfsConnectionExtensions.cs` - Convenience methods
- `src/MJCZone.MediaMatic/MediaMaticSettings.cs` - Configuration (currently empty)

---

## TUS Protocol Analysis

### What is TUS?

TUS (Tus Upload Protocol) is an open protocol for resumable file uploads built on HTTP. It enables uploads to be interrupted at any time and resumed from where they stopped without re-uploading previous data.

### How TUS Works

TUS uses three primary HTTP methods:

1. **POST Request - Create Upload**
   - Client creates a new upload resource
   - Specifies file size via `Upload-Length` header
   - Server responds with `Location` header indicating upload URL

2. **PATCH Request - Upload Data**
   - Client sends chunks with `Upload-Offset` header
   - Server responds with updated offset
   - Process repeats until file is complete

3. **HEAD Request - Check Progress**
   - Client queries current upload offset
   - Used to resume interrupted uploads
   - Returns `Upload-Offset` to indicate next byte needed

### TUS Protocol - Advantages

**Standardization & Ecosystem:**
- Open IETF draft specification with broad industry support
- Battle-tested (used by Transloadit for millions of requests globally)
- Rich ecosystem with multiple client/server implementations
- Cross-platform compatibility

**Reliability:**
- Automatic resumption from exact byte offset after interruption
- Bandwidth savings (no re-uploading)
- Designed for unreliable network connections
- Proven at scale in production

**Developer Experience:**
- Ready-made client/server libraries
- Mature JavaScript/TypeScript clients (tus-js-client, Uppy)
- .NET server implementation (tusdotnet)
- Comprehensive documentation

**Technical Capabilities:**
- Native chunking support
- Built-in offset tracking
- Parallel upload extension
- Metadata support via headers
- Extensible with optional features (checksums, expiration, concatenation)

### TUS Protocol - Disadvantages

**Implementation Requirements:**
- Requires BOTH client AND server to implement TUS protocol
- Lock-in: switching requires significant refactoring
- HTTP proxies may strip headers or block PATCH requests
- Learning curve for protocol specifications

**Storage Integration Challenges:**

**S3 Incompatibility:**
- TUS data model conflicts with S3 multipart uploads
- S3 requires minimum 5MB parts (except last part)
- TUS allows flexible chunk sizes
- Requires temporary disk storage before S3 transfer
- No object locking in S3 (eventual consistency issues)

**Azure Limitations:**
- Similar constraints with Azure Blob Storage multipart API
- Requires buffering before cloud transfer

**Intermediate Storage Required:**
- Must buffer chunks to disk before cloud transfer
- Increases latency
- Requires local disk space
- Additional I/O operations

**Architectural Constraints:**
- Cannot leverage native cloud provider multipart APIs directly
- No native file/filename concept (workaround with headers)
- Part count limits (S3 limited to 1,000-10,000 parts)
- Performance overhead from extra abstraction layer

**Production Considerations:**
- Maintenance burden (dependent on third-party library updates)
- Version compatibility with protocol evolution
- Debugging complexity at protocol level
- Resource usage for temporary storage and buffering

### .NET TUS Implementation: tusdotnet

**Package Information:**
- Name: tusdotnet (MIT License)
- Latest Version: 2.10.0
- Platform Support: .NET Framework, .NET Standard 1.3+, .NET 6+
- GitHub: 800+ stars, actively maintained

**Features:**
- Full TUS 1.0.0 protocol support
- All major extensions (checksum, concatenation, creation, expiration, termination)
- ASP.NET Core middleware (`app.MapTus`, `app.UseTus`)
- Customizable storage backends
- Pipeline support (.NET Core 3.1+) for better performance

**Performance:**
- Streams vs Pipelines: nearly identical performance
- 1MB files: ~1.7 seconds for 20 files
- 10MB files: ~4.8 seconds for 20 files
- Uses ArrayPool<T> for memory efficiency
- Scalable to high-volume scenarios

**Storage Backend Support:**
- Local disk (default)
- Custom implementations (community S3/Azure extensions available)
- Requires additional packages for native cloud support

### JavaScript/TypeScript TUS Clients

**tus-js-client:**
- Official TUS client implementation
- Version: 4.3.1 (actively maintained)
- Platforms: Browsers, Node.js, React Native, Cordova
- Features: Full protocol support, retry logic, progress callbacks
- TypeScript: Full type definitions included

**Uppy with TUS Plugin:**
- Package: @uppy/tus
- Integration: Wraps tus-js-client with UI components
- Features: File picker, drag-drop, progress bars, resumable uploads
- Best for: Complete upload UX solution

**Developer Experience:**
- Simple API for upload creation and management
- Built-in retry and error handling
- Progress events for UI updates
- Automatic resume on connection restore

---

## TUS vs Custom Implementation Comparison

| **Aspect** | **TUS Protocol** | **Custom Implementation** |
|------------|------------------|---------------------------|
| **Complexity** | Medium (protocol compliance required) | Low-Medium (direct API usage) |
| **Client Requirements** | Must use TUS-compatible library | Any HTTP client (fetch, axios, etc.) |
| **Server Requirements** | tusdotnet + storage backend | Direct cloud SDK integration |
| **S3 Integration** | ❌ Incompatible, requires buffering | ✅ Native multipart upload API |
| **Azure Integration** | ❌ Incompatible, requires buffering | ✅ Native block blob API |
| **Local Storage** | ✅ Direct file writing | ✅ Direct file writing |
| **FTP/SFTP** | ⚠️ Sequential write (limited resumption) | ⚠️ Sequential write (limited resumption) |
| **Flexibility** | ⚠️ Protocol-constrained | ✅ Full control over implementation |
| **Customization** | ⚠️ Limited to protocol extensions | ✅ Unlimited |
| **Performance** | ⚠️ Extra abstraction layer | ✅ Direct cloud API calls |
| **Latency** | ⚠️ Higher (buffering, processing) | ✅ Lower (direct upload) |
| **Disk Usage** | ❌ Requires temporary storage | ✅ Optional (depends on strategy) |
| **Maintenance** | ⚠️ Dependent on library updates | ✅ Self-contained |
| **Debugging** | ⚠️ Protocol-level complexity | ✅ Straightforward |
| **Standards Compliance** | ✅ Full TUS 1.0.0 | ⚠️ Custom approach |
| **Cross-Platform** | ✅ Excellent (standard protocol) | ✅ Good (if documented) |
| **Ecosystem** | ✅ Rich (clients, servers, tools) | ⚠️ Build your own |
| **Learning Curve** | ⚠️ Steeper (protocol knowledge) | ✅ Gentler (familiar patterns) |

---

## Custom Implementation Strategy

### Recommendation: Custom Chunked Upload Implementation

**Why Custom for MediaMatic:**

1. **Architecture Alignment**: MediaMatic abstracts 14+ storage providers. Custom implementation allows:
   - Direct integration with each provider's native APIs
   - Provider-specific optimizations (S3 multipart, Azure blocks)
   - Unified abstraction matching MediaMatic's existing patterns

2. **Performance & Efficiency**:
   - No intermediate buffering to disk
   - Direct streaming to cloud storage
   - Lower latency and resource usage
   - Better utilization of provider-native features

3. **Flexibility & Control**:
   - Full control over metadata structure
   - Custom progress tracking tailored to MediaMatic's needs
   - Provider-specific error handling and retry logic
   - No protocol constraints

4. **Maintenance & Simplicity**:
   - No dependency on third-party protocol library
   - Self-contained implementation
   - Easier debugging and troubleshooting
   - Incremental rollout per provider

5. **Cloud Storage Compatibility**:
   - TUS has fundamental incompatibilities with S3/Azure multipart APIs
   - Custom implementation leverages native cloud features
   - No workarounds or buffering required

### When TUS Would Be Better

TUS would be preferable if:
- You need interoperability with existing TUS clients/servers
- You're building a file upload service (not a storage abstraction)
- You want to avoid custom implementation work
- Standardization is more important than performance
- You only support a single storage backend

---

## Implementation Plan

### Core Interfaces

```csharp
namespace MJCZone.MediaMatic.Interfaces
{
    /// <summary>
    /// Represents a chunked upload session.
    /// </summary>
    public interface IChunkedUploadSession
    {
        /// <summary>
        /// Unique identifier for this upload session.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Destination path for the uploaded file.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Total size of the file being uploaded in bytes.
        /// </summary>
        long TotalSize { get; }

        /// <summary>
        /// Number of bytes successfully uploaded so far.
        /// </summary>
        long UploadedBytes { get; }

        /// <summary>
        /// When the upload session was created.
        /// </summary>
        DateTime CreatedAt { get; }

        /// <summary>
        /// When the upload session expires (optional).
        /// </summary>
        DateTime? ExpiresAt { get; }

        /// <summary>
        /// Additional metadata for the upload.
        /// </summary>
        IDictionary<string, string>? Metadata { get; }
    }

    /// <summary>
    /// Result of uploading a chunk.
    /// </summary>
    public class ChunkUploadResult
    {
        /// <summary>
        /// New offset after uploading this chunk.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// ETag or identifier for the uploaded chunk (provider-specific).
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Part number (for multipart uploads).
        /// </summary>
        public int? PartNumber { get; set; }
    }

    /// <summary>
    /// Progress information for an upload.
    /// </summary>
    public class UploadProgress
    {
        /// <summary>
        /// Bytes uploaded so far.
        /// </summary>
        public long UploadedBytes { get; set; }

        /// <summary>
        /// Total file size in bytes.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Percentage complete (0-100).
        /// </summary>
        public double Percentage => TotalBytes > 0 ? (UploadedBytes * 100.0 / TotalBytes) : 0;

        /// <summary>
        /// Upload speed in bytes per second.
        /// </summary>
        public long? BytesPerSecond { get; set; }

        /// <summary>
        /// Estimated time remaining.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    /// <summary>
    /// Options for chunked uploads.
    /// </summary>
    public class ChunkedUploadOptions
    {
        /// <summary>
        /// Size of each chunk in bytes. Default is 5MB.
        /// </summary>
        public long ChunkSize { get; set; } = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Maximum file size allowed in bytes. Null for unlimited.
        /// </summary>
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// Allowed file types/extensions.
        /// </summary>
        public IList<string>? AllowedFileTypes { get; set; }

        /// <summary>
        /// Session expiration duration. Default is 7 days.
        /// </summary>
        public TimeSpan SessionExpiration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Progress callback invoked after each chunk upload.
        /// </summary>
        public Action<UploadProgress>? OnProgress { get; set; }

        /// <summary>
        /// Additional metadata to store with the upload.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Support for chunked/resumable uploads.
    /// </summary>
    public interface IChunkedUploadSupport
    {
        /// <summary>
        /// Initiates a new chunked upload session.
        /// </summary>
        Task<IChunkedUploadSession> InitiateUploadAsync(
            string path,
            long totalSize,
            ChunkedUploadOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a chunk of data.
        /// </summary>
        Task<ChunkUploadResult> UploadChunkAsync(
            string sessionId,
            long offset,
            Stream chunkData,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current status of an upload session.
        /// </summary>
        Task<IChunkedUploadSession> GetUploadStatusAsync(
            string sessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes the upload and finalizes the file.
        /// </summary>
        Task CompleteUploadAsync(
            string sessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Aborts an upload session and cleans up resources.
        /// </summary>
        Task AbortUploadAsync(
            string sessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists active upload sessions (optional, for management).
        /// </summary>
        Task<IEnumerable<IChunkedUploadSession>> ListSessionsAsync(
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for persisting upload session state.
    /// </summary>
    public interface IUploadSessionRepository
    {
        Task SaveAsync(IChunkedUploadSession session, CancellationToken cancellationToken = default);
        Task<IChunkedUploadSession?> GetAsync(string sessionId, CancellationToken cancellationToken = default);
        Task UpdateAsync(IChunkedUploadSession session, CancellationToken cancellationToken = default);
        Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<IChunkedUploadSession>> ListAsync(CancellationToken cancellationToken = default);
        Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
    }
}
```

### Configuration Extensions

```csharp
namespace MJCZone.MediaMatic
{
    public class MediaMaticSettings
    {
        // ... existing settings ...

        /// <summary>
        /// Settings for chunked/resumable uploads.
        /// </summary>
        public ChunkedUploadSettings? ChunkedUpload { get; set; }
    }

    public class ChunkedUploadSettings
    {
        /// <summary>
        /// Enable chunked upload support. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default chunk size in bytes. Default is 5MB.
        /// </summary>
        public long DefaultChunkSize { get; set; } = 5 * 1024 * 1024;

        /// <summary>
        /// Maximum file size in bytes. Null for unlimited.
        /// </summary>
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// Minimum chunk size in bytes. Default is 1MB.
        /// </summary>
        public long MinChunkSize { get; set; } = 1 * 1024 * 1024;

        /// <summary>
        /// Maximum chunk size in bytes. Default is 100MB.
        /// </summary>
        public long MaxChunkSize { get; set; } = 100 * 1024 * 1024;

        /// <summary>
        /// Session expiration duration. Default is 7 days.
        /// </summary>
        public TimeSpan SessionExpiration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Cleanup interval for expired sessions. Default is 1 hour.
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Allowed file types/extensions. Null for no restrictions.
        /// </summary>
        public IList<string>? AllowedFileTypes { get; set; }

        /// <summary>
        /// Repository type for session persistence.
        /// </summary>
        public SessionRepositoryType RepositoryType { get; set; } = SessionRepositoryType.Memory;

        /// <summary>
        /// Connection string for session repository (if applicable).
        /// </summary>
        public string? RepositoryConnectionString { get; set; }
    }

    public enum SessionRepositoryType
    {
        Memory,
        File,
        Database,
        Redis
    }
}
```

### S3 Provider Implementation Example

```csharp
using Amazon.S3;
using Amazon.S3.Model;

namespace MJCZone.MediaMatic.Providers.S3
{
    public class S3ChunkedUploadProvider : IChunkedUploadSupport
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly IUploadSessionRepository _sessionRepository;
        private readonly ILogger<S3ChunkedUploadProvider> _logger;

        public S3ChunkedUploadProvider(
            IAmazonS3 s3Client,
            string bucketName,
            IUploadSessionRepository sessionRepository,
            ILogger<S3ChunkedUploadProvider> logger)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IChunkedUploadSession> InitiateUploadAsync(
            string path,
            long totalSize,
            ChunkedUploadOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // Validate file size
            if (options?.MaxFileSize.HasValue == true && totalSize > options.MaxFileSize.Value)
            {
                throw new InvalidOperationException(
                    $"File size {totalSize} exceeds maximum allowed size {options.MaxFileSize.Value}");
            }

            // Initiate S3 multipart upload
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = _bucketName,
                Key = path
            };

            // Add metadata
            if (options?.Metadata != null)
            {
                foreach (var (key, value) in options.Metadata)
                {
                    request.Metadata[key] = value;
                }
            }

            var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            // Create session
            var session = new ChunkedUploadSession
            {
                SessionId = response.UploadId,
                FilePath = path,
                TotalSize = totalSize,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(options?.SessionExpiration ?? TimeSpan.FromDays(7)),
                Metadata = options?.Metadata,
                ProviderData = new Dictionary<string, object>
                {
                    ["BucketName"] = _bucketName,
                    ["Parts"] = new List<PartETag>()
                }
            };

            await _sessionRepository.SaveAsync(session, cancellationToken);

            _logger.LogInformation(
                "Initiated multipart upload for {Path} with session {SessionId}",
                path, session.SessionId);

            return session;
        }

        public async Task<ChunkUploadResult> UploadChunkAsync(
            string sessionId,
            long offset,
            Stream chunkData,
            CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new InvalidOperationException($"Upload session {sessionId} not found");
            }

            // Check if session has expired
            if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException($"Upload session {sessionId} has expired");
            }

            // Calculate part number (1-based)
            var partNumber = (int)(offset / (5 * 1024 * 1024)) + 1;

            // Upload part to S3
            var request = new UploadPartRequest
            {
                BucketName = _bucketName,
                Key = session.FilePath,
                UploadId = sessionId,
                PartNumber = partNumber,
                InputStream = chunkData
            };

            var response = await _s3Client.UploadPartAsync(request, cancellationToken);

            // Store part ETag
            var parts = (List<PartETag>)session.ProviderData["Parts"];
            parts.Add(new PartETag(partNumber, response.ETag));

            // Update session progress
            session.UploadedBytes = offset + chunkData.Length;
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            _logger.LogDebug(
                "Uploaded part {PartNumber} for session {SessionId} ({UploadedBytes}/{TotalSize} bytes)",
                partNumber, sessionId, session.UploadedBytes, session.TotalSize);

            return new ChunkUploadResult
            {
                Offset = session.UploadedBytes,
                ETag = response.ETag,
                PartNumber = partNumber
            };
        }

        public async Task<IChunkedUploadSession> GetUploadStatusAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new InvalidOperationException($"Upload session {sessionId} not found");
            }

            return session;
        }

        public async Task CompleteUploadAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                throw new InvalidOperationException($"Upload session {sessionId} not found");
            }

            var parts = (List<PartETag>)session.ProviderData["Parts"];

            // Complete S3 multipart upload
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = _bucketName,
                Key = session.FilePath,
                UploadId = sessionId,
                PartETags = parts.OrderBy(p => p.PartNumber).ToList()
            };

            await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            // Clean up session
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);

            _logger.LogInformation(
                "Completed multipart upload for {Path} with session {SessionId}",
                session.FilePath, sessionId);
        }

        public async Task AbortUploadAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                // Session already deleted or never existed
                return;
            }

            // Abort S3 multipart upload
            var request = new AbortMultipartUploadRequest
            {
                BucketName = _bucketName,
                Key = session.FilePath,
                UploadId = sessionId
            };

            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);

            // Clean up session
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);

            _logger.LogInformation(
                "Aborted multipart upload for {Path} with session {SessionId}",
                session.FilePath, sessionId);
        }

        public async Task<IEnumerable<IChunkedUploadSession>> ListSessionsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _sessionRepository.ListAsync(cancellationToken);
        }
    }
}
```

### TypeScript Client Example

```typescript
export interface UploadSession {
    sessionId: string;
    filePath: string;
    totalSize: number;
    uploadedBytes: number;
    createdAt: string;
    expiresAt?: string;
}

export interface UploadProgress {
    uploadedBytes: number;
    totalBytes: number;
    percentage: number;
    bytesPerSecond?: number;
    estimatedTimeRemaining?: number;
}

export interface UploadOptions {
    chunkSize?: number;
    maxFileSize?: number;
    allowedFileTypes?: string[];
    metadata?: Record<string, string>;
    onProgress?: (progress: UploadProgress) => void;
    onError?: (error: Error) => void;
}

export class MediaMaticUploader {
    private readonly baseUrl: string;
    private readonly defaultChunkSize = 5 * 1024 * 1024; // 5MB

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    /**
     * Upload a file with chunking and resumption support.
     */
    async upload(
        file: File,
        path: string,
        options: UploadOptions = {}
    ): Promise<void> {
        const chunkSize = options.chunkSize || this.defaultChunkSize;

        // Validate file size
        if (options.maxFileSize && file.size > options.maxFileSize) {
            throw new Error(
                `File size ${file.size} exceeds maximum allowed size ${options.maxFileSize}`
            );
        }

        // Validate file type
        if (options.allowedFileTypes && options.allowedFileTypes.length > 0) {
            const extension = file.name.split('.').pop()?.toLowerCase();
            if (!extension || !options.allowedFileTypes.includes(extension)) {
                throw new Error(
                    `File type .${extension} is not allowed. Allowed types: ${options.allowedFileTypes.join(', ')}`
                );
            }
        }

        // Initiate upload
        const session = await this.initiateUpload(path, file.size, options.metadata);

        try {
            // Upload chunks
            const startTime = Date.now();
            let offset = 0;

            while (offset < file.size) {
                const chunkEnd = Math.min(offset + chunkSize, file.size);
                const chunk = file.slice(offset, chunkEnd);

                await this.uploadChunk(session.sessionId, offset, chunk);

                offset = chunkEnd;

                // Report progress
                if (options.onProgress) {
                    const elapsed = (Date.now() - startTime) / 1000;
                    const bytesPerSecond = elapsed > 0 ? offset / elapsed : 0;
                    const remainingBytes = file.size - offset;
                    const estimatedTimeRemaining =
                        bytesPerSecond > 0 ? remainingBytes / bytesPerSecond : undefined;

                    options.onProgress({
                        uploadedBytes: offset,
                        totalBytes: file.size,
                        percentage: (offset / file.size) * 100,
                        bytesPerSecond,
                        estimatedTimeRemaining
                    });
                }
            }

            // Complete upload
            await this.completeUpload(session.sessionId);
        } catch (error) {
            // On error, session remains for potential resumption
            if (options.onError) {
                options.onError(error as Error);
            }
            throw error;
        }
    }

    /**
     * Resume a previously interrupted upload.
     */
    async resume(
        sessionId: string,
        file: File,
        options: UploadOptions = {}
    ): Promise<void> {
        const chunkSize = options.chunkSize || this.defaultChunkSize;

        // Get current status
        const session = await this.getStatus(sessionId);

        // Validate file matches session
        if (file.size !== session.totalSize) {
            throw new Error('File size does not match upload session');
        }

        try {
            // Resume from last offset
            const startTime = Date.now();
            let offset = session.uploadedBytes;

            while (offset < file.size) {
                const chunkEnd = Math.min(offset + chunkSize, file.size);
                const chunk = file.slice(offset, chunkEnd);

                await this.uploadChunk(sessionId, offset, chunk);

                offset = chunkEnd;

                // Report progress
                if (options.onProgress) {
                    const elapsed = (Date.now() - startTime) / 1000;
                    const uploadedSinceResume = offset - session.uploadedBytes;
                    const bytesPerSecond = elapsed > 0 ? uploadedSinceResume / elapsed : 0;
                    const remainingBytes = file.size - offset;
                    const estimatedTimeRemaining =
                        bytesPerSecond > 0 ? remainingBytes / bytesPerSecond : undefined;

                    options.onProgress({
                        uploadedBytes: offset,
                        totalBytes: file.size,
                        percentage: (offset / file.size) * 100,
                        bytesPerSecond,
                        estimatedTimeRemaining
                    });
                }
            }

            // Complete upload
            await this.completeUpload(sessionId);
        } catch (error) {
            if (options.onError) {
                options.onError(error as Error);
            }
            throw error;
        }
    }

    /**
     * Abort an upload session.
     */
    async abort(sessionId: string): Promise<void> {
        const response = await fetch(`${this.baseUrl}/uploads/${sessionId}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error(`Failed to abort upload: ${response.statusText}`);
        }
    }

    /**
     * Get status of an upload session.
     */
    async getStatus(sessionId: string): Promise<UploadSession> {
        const response = await fetch(`${this.baseUrl}/uploads/${sessionId}`);

        if (!response.ok) {
            throw new Error(`Failed to get upload status: ${response.statusText}`);
        }

        return await response.json();
    }

    private async initiateUpload(
        path: string,
        totalSize: number,
        metadata?: Record<string, string>
    ): Promise<UploadSession> {
        const response = await fetch(`${this.baseUrl}/uploads`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                path,
                totalSize,
                metadata
            })
        });

        if (!response.ok) {
            throw new Error(`Failed to initiate upload: ${response.statusText}`);
        }

        return await response.json();
    }

    private async uploadChunk(
        sessionId: string,
        offset: number,
        chunk: Blob
    ): Promise<void> {
        const response = await fetch(
            `${this.baseUrl}/uploads/${sessionId}/chunks?offset=${offset}`,
            {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/octet-stream'
                },
                body: chunk
            }
        );

        if (!response.ok) {
            throw new Error(`Failed to upload chunk: ${response.statusText}`);
        }
    }

    private async completeUpload(sessionId: string): Promise<void> {
        const response = await fetch(`${this.baseUrl}/uploads/${sessionId}/complete`, {
            method: 'POST'
        });

        if (!response.ok) {
            throw new Error(`Failed to complete upload: ${response.statusText}`);
        }
    }
}

// Usage example
const uploader = new MediaMaticUploader('https://api.example.com');

const file = document.getElementById('fileInput').files[0];

await uploader.upload(file, '/uploads/myfile.mp4', {
    chunkSize: 10 * 1024 * 1024, // 10MB chunks
    maxFileSize: 5 * 1024 * 1024 * 1024, // 5GB max
    allowedFileTypes: ['mp4', 'mov', 'avi'],
    metadata: {
        userId: '12345',
        category: 'videos'
    },
    onProgress: (progress) => {
        console.log(`Upload progress: ${progress.percentage.toFixed(2)}%`);
        console.log(`Speed: ${(progress.bytesPerSecond / 1024 / 1024).toFixed(2)} MB/s`);
        if (progress.estimatedTimeRemaining) {
            console.log(`ETA: ${progress.estimatedTimeRemaining.toFixed(0)}s`);
        }
    },
    onError: (error) => {
        console.error('Upload error:', error);
        // Save sessionId for potential resumption
    }
});
```

---

## Implementation Phases

### Phase 1: Core Infrastructure (Week 1)
- Define `IChunkedUploadSession`, `IChunkedUploadSupport`, `IUploadSessionRepository`
- Create models: `ChunkedUploadOptions`, `ChunkUploadResult`, `UploadProgress`
- Implement in-memory session repository
- Add configuration to `MediaMaticSettings`
- Unit tests for interfaces and models

### Phase 2: S3 Provider (Weeks 2-3)
- Implement `S3ChunkedUploadProvider` using AWS SDK multipart API
- Use `InitiateMultipartUpload` → `UploadPart` → `CompleteMultipartUpload`
- Store part ETags for completion
- Add retry logic with exponential backoff
- Handle abort/cleanup on failure
- Integration tests with LocalStack or real S3

### Phase 3: Azure Provider (Weeks 3-4)
- Implement `AzureChunkedUploadProvider` using Azure SDK block blob API
- Use `PutBlock` → `PutBlockList`
- Manage block IDs and ordering
- Add retry logic
- Handle cleanup
- Integration tests with Azurite or real Azure

### Phase 4: Local Provider (Week 4)
- Implement `LocalChunkedUploadProvider` with chunked file writing
- Support resume by checking existing file size
- Validate chunk integrity (optional checksums)
- Handle partial uploads
- Unit and integration tests

### Phase 5: Validation & Configuration (Week 5)
- Implement pre-upload validation: file size, type, MIME type
- Add configurable size limits in settings
- File type whitelisting/blacklisting
- Validate chunk offsets and sizes
- Comprehensive validation tests

### Phase 6: Progress Tracking (Week 5)
- Add progress callbacks to upload methods
- Calculate percentage, speed, ETA
- Implement status query methods
- Add progress event aggregation
- Progress tracking tests

### Phase 7: Session Management (Week 6)
- Implement file-based session repository (JSON)
- Add session expiration/cleanup background task
- Support session resumption across restarts
- Session listing and querying
- Cleanup tests

### Phase 8: TypeScript Client Library (Week 7-8)
- Create `MediaMaticUploader` class
- Implement automatic file chunking
- Add progress event callbacks
- Support upload pause/resume
- Handle network errors with retry
- Provide TypeScript definitions
- Client-side tests

### Phase 9: ASP.NET Core Integration (Week 8)
- Create minimal API endpoints for upload operations
- Session management middleware
- Progress query endpoints
- Authorization integration
- Example ASP.NET Core project

### Phase 10: Documentation & Testing (Week 9)
- Comprehensive API documentation
- Integration guides for each provider
- Performance benchmarks
- Example applications (web, console)
- Migration guide from simple uploads

---

## Provider Implementation Priority

### High Priority
1. **S3** - Most common cloud provider, native multipart upload API
2. **Azure** - Second most common, block blob API
3. **Local** - Development, testing, and simple deployments

### Medium Priority
4. **GCP** - Growing cloud provider, resumable upload API
5. **Memory** - Testing and ephemeral storage

### Low Priority
6. **FTP/SFTP** - Limited resumability (sequential write)
7. **Other providers** - Implement as needed based on demand

---

## Performance Considerations

### Optimal Chunk Sizes

**S3:**
- Minimum part size: 5MB (except last part)
- Maximum parts: 10,000
- Recommended chunk size: 5-100MB depending on file size
- Formula: `chunkSize = max(5MB, fileSize / 10000)`

**Azure:**
- Maximum block size: 4000MB (4GB)
- Maximum blocks: 50,000
- Recommended chunk size: 4-100MB
- Formula: `chunkSize = max(4MB, fileSize / 50000)`

**Local:**
- No specific constraints
- Recommended: 1-10MB for balanced performance
- Consider disk I/O characteristics

### Retry Strategy

**Exponential Backoff:**
```
Delay = BaseDelay * (2 ^ attemptNumber) + Random(0, Jitter)
BaseDelay = 1 second
MaxRetries = 5
Jitter = 500ms
```

**Retry Conditions:**
- Network timeouts
- HTTP 5xx errors
- HTTP 429 (Too Many Requests)
- Transient connection errors

**Don't Retry:**
- HTTP 4xx (except 429) - client errors
- Invalid credentials
- File not found
- Invalid chunk offset

### Memory Management

**Streaming:**
- Never load entire file into memory
- Use streams for chunk uploads
- Dispose streams properly after upload
- Use `ArrayPool<byte>` for buffers

**Session Storage:**
- In-memory: Fast, but lost on restart
- File-based: Persistent, slower
- Database: Scalable, requires infrastructure
- Redis: Fast + persistent, requires infrastructure

---

## Security Considerations

### Authentication & Authorization
- Validate user permissions before initiating upload
- Generate secure, unpredictable session IDs (GUID)
- Validate session ownership on each chunk upload
- Implement rate limiting per user/IP

### Validation
- Validate file size before initiating upload
- Validate chunk offset and size on each upload
- Verify total uploaded size doesn't exceed declared size
- Validate file type/MIME type
- Scan for malware (integrate with AV service)

### Session Management
- Implement session expiration (default 7 days)
- Clean up expired sessions regularly
- Limit maximum concurrent sessions per user
- Secure session storage (encrypt if containing sensitive data)

### Data Integrity
- Calculate checksums for chunks (MD5, SHA256)
- Verify checksums on upload
- Use ETags for part tracking
- Validate final file integrity after completion

---

## Monitoring & Observability

### Metrics to Track
- Active upload sessions
- Upload success/failure rates
- Average upload speed
- Chunk retry rates
- Session expiration/cleanup rates
- Storage consumed by incomplete uploads
- Average time to completion

### Logging
- Session lifecycle events (initiate, complete, abort)
- Chunk upload events with size and duration
- Errors and retry attempts
- Cleanup operations
- Performance metrics

### Alerting
- High failure rates
- Excessive retry rates
- Storage quota warnings
- Long-running sessions
- Cleanup failures

---

## Future Enhancements

### Phase 2 Features
1. **Parallel chunk uploads** - Upload multiple chunks simultaneously
2. **Compression** - Compress chunks before upload (configurable)
3. **Encryption** - Client-side encryption before upload
4. **Deduplication** - Skip uploading duplicate chunks
5. **Bandwidth throttling** - Limit upload speed
6. **Network quality detection** - Adjust chunk size based on connection

### Advanced Features
1. **P2P uploads** - Peer-to-peer chunk distribution (WebRTC)
2. **CDN integration** - Upload to edge locations
3. **Intelligent routing** - Select optimal storage provider
4. **Cost optimization** - Choose cheapest provider for file characteristics
5. **Multi-region** - Upload to multiple regions simultaneously
6. **Webhook notifications** - Notify on upload completion

---

## References

### TUS Protocol
- Official Spec: https://tus.io/protocols/resumable-upload
- GitHub: https://github.com/tus
- tusdotnet: https://github.com/tusdotnet/tusdotnet
- tus-js-client: https://github.com/tus/tus-js-client

### Cloud Provider APIs
- AWS S3 Multipart: https://docs.aws.amazon.com/AmazonS3/latest/userguide/mpuoverview.html
- Azure Block Blobs: https://learn.microsoft.com/en-us/rest/api/storageservices/put-block
- GCP Resumable: https://cloud.google.com/storage/docs/resumable-uploads

### Related Technologies
- FluentStorage: https://github.com/robinrodricks/FluentStorage
- Uppy: https://uppy.io/
- FilePond: https://pqina.nl/filepond/

---

## Conclusion

Implementing custom chunked upload support in MediaMatic will provide:

✅ **Resumable uploads** - Continue interrupted uploads from last successful offset
✅ **Large file support** - Handle multi-GB files efficiently
✅ **Progress tracking** - Real-time upload progress for UX
✅ **Cloud optimization** - Leverage native S3/Azure multipart APIs
✅ **Flexibility** - Provider-specific optimizations
✅ **Control** - Full ownership of implementation

The custom approach aligns with MediaMatic's architecture as a storage abstraction library and avoids the constraints and performance overhead of the TUS protocol.

Implementation can proceed incrementally, starting with core infrastructure and S3 support, then expanding to other providers as needed.
