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

// Map MediaMatic endpoints (optional)
app.MapMediaMaticEndpoints();

app.Run();
```

### Configuration Options

```csharp
builder.Services.AddMediaMatic(options =>
{
    // Default storage provider
    options.DefaultStorageProvider = "aws";

    // Default image processing options
    options.DefaultImageOptions = new ImageProcessingOptions
    {
        Quality = 85,
        Format = ImageFormat.WebP,
    };

    // Default thumbnail sizes
    options.DefaultThumbnailSizes = new[] { 100, 300, 600, 1200 };

    // Enable browser detection for format negotiation
    options.EnableBrowserDetection = true;
});
```

## Storage Providers

### In-Memory (Development)

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.UseInMemoryStorage();
});
```

### Local File System

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.UseLocalStorage("/var/media");
});
```

### AWS S3

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.UseAwsS3(s3 =>
    {
        s3.AccessKeyId = builder.Configuration["AWS:AccessKeyId"];
        s3.SecretAccessKey = builder.Configuration["AWS:SecretAccessKey"];
        s3.Region = "us-east-1";
        s3.BucketName = "my-media-bucket";
    });
});
```

### Google Cloud Storage

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.UseGoogleCloudStorage(gcs =>
    {
        gcs.ProjectId = builder.Configuration["GCP:ProjectId"];
        gcs.BucketName = "my-media-bucket";
        gcs.JsonKeyPath = builder.Configuration["GCP:JsonKeyPath"];
    });
});
```

### Multiple Providers

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.AddStorageProvider("primary", storage =>
    {
        storage.UseAwsS3(s3 => { /* ... */ });
    });

    options.AddStorageProvider("backup", storage =>
    {
        storage.UseGoogleCloudStorage(gcs => { /* ... */ });
    });

    options.DefaultStorageProvider = "primary";
});
```

## Dependency Injection

### Injecting Services

```csharp
public class ImageController : ControllerBase
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IVideoProcessor _videoProcessor;
    private readonly IBlobStorage _storage;

    public ImageController(
        IImageProcessor imageProcessor,
        IVideoProcessor videoProcessor,
        IBlobStorage storage)
    {
        _imageProcessor = imageProcessor;
        _videoProcessor = videoProcessor;
        _storage = storage;
    }
}
```

### Available Services

| Service | Description |
|---------|-------------|
| `IImageProcessor` | Image processing operations |
| `IVideoProcessor` | Video processing operations |
| `IBlobStorage` | Storage operations |
| `IMimeTypeDetector` | MIME type detection |
| `IImageMetadataExtractor` | Image metadata extraction |
| `IVideoMetadataExtractor` | Video metadata extraction |

## REST API Endpoints

MediaMatic provides a comprehensive REST API for file and media management. Map the endpoints in your startup:

```csharp
var app = builder.Build();

// Map all MediaMatic endpoints
app.MapMediaMaticEndpoints();

// Or map individual endpoint groups
app.MapMediaMaticFileEndpoints();      // /files/ operations
app.MapMediaMaticFolderEndpoints();    // /folders/ operations
app.MapMediaMaticBrowseEndpoints();    // /browse/ operations
app.MapMediaMaticTransformEndpoints(); // /transform/ operations
app.MapMediaMaticStatsEndpoints();     // /stats/ operations
app.MapMediaMaticArchiveEndpoints();   // /archive/ operations
```

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

---

## Minimal API Endpoints

### Image Upload

```csharp
app.MapPost("/api/images", async (
    IFormFile file,
    IImageProcessor processor,
    IBlobStorage storage) =>
{
    using var stream = file.OpenReadStream();

    // Process image
    var result = await processor.ResizeAsync(stream, width: 1920, height: null,
        new ImageProcessingOptions
        {
            Format = ImageFormat.WebP,
            Quality = 85,
        });

    // Save to storage
    var path = $"images/{Guid.NewGuid()}.webp";
    await storage.WriteAsync(path, result.stream);

    return Results.Ok(new
    {
        path,
        width = result.width,
        height = result.height,
        size = result.fileSize,
    });
})
.DisableAntiforgery();
```

### Image Retrieval with Format Negotiation

```csharp
app.MapGet("/api/images/{*path}", async (
    string path,
    HttpContext context,
    IBlobStorage storage) =>
{
    // Determine optimal format
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    var accept = context.Request.Headers["Accept"].ToString();
    var format = BrowserFormatSelector.SelectOptimalFormat(userAgent, accept);

    // Get image with correct format
    var imagePath = Path.ChangeExtension(path, format.ToString().ToLower());

    if (!await storage.ExistsAsync(imagePath))
    {
        return Results.NotFound();
    }

    using var stream = await storage.OpenReadAsync(imagePath);

    // Set caching headers
    context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
    context.Response.Headers["Vary"] = "Accept";

    return Results.File(stream, GetContentType(format));
});
```

