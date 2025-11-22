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

## Next Steps

- [Browser Detection](browser-detection.md) - Format negotiation details
- [Storage Providers](storage-providers.md) - Provider configuration
- [Testing](testing.md) - Integration testing
