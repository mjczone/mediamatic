# ASP.NET Core Integration

MediaMatic provides seamless integration with ASP.NET Core applications through the `MJCZone.MediaMatic.AspNetCore` package.

## Installation

```bash
dotnet add package MJCZone.MediaMatic.AspNetCore
```

## Service Registration

### Basic Setup

```csharp
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add MediaMatic services
builder.Services.AddMediaMatic();

var app = builder.Build();

// Configure MediaMatic (adds middleware and maps REST API endpoints)
app.UseMediaMatic();

app.Run();
```

### Advanced: Manual Endpoint Mapping

If you need to control middleware separately or integrate with existing middleware pipelines:

```csharp
var app = builder.Build();

// Add your custom middleware first
app.UseAuthentication();
app.UseAuthorization();

// Just map MediaMatic endpoints (no middleware)
app.MapMediaMaticEndpoints();

app.Run();
```

**Note**: `UseMediaMatic()` is the recommended approach as it automatically:
- Adds the MediaMatic middleware
- Maps all REST API endpoints
- Allows optional custom middleware configuration via callback

## Configuration Options

### MediaMaticOptions Properties

```csharp
builder.Services.Configure<MediaMaticOptions>(options =>
{
    // Base path for all MediaMatic endpoints (default: "/api/mm")
    options.BasePath = "/api/media";

    // Require authentication for all endpoints (default: false)
    options.RequireAuthentication = true;

    // Require a specific role for write operations
    options.RequireRole = "Editor";

    // Allow read-only access with a different role
    options.ReadOnlyRole = "Viewer";

    // Enable CORS (default: false)
    options.EnableCors = true;
    options.CorsPolicyName = "MediaMaticCorsPolicy";

    // Encryption key for connection strings (base64-encoded 256-bit key)
    options.ConnectionStringEncryptionKey = "your-base64-encoded-key";
});
```

### Configuration from appsettings.json

```json
{
  "MediaMatic": {
    "BasePath": "/api/mm",
    "RequireAuthentication": false,
    "RequireRole": "Editor",
    "ReadOnlyRole": "Viewer",
    "EnableCors": false,
    "CorsPolicyName": "MediaMaticCorsPolicy",
    "Filesources": [
      {
        "Id": "local-storage",
        "Provider": "Local",
        "ConnectionString": "file:///var/media",
        "DisplayName": "Local File Storage",
        "Description": "Server filesystem storage",
        "Tags": ["local", "production"],
        "IsEnabled": true
      }
    ],
    "ConnectionStringEncryptionKey": "base64-encoded-key"
  }
}
```

```csharp
builder.Services.Configure<MediaMaticOptions>(
    builder.Configuration.GetSection("MediaMatic")
);

builder.Services.AddMediaMatic();
```

## Filesource Configuration

Filesources define storage backends for MediaMatic. You can configure them using the fluent API or via appsettings.json.

### Fluent Configuration

```csharp
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

builder.Services.AddMediaMatic(config =>
{
    // Add a single filesource
    config.WithFilesource(new FilesourceDto
    {
        Id = "my-storage",
        Provider = "Memory",
        ConnectionString = "memory://",
        DisplayName = "In-Memory Storage",
        Description = "Development storage",
        Tags = ["dev", "test"],
        IsEnabled = true
    });

    // Or add multiple filesources
    config.WithFilesources(
        new FilesourceDto
        {
            Id = "local-files",
            Provider = "Local",
            ConnectionString = "file:///var/media",
            DisplayName = "Local Storage",
            IsEnabled = true
        },
        new FilesourceDto
        {
            Id = "s3-bucket",
            Provider = "S3",
            ConnectionString = "s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1",
            DisplayName = "AWS S3 Storage",
            IsEnabled = true
        }
    );
});
```

### Filesource Providers

MediaMatic supports these provider types:

| Provider | ConnectionString Format | Example |
|----------|------------------------|---------|
| **Memory** | `memory://` | `memory://` |
| **Local** | `file:///path/to/folder` | `file:///var/media` |
| **S3** | `s3://keyId=...;key=...;bucket=...;region=...` | `s3://keyId=AKIAIOSFODNN7EXAMPLE;key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;bucket=my-bucket;region=us-east-1` |
| **Minio** | `minio://endpoint=...;keyId=...;key=...;bucket=...` | `minio://endpoint=localhost:9000;keyId=minioadmin;key=minioadmin;bucket=my-bucket` |
| **GCP** | `gcp://projectId=...;bucket=...;jsonKey=...` | `gcp://projectId=my-project;bucket=my-bucket;jsonKey=/path/to/key.json` |
| **Azure** | `azure://account=...;key=...;container=...` | `azure://account=myaccount;key=...;container=my-container` |
| **SFTP** | `sftp://host=...;username=...;password=...` | `sftp://host=example.com;username=user;password=pass;root=/uploads` |
| **ZipFile** | `zip:///path/to/file.zip` | `zip:///var/archives/media.zip` |