### Video Thumbnail Generation

```csharp
app.MapPost("/api/videos/thumbnails", async (
    IFormFile file,
    IVideoProcessor processor,
    IBlobStorage storage) =>
{
    // Save video temporarily
    var tempPath = Path.GetTempFileName();
    using (var stream = File.Create(tempPath))
    {
        await file.CopyToAsync(stream);
    }

    try
    {
        // Generate thumbnails
        var options = new ThumbnailOptions
        {
            Count = 5,
            Width = 320,
            Quality = 85,
            OutputPath = Path.GetTempPath(),
        };

        var thumbnails = await processor.GenerateThumbnailsAsync(tempPath, options);

        // Upload thumbnails to storage
        var uploadedPaths = new List<string>();

        foreach (var thumb in thumbnails)
        {
            var path = $"thumbnails/{Guid.NewGuid()}.jpg";
            using var thumbStream = File.OpenRead(thumb);
            await storage.WriteAsync(path, thumbStream);
            uploadedPaths.Add(path);
        }

        return Results.Ok(new { thumbnails = uploadedPaths });
    }
    finally
    {
        File.Delete(tempPath);
    }
})
.DisableAntiforgery();
```

## Controller-Based APIs

### Image Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageProcessor _processor;
    private readonly IBlobStorage _storage;

    public ImagesController(IImageProcessor processor, IBlobStorage storage)
    {
        _processor = processor;
        _storage = storage;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        var result = await _processor.ResizeAsync(stream, 1920, null);

        var path = $"images/{Guid.NewGuid()}.webp";
        await _storage.WriteAsync(path, result.stream);

        return Ok(new { path });
    }

    [HttpGet("{*path}")]
    public async Task<IActionResult> Get(string path)
    {
        if (!await _storage.ExistsAsync(path))
        {
            return NotFound();
        }

        var stream = await _storage.OpenReadAsync(path);
        return File(stream, "image/webp");
    }
}
```

## Middleware

### Image Optimization Middleware

```csharp
public class ImageOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IImageProcessor _processor;

    public ImageOptimizationMiddleware(
        RequestDelegate next,
        IImageProcessor processor)
    {
        _next = next;
        _processor = processor;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsImageRequest(context.Request))
        {
            // Check for resize parameters
            if (context.Request.Query.TryGetValue("w", out var widthStr) &&
                int.TryParse(widthStr, out var width))
            {
                // Process image on-the-fly
                // (In production, cache the result)
            }
        }

        await _next(context);
    }

    private bool IsImageRequest(HttpRequest request)
    {
        return request.Path.Value?.EndsWith(".jpg") == true ||
               request.Path.Value?.EndsWith(".png") == true ||
               request.Path.Value?.EndsWith(".webp") == true;
    }
}
```

## Configuration from appsettings.json

```json
{
  "MediaMatic": {
    "Storage": {
      "Provider": "aws",
      "Aws": {
        "Region": "us-east-1",
        "BucketName": "my-bucket"
      }
    },
    "ImageProcessing": {
      "DefaultQuality": 85,
      "DefaultFormat": "WebP",
      "ThumbnailSizes": [100, 300, 600, 1200]
    },
    "VideoProcessing": {
      "ThumbnailCount": 5,
      "DefaultWidth": 320
    }
  }
}
```

```csharp
builder.Services.Configure<MediaMaticOptions>(
    builder.Configuration.GetSection("MediaMatic")
);

builder.Services.AddMediaMatic();
```

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("storage");

public class StorageHealthCheck : IHealthCheck
{
    private readonly IBlobStorage _storage;

    public StorageHealthCheck(IBlobStorage storage)
    {
        _storage = storage;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _storage.ListAsync();
            return HealthCheckResult.Healthy();
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

## Authorization

MediaMatic provides an authorization hook for controlling access to filesources and operations:

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.Authorization = async (context) =>
    {
        // Check if user has access to this filesource
        if (context.FilesourceId == "admin-files" &&
            !context.User?.IsInRole("Admin") == true)
        {
            return false;
        }

        // Check if user can perform this operation
        if (context.Operation?.StartsWith("files/delete") == true &&
            !context.User?.IsInRole("Editor") == true)
        {
            return false;
        }

        return true;
    };
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

builder.Services.AddSingleton<IMediaMaticAuditLogger, ConsoleAuditLogger>();
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

## Next Steps

- [Browser Detection](browser-detection.md) - Format negotiation details
- [Storage Providers](storage-providers.md) - Provider configuration
- [Testing](testing.md) - Integration testing
