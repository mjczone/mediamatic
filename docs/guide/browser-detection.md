# Browser Detection

MediaMatic uses [DeviceDetector.NET](https://github.com/totpero/DeviceDetector.NET) for browser and device detection, enabling intelligent format negotiation to serve optimal media formats.

## Why Browser Detection?

Modern image formats like WebP and AVIF offer better compression but aren't supported by all browsers:

| Format | Chrome | Firefox | Safari | Edge | IE 11 |
|--------|--------|---------|--------|------|-------|
| JPEG | ✅ | ✅ | ✅ | ✅ | ✅ |
| PNG | ✅ | ✅ | ✅ | ✅ | ✅ |
| WebP | ✅ | ✅ | ✅ (14+) | ✅ | ❌ |
| AVIF | ✅ | ✅ | ✅ (16+) | ✅ | ❌ |

By detecting the browser, you can serve the best supported format.

## Format Negotiation

### Basic Usage

```csharp
using MJCZone.MediaMatic.AspNetCore;

// In a controller or endpoint
var optimalFormat = BrowserFormatSelector.SelectOptimalFormat(
    userAgent: context.Request.Headers["User-Agent"].ToString(),
    accept: context.Request.Headers["Accept"].ToString()
);

Console.WriteLine($"Optimal format: {optimalFormat}"); // WebP, Avif, Jpeg, etc.
```

### How It Works

1. **Parse Accept Header** - Check for `image/avif`, `image/webp`
2. **Detect Browser** - Identify browser and version
3. **Select Best Format** - Return best supported format

```csharp
public static ImageFormat SelectOptimalFormat(string userAgent, string accept)
{
    // Check Accept header first
    if (accept.Contains("image/avif"))
    {
        var device = DeviceDetector.Parse(userAgent);
        if (SupportsAvif(device))
            return ImageFormat.Avif;
    }

    if (accept.Contains("image/webp"))
        return ImageFormat.WebP;

    return ImageFormat.Jpeg;
}
```

## Device Detection

### Detecting Device Type

```csharp
using DeviceDetectorNET;

var detector = new DeviceDetector(userAgent);
detector.Parse();

// Device type
var deviceType = detector.GetDeviceName(); // "smartphone", "desktop", etc.

// Browser info
var browser = detector.GetBrowserClient();
Console.WriteLine($"Browser: {browser.Name} {browser.Version}");

// OS info
var os = detector.GetOs();
Console.WriteLine($"OS: {os.Name} {os.Version}");
```

### Device Types

| Type | Examples |
|------|----------|
| `desktop` | Windows PC, Mac |
| `smartphone` | iPhone, Android phone |
| `tablet` | iPad, Android tablet |
| `tv` | Smart TV, Apple TV |
| `console` | PlayStation, Xbox |
| `car` | Tesla browser |
| `bot` | Googlebot, Bingbot |

## ASP.NET Core Middleware

### Image Format Middleware

Automatically serve optimal image format:

```csharp
public class ImageFormatMiddleware
{
    private readonly RequestDelegate _next;

    public ImageFormatMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only for image requests
        if (context.Request.Path.StartsWithSegments("/images"))
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var accept = context.Request.Headers["Accept"].ToString();

            var format = BrowserFormatSelector.SelectOptimalFormat(userAgent, accept);

            // Set format in request items for downstream use
            context.Items["OptimalImageFormat"] = format;
        }

        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<ImageFormatMiddleware>();
```

### Usage in Endpoints

```csharp
app.MapGet("/images/{*path}", async (
    string path,
    HttpContext context,
    IBlobStorage storage) =>
{
    var format = context.Items["OptimalImageFormat"] as ImageFormat?
        ?? ImageFormat.Jpeg;

    // Get path with correct extension
    var optimizedPath = GetOptimizedPath(path, format);

    using var stream = await storage.OpenReadAsync(optimizedPath);
    return Results.File(stream, GetContentType(format));
});
```

## Responsive Image Serving

### Device-Based Sizing

Serve different image sizes based on device:

```csharp
public static int GetOptimalWidth(string userAgent)
{
    var detector = new DeviceDetector(userAgent);
    detector.Parse();

    return detector.GetDeviceName() switch
    {
        "smartphone" => 640,
        "tablet" => 1024,
        "desktop" => 1920,
        _ => 1280,
    };
}
```

### Picture Element Support

Generate `<picture>` element with multiple sources:

```csharp
public string GeneratePictureElement(string imagePath, string alt)
{
    return $@"
        <picture>
            <source srcset=""{imagePath}.avif"" type=""image/avif"">
            <source srcset=""{imagePath}.webp"" type=""image/webp"">
            <img src=""{imagePath}.jpg"" alt=""{alt}"">
        </picture>
    ";
}
```

### Srcset Generation

```csharp
public string GenerateSrcset(string basePath)
{
    var sizes = new[] { 320, 640, 960, 1280, 1920 };

    var srcset = sizes
        .Select(w => $"{basePath}-{w}w.webp {w}w")
        .ToList();

    return string.Join(", ", srcset);
}

// Output: "image-320w.webp 320w, image-640w.webp 640w, ..."
```

## Common Patterns

### API Endpoint

```csharp
app.MapGet("/api/images/{id}", async (
    int id,
    HttpContext context,
    IImageService imageService) =>
{
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    var accept = context.Request.Headers["Accept"].ToString();

    var format = BrowserFormatSelector.SelectOptimalFormat(userAgent, accept);
    var width = BrowserFormatSelector.GetOptimalWidth(userAgent);

    var image = await imageService.GetOptimizedAsync(id, format, width);

    return Results.File(
        image.Stream,
        image.ContentType,
        enableRangeProcessing: true
    );
});
```

### Content Negotiation Header

Set Vary header for proper caching:

```csharp
app.MapGet("/images/{*path}", async (HttpContext context) =>
{
    // Important: Tell CDN/proxy to cache by Accept header
    context.Response.Headers["Vary"] = "Accept";

    // ... serve image
});
```

### Cache-Control

```csharp
context.Response.Headers["Cache-Control"] = "public, max-age=31536000"; // 1 year
context.Response.Headers["Vary"] = "Accept";
```

## Bot Detection

Handle search engine bots specially:

```csharp
public static bool IsBot(string userAgent)
{
    var detector = new DeviceDetector(userAgent);
    detector.Parse();

    return detector.IsBot();
}

// In endpoint
if (BrowserFormatSelector.IsBot(userAgent))
{
    // Serve original JPEG for SEO
    return Results.File(jpegStream, "image/jpeg");
}
```

### Common Bots

| Bot | User Agent Contains |
|-----|---------------------|
| Googlebot | `Googlebot` |
| Bingbot | `bingbot` |
| Facebook | `facebookexternalhit` |
| Twitter | `Twitterbot` |
| LinkedIn | `LinkedInBot` |

## Performance Tips

### Caching Device Detection

```csharp
services.AddMemoryCache();

public class CachedDeviceDetector
{
    private readonly IMemoryCache _cache;

    public DeviceInfo Detect(string userAgent)
    {
        return _cache.GetOrCreate(userAgent, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            var detector = new DeviceDetector(userAgent);
            detector.Parse();

            return new DeviceInfo
            {
                DeviceType = detector.GetDeviceName(),
                BrowserName = detector.GetBrowserClient()?.Name,
                BrowserVersion = detector.GetBrowserClient()?.Version,
            };
        });
    }
}
```

### Pre-generation

Pre-generate all format variants during upload:

```csharp
// During upload
var formats = new[] { ImageFormat.Jpeg, ImageFormat.WebP, ImageFormat.Avif };

foreach (var format in formats)
{
    var result = await processor.ConvertFormatAsync(stream, format);
    await storage.WriteAsync($"{basePath}.{format.Extension}", result.Stream);
}

// During serving - just select the right file
var path = $"{basePath}.{optimalFormat.Extension}";
```

## Testing

### Mock User Agents

```csharp
// Chrome (WebP + AVIF)
var chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0";

// Safari (WebP only in newer versions)
var safari = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Safari/605.1.15";

// IE 11 (JPEG only)
var ie11 = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
```

## Next Steps

- [ASP.NET Core Integration](aspnetcore-integration.md) - Full web integration
- [Image Processing](image-processing.md) - Generate format variants
- [Configuration](configuration.md) - Configure default formats