### FilesourceDto Properties

```csharp
public sealed class FilesourceDto
{
    // Unique identifier for this filesource
    public string? Id { get; set; }

    // Provider type: "Memory", "Local", "S3", "Minio", "GCP", "Azure", "SFTP", "ZipFile"
    public string? Provider { get; set; }

    // Provider-specific connection string
    public string? ConnectionString { get; set; }

    // Human-readable display name
    public string? DisplayName { get; set; }

    // Optional description
    public string? Description { get; set; }

    // Tags for categorization
    public ICollection<string>? Tags { get; set; }

    // Enable/disable this filesource (default: true)
    public bool? IsEnabled { get; set; }

    // Auto-populated timestamps
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
```

## Filesource Repository Configuration

By default, MediaMatic uses an in-memory repository to store filesource configurations. You can configure alternative storage:

### File-Based Repository

```csharp
builder.Services.AddMediaMatic(config =>
{
    config.UseFileFilesourceRepository("/var/mediamatic/filesources.json");

    // Pre-configure filesources (will be saved to the file)
    config.WithFilesource(new FilesourceDto
    {
        Id = "default",
        Provider = "Local",
        ConnectionString = "file:///var/media"
    });
});
```

### Database Repository

MediaMatic can store filesource configurations in a database. It uses DapperMatic's `IDbConnectionFactory` for database operations, which is automatically registered by `AddMediaMatic()`.

```csharp
builder.Services.AddMediaMatic(config =>
{
    config.UseDatabaseFilesourceRepository(
        provider: "postgresql",  // or "sqlserver", "mysql", "sqlite"
        connectionString: builder.Configuration.GetConnectionString("MediaMatic")
    );
});
```

**Integration with DapperMatic:** If you're using [DapperMatic](https://github.com/mjczone/MJCZone.DapperMatic) in the same project, MediaMatic will automatically use DapperMatic's `IDbConnectionFactory` (thanks to `TryAddSingleton`). Both libraries can share the same database connection infrastructure.

The repository will automatically create the `mm_filesources` table on startup.

### Custom Repository

```csharp
public class MyCustomRepository : IMediaMaticFilesourceRepository
{
    // Implement custom storage logic
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomFilesourceRepository<MyCustomRepository>();
});
```

## Custom Implementations

### Custom Filesource ID Factory

By default, filesources without an ID get a GUID assigned. Customize this:

```csharp
public class CustomFilesourceIdFactory : IFilesourceIdFactory
{
    public string GenerateId() => $"fs-{DateTime.UtcNow:yyyyMMddHHmmss}";
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomFilesourceIdFactory<CustomFilesourceIdFactory>();
});
```

### Custom Permissions

```csharp
public class MyPermissions : IMediaMaticPermissions
{
    public Task<bool> CanAccessFilesourceAsync(IOperationContext context)
    {
        // Custom authorization logic
        return Task.FromResult(true);
    }
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomPermissions<MyPermissions>();
});
```

### Custom Audit Logger

```csharp
public class MyAuditLogger : IMediaMaticAuditLogger
{
    public Task LogAsync(MediaMaticAuditEvent auditEvent)
    {
        // Custom logging logic
        return Task.CompletedTask;
    }
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomAuditLogger<MyAuditLogger>();
});
```

## Dependency Injection

### Injecting Services

```csharp
public class ImageController : ControllerBase
{
    private readonly IMediaMaticService _mediaMaticService;
    private readonly IImageProcessor _imageProcessor;

    public ImageController(
        IMediaMaticService mediaMaticService,
        IImageProcessor imageProcessor)
    {
        _mediaMaticService = mediaMaticService;
        _imageProcessor = imageProcessor;
    }
}
```

### Available Services

| Service | Description |
|---------|-------------|
| `IMediaMaticService` | Primary service for file and media operations |
| `IImageProcessor` | Image processing operations (resize, convert, etc.) |
| `IMediaMaticFilesourceRepository` | Filesource management repository |
| `IVfsConnectionFactory` | Factory for creating VFS connections |
| `IOperationContext` | Current operation context (scoped) |
| `IMediaMaticPermissions` | Permission checking service |
| `IMediaMaticAuditLogger` | Audit logging service |

## REST API Endpoints

MediaMatic provides a comprehensive REST API for file and media management.

### Using UseMediaMatic (Recommended)

```csharp
var app = builder.Build();

// Adds middleware and maps all endpoints
app.UseMediaMatic();
```

### Manual Endpoint Mapping (Advanced)

If you need fine-grained control, map individual endpoint groups:

```csharp
var app = builder.Build();

// Map all MediaMatic endpoints at once
app.MapMediaMaticEndpoints();

// OR map individual endpoint groups for selective API exposure
app.MapMediaMaticFilesourceEndpoints(); // /fs/ filesource CRUD operations
app.MapMediaMaticFileEndpoints();       // /files/ operations
app.MapMediaMaticFolderEndpoints();     // /folders/ operations
app.MapMediaMaticBrowseEndpoints();     // /browse/ operations
app.MapMediaMaticTransformEndpoints();  // /transform/ operations
app.MapMediaMaticMetadataEndpoints();   // /metadata/ operations
app.MapMediaMaticArchiveEndpoints();    // /archive/ operations
```

### Filesource Management (`/fs/`)

Manage filesource configurations via REST API:

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/mm/fs/` | List all filesources |
| `GET` | `/api/mm/fs/{filesourceId}` | Get a specific filesource |
| `GET` | `/api/mm/fs/{filesourceId}/exists` | Check if filesource exists |
| `POST` | `/api/mm/fs/` | Create a new filesource |
| `PUT` | `/api/mm/fs/{filesourceId}` | Update a filesource |
| `PATCH` | `/api/mm/fs/{filesourceId}` | Partially update a filesource |
| `DELETE` | `/api/mm/fs/{filesourceId}` | Delete a filesource |

### File Operations (`/files/`)

File operations use the `/files/{*filePath}` route pattern:

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/mm/fs/{filesourceId}/files/{*filePath}` | Download a file (inline display) |
| `GET` | `/api/mm/fs/{filesourceId}/files/{*filePath}?download=true` | Force download |
| `POST` | `/api/mm/fs/{filesourceId}/files/{*filePath}` | Upload a new file |
| `PUT` | `/api/mm/fs/{filesourceId}/files/{*filePath}` | Overwrite an existing file |
| `DELETE` | `/api/mm/fs/{filesourceId}/files/{*filePath}` | Delete a file |

**Bucket variants** include `/bu/{bucketName}/` in the path:
```
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/files/{*filePath}
```

### Folder Operations (`/folders/`)

Folder operations use the `/folders/{*folderPath}` route pattern:

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/mm/fs/{filesourceId}/folders/{*folderPath}` | Create a folder |
| `DELETE` | `/api/mm/fs/{filesourceId}/folders/{*folderPath}` | Delete a folder recursively |

### Browse Endpoint (`/browse/`)

The unified browse endpoint lists files and/or folders with rich metadata:

```
GET /api/mm/fs/{filesourceId}/browse/{*folderPath?}
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/browse/{*folderPath?}
```

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `type` | string | `all` | `files`, `folders`, or `all` |
| `filter` | string | null | Wildcard filter: `*.pdf`, `*report*.xlsx` |
| `recursive` | bool | false | Include subdirectories |
| `fields` | string | `all` | Comma-separated: `path,size,lastModified,extension,category` |

**Response (default - all fields):**
```json
{
  "folders": [
    { "path": "images/", "name": "images" }
  ],
  "files": [
    {
      "path": "images/photo.jpg",
      "name": "photo.jpg",
      "size": 102400,
      "lastModified": "2025-01-15T10:30:00Z",
      "extension": ".jpg",
      "category": "image"
    }
  ]
}
```

**Minimal response (`?fields=path`):**
```json
{
  "folders": ["images/"],
  "files": ["images/photo.jpg"]
}
```

**File Categories** (based on extension):
- `image`: jpg, jpeg, png, gif, webp, avif, bmp, tiff, svg
- `video`: mp4, webm, mov, avi, mkv
- `audio`: mp3, wav, ogg, flac, aac
- `document`: pdf, doc, docx, xls, xlsx, ppt, pptx
- `archive`: zip, tar, gz, rar, 7z
- `code`: js, ts, cs, py, java, html, css, json, xml
- `other`: everything else

### Transform Endpoints (`/transform/`)

Transform images on-the-fly with URL parameters. See [Transformation URL API](./transformation-url-api.md) for full documentation.

**GET Transform (returns image):**
```
GET /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}
GET /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}?download=true
GET /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}?saveTo=path/to/save.webp
```

**POST Transform (returns metadata, for CMS pre-generation):**
```
POST /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}
```

Request body:
```json
{ "saveTo": "thumbs/photo_400.webp" }
```

Response:
```json
{
  "path": "thumbs/photo_400.webp",
  "size": 12345,
  "width": 400,
  "height": 300,
  "format": "webp",
  "success": true
}
```

**Batch Transform (generate multiple variants):**
```
POST /api/mm/fs/{filesourceId}/transform-batch/
```

Request body:
```json
{
  "source": "images/photo.jpg",
  "variants": [
    { "transformations": "w_400,f_webp", "saveTo": "thumbs/photo_400.webp" },
    { "transformations": "w_800,f_webp", "saveTo": "thumbs/photo_800.webp" },
    { "transformations": "w_200,h_200,c_cover,f_webp", "saveTo": "thumbs/photo_square.webp" }
  ]
}
```

### Archive Endpoints (`/archive/`)

Create, list, download, and delete archives of files and folders.

**Create Folder Archive:**
```
POST /api/mm/fs/{filesourceId}/archive/folders/{*folderPath}
POST /api/mm/fs/{filesourceId}/bu/{bucketName}/archive/folders/{*folderPath}
```

Request body (optional):
```json
{
  "name": "my-archive",
  "compression": "zip"
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `name` | string | timestamp | Archive filename (without extension) |
| `compression` | string | `zip` | Format: `zip`, `tar`, or `tar.gz` |

Response:
```json
{
  "archiveId": "my-archive.zip",
  "archivePath": "__archives/my-archive.zip",
  "fileCount": 15
}
```

**Create File List Archive:**
```
POST /api/mm/fs/{filesourceId}/archive/files/
POST /api/mm/fs/{filesourceId}/bu/{bucketName}/archive/files/
```

Request body:
```json
{
  "name": "selected-files",
  "compression": "tar.gz",
  "paths": [
    "images/photo1.jpg",
    "images/photo2.jpg",
    "documents/"
  ]
}
```

Note: Paths ending with `/` are treated as folders and all their contents are included recursively.

**List Archives:**
```
GET /api/mm/fs/{filesourceId}/archives/folders/{*folderPath}
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/archives/folders/{*folderPath}
```

Response:
```json
{
  "archives": [
    {
      "archiveId": "my-archive.zip",
      "fileName": "my-archive.zip",
      "path": "__archives/my-archive.zip",
      "size": 0,
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

**Download Archive:**
```
GET /api/mm/fs/{filesourceId}/archives/{archiveId}
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/archives/{archiveId}
```

Returns the archive file with appropriate content type (`application/zip`, `application/x-tar`, or `application/gzip`).

**Delete Archive:**
```
DELETE /api/mm/fs/{filesourceId}/archives/{archiveId}
DELETE /api/mm/fs/{filesourceId}/bu/{bucketName}/archives/{archiveId}
```

Returns `204 No Content` on success.

### Metadata Endpoints (`/metadata/`)

Extract metadata from files without downloading them.

```
GET /api/mm/fs/{filesourceId}/metadata/{*filePath}
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/metadata/{*filePath}
```

Response varies by file type:

**Image metadata:**
```json
{
  "mimeType": "image/jpeg",
  "width": 1920,
  "height": 1080,
  "format": "jpeg",
  "colorSpace": "sRGB",
  "hasAlpha": false
}
```

**Video metadata (requires FFmpeg):**
```json
{
  "mimeType": "video/mp4",
  "width": 1920,
  "height": 1080,
  "duration": 120.5,
  "codec": "h264",
  "frameRate": 30
}
```

**Non-media files:**
```json
{
  "mimeType": "application/pdf",
  "size": 102400
}
```

---

## Minimal API Endpoints

### Image Upload with Processing

```csharp
app.MapPost("/api/images", async (
    IFormFile file,
    IImageProcessor processor,
    IMediaMaticService mediaService) =>
{
    using var stream = file.OpenReadStream();

    // Process image
    var processed = await processor.ResizeAsync(stream, width: 1920, height: null);

    // Save to MediaMatic filesource
    var filesourceId = "my-storage";
    var filePath = $"images/{Guid.NewGuid()}.webp";

    // Upload using MediaMatic service
    var uploadResponse = await mediaService.UploadFileAsync(
        filesourceId,
        filePath,
        processed.stream,
        file.FileName
    );

    return Results.Ok(new
    {
        path = filePath,
        width = processed.width,
        height = processed.height,
        size = uploadResponse.SizeInBytes,
    });
})
.DisableAntiforgery();
```

### Image Retrieval

```csharp
app.MapGet("/api/images/{*path}", async (
    string path,
    IMediaMaticService mediaService) =>
{
    var filesourceId = "my-storage";

    var fileStream = await mediaService.DownloadFileAsync(filesourceId, path);

    if (fileStream == null)
    {
        return Results.NotFound();
    }

    // Get MIME type
    var metadata = await mediaService.GetFileMetadataAsync(filesourceId, path);
    var contentType = metadata?.MimeType ?? "application/octet-stream";

    return Results.File(fileStream, contentType);
});
```

## Controller-Based APIs

### Image Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageProcessor _processor;
    private readonly IMediaMaticService _mediaService;

    public ImagesController(IImageProcessor processor, IMediaMaticService mediaService)
    {
        _processor = processor;
        _mediaService = mediaService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        var result = await _processor.ResizeAsync(stream, 1920, null);

        var path = $"images/{Guid.NewGuid()}.webp";
        await _mediaService.UploadFileAsync("my-storage", path, result.stream, file.FileName);

        return Ok(new { path });
    }

    [HttpGet("{*path}")]
    public async Task<IActionResult> Get(string path)
    {
        var fileStream = await _mediaService.DownloadFileAsync("my-storage", path);

        if (fileStream == null)
        {
            return NotFound();
        }

        return File(fileStream, "image/webp");
    }
}
```

## Authorization

MediaMatic provides a flexible permissions system for controlling access to filesources and operations:

```csharp
public class MyPermissions : IMediaMaticPermissions
{
    public Task<bool> CanAccessFilesourceAsync(IOperationContext context)
    {
        // Check if user has access to this filesource
        if (context.FilesourceId == "admin-files" &&
            !context.User?.IsInRole("Admin") == true)
        {
            return Task.FromResult(false);
        }

        // Check if user can perform this operation
        if (context.Operation?.Contains("delete") == true &&
            !context.User?.IsInRole("Editor") == true)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomPermissions<MyPermissions>();
});
```

### Operation Context

The `IOperationContext` provides access to all request details for authorization decisions:

| Property | Description |
|----------|-------------|
| `User` | ClaimsPrincipal for the current request |
| `Operation` | Operation name (e.g., "files/get", "files/post") |
| `FilesourceId` | ID of the filesource being accessed |
| `BucketName` | Bucket name if applicable |
| `FolderPath` | Folder path being accessed |
| `FilePath` | File path being accessed |
| `FileName` | File name being accessed |
| `FileSizeInBytes` | File size for uploads |
| `MimeType` | MIME type of the file |
| `HttpMethod` | HTTP method (GET, POST, etc.) |
| `IpAddress` | Client IP address |
| `HeaderValues` | Request headers (including User-Agent) |
| `RequestId` | Correlation ID for logging |
| `Properties` | Custom properties for additional data |

## Audit Logging

MediaMatic provides an audit logging system for tracking all file operations. This is useful for compliance, debugging, and integrating with CMS or asset management systems.

### Basic Audit Logger

```csharp
public class ConsoleAuditLogger : IMediaMaticAuditLogger
{
    public Task LogAsync(MediaMaticAuditEvent auditEvent)
    {
        Console.WriteLine($"[{auditEvent.Timestamp:s}] " +
            $"{auditEvent.UserIdentifier} " +
            $"{auditEvent.Operation} " +
            $"{auditEvent.FilePath ?? auditEvent.FolderPath} " +
            $"({(auditEvent.Success ? "OK" : "FAILED")})");
        return Task.CompletedTask;
    }
}

builder.Services.AddMediaMatic(config =>
{
    config.UseCustomAuditLogger<ConsoleAuditLogger>();
});
```

### Database Audit Logger (CMS Integration)

For CMS or asset management integration, store audit events in your database:

```csharp
public class DatabaseAuditLogger : IMediaMaticAuditLogger
{
    private readonly IDbConnection _db;

    public DatabaseAuditLogger(IDbConnection db)
    {
        _db = db;
    }

    public async Task LogAsync(MediaMaticAuditEvent e)
    {
        await _db.ExecuteAsync(@"
            INSERT INTO MediaAuditLog
                (UserIdentifier, Operation, FilesourceId, BucketName,
                 FolderPath, FilePath, FileName, FileSizeInBytes, MimeType,
                 Success, Message, IpAddress, UserAgent, RequestId, Timestamp)
            VALUES
                (@UserIdentifier, @Operation, @FilesourceId, @BucketName,
                 @FolderPath, @FilePath, @FileName, @FileSizeInBytes, @MimeType,
                 @Success, @Message, @IpAddress, @UserAgent, @RequestId, @Timestamp)",
            e);
    }
}
```

### Audit Event Properties

The `MediaMaticAuditEvent` includes:

| Property | Description |
|----------|-------------|
| `UserIdentifier` | User name or ID from claims |
| `Operation` | Operation performed (e.g., "files/post", "transform/get") |
| `FilesourceId` | ID of the filesource |
| `BucketName` | Bucket name if applicable |
| `FolderName` | Folder name |
| `FolderPath` | Full folder path |
| `OriginalFileName` | Original upload filename |
| `FileName` | Stored filename |
| `FilePath` | Full file path |
| `FileSizeInBytes` | File size |
| `MimeType` | MIME type (e.g., "image/jpeg") |
| `Success` | Whether the operation succeeded |
| `Message` | Success/error message |
| `Timestamp` | When the operation occurred |
| `RequestId` | Correlation ID |
| `IpAddress` | Client IP address |
| `UserAgent` | Client User-Agent header |
| `Properties` | Custom properties dictionary |

### CMS Integration Pattern

Use audit logging to maintain file metadata in your CMS database without creating a hard dependency:

```csharp
public class CmsFileTracker : IMediaMaticAuditLogger
{
    private readonly ICmsDatabase _cms;

    public CmsFileTracker(ICmsDatabase cms)
    {
        _cms = cms;
    }

    public async Task LogAsync(MediaMaticAuditEvent e)
    {
        if (!e.Success) return;

        switch (e.Operation)
        {
            case "files/post":
                // Track new file upload
                await _cms.Files.InsertAsync(new CmsFile
                {
                    Path = e.FilePath,
                    FileName = e.FileName,
                    OriginalName = e.OriginalFileName,
                    MimeType = e.MimeType,
                    SizeInBytes = e.FileSizeInBytes ?? 0,
                    UploadedBy = e.UserIdentifier,
                    UploadedAt = e.Timestamp,
                    FilesourceId = e.FilesourceId,
                });
                break;

            case "files/delete":
                // Remove file from tracking
                await _cms.Files.DeleteAsync(e.FilePath);
                break;

            case "files/get":
            case "transform/get":
                // Track download/view
                await _cms.FileViews.InsertAsync(new CmsFileView
                {
                    FilePath = e.FilePath,
                    ViewedBy = e.UserIdentifier,
                    ViewedAt = e.Timestamp,
                    IpAddress = e.IpAddress,
                    UserAgent = e.UserAgent,
                });
                break;
        }
    }
}
```

This pattern allows:
- **File tracking** without coupling MediaMatic to your database schema
- **View analytics** for tracking downloads and transformations
- **User activity** logging for compliance
- **Browser/device tracking** via User-Agent
- **Flexible integration** with any CMS or DAM system

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<MediaMaticHealthCheck>("mediamatic");

public class MediaMaticHealthCheck : IHealthCheck
{
    private readonly IMediaMaticFilesourceRepository _repository;

    public MediaMaticHealthCheck(IMediaMaticFilesourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filesources = await _repository.GetAllFilesourcesAsync();
            return HealthCheckResult.Healthy($"Found {filesources.Count} filesource(s)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```

## Error Handling

```csharp
app.UseExceptionHandler(error =>
{
    error.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var response = exception switch
        {
            FileNotFoundException => new { error = "File not found" },
            NotSupportedException => new { error = "Format not supported" },
            _ => new { error = "An error occurred" },
        };

        context.Response.StatusCode = exception switch
        {
            FileNotFoundException => 404,
            NotSupportedException => 400,
            _ => 500,
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});
```

## Next Steps

- [Storage Providers](storage-providers.md) - Provider configuration details
- [Transformation URL API](transformation-url-api.md) - Image transformation syntax
- [Testing](testing.md) - Integration testing
