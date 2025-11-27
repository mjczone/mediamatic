# Metadata Extraction

MediaMatic provides  metadata extraction using three specialized libraries:

| Library | Purpose | Formats |
|---------|---------|---------|
| [MimeDetective](https://github.com/MediatedCommunications/Mime-Detective) | MIME type detection | All files |
| [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) | EXIF/image metadata | JPEG, PNG, GIF, BMP, TIFF, WebP |
| [TagLibSharp](https://github.com/mono/taglib-sharp) | Audio/video tags | MP3, MP4, MKV, AVI, etc. |

## MIME Type Detection

### Content-Based Detection

MimeDetective analyzes file content (magic bytes), not just extensions:

```csharp
using MJCZone.MediaMatic.Metadata;

var detector = new MimeTypeDetector();

// Detect from stream
using var stream = File.OpenRead("file.dat");
var mimeType = detector.Detect(stream);

Console.WriteLine($"MIME Type: {mimeType}"); // e.g., "image/jpeg"
```

### Why Content-Based?

| Approach | Detects | Misses |
|----------|---------|--------|
| Extension-only | file.jpg → image/jpeg | renamed_virus.jpg.exe |
| **Content-based** | Actual file content | Nothing (14K+ signatures) |

### Common MIME Types

```csharp
// Images
var jpeg = detector.Detect(jpegStream);  // "image/jpeg"
var png = detector.Detect(pngStream);    // "image/png"
var webp = detector.Detect(webpStream);  // "image/webp"

// Videos
var mp4 = detector.Detect(mp4Stream);    // "video/mp4"
var webm = detector.Detect(webmStream);  // "video/webm"

// Audio
var mp3 = detector.Detect(mp3Stream);    // "audio/mpeg"
var flac = detector.Detect(flacStream);  // "audio/flac"
```

## Image Metadata (EXIF)

### Extracting EXIF Data

```csharp
using MJCZone.MediaMatic.Metadata;

var extractor = new ImageMetadataExtractor();

using var stream = File.OpenRead("photo.jpg");
var metadata = extractor.Extract(stream);

// Basic properties
Console.WriteLine($"Width: {metadata.Width}");
Console.WriteLine($"Height: {metadata.Height}");
Console.WriteLine($"Format: {metadata.Format}");

// EXIF data
if (metadata.ExifData != null)
{
    Console.WriteLine($"Camera: {metadata.ExifData.CameraMake} {metadata.ExifData.CameraModel}");
    Console.WriteLine($"Date Taken: {metadata.ExifData.DateTaken}");
    Console.WriteLine($"ISO: {metadata.ExifData.IsoSpeed}");
    Console.WriteLine($"Aperture: f/{metadata.ExifData.Aperture}");
    Console.WriteLine($"Shutter: {metadata.ExifData.ShutterSpeed}");
    Console.WriteLine($"Focal Length: {metadata.ExifData.FocalLength}mm");
}

// GPS location
if (metadata.GpsData != null)
{
    Console.WriteLine($"Location: {metadata.GpsData.Latitude}, {metadata.GpsData.Longitude}");
    Console.WriteLine($"Altitude: {metadata.GpsData.Altitude}m");
}
```

### Available EXIF Properties

| Category | Properties |
|----------|------------|
| **Camera** | Make, Model, Lens, Serial |
| **Settings** | ISO, Aperture, Shutter Speed, Focal Length |
| **Date/Time** | Date Taken, Date Digitized, Date Modified |
| **Image** | Width, Height, Orientation, Color Space |
| **GPS** | Latitude, Longitude, Altitude, Direction |
| **Software** | Software, Copyright, Artist |

### Handling Orientation

EXIF orientation tag indicates how to rotate the image:

```csharp
var orientation = metadata.ExifData?.Orientation;

switch (orientation)
{
    case 1: // Normal
        break;
    case 3: // Rotated 180°
        break;
    case 6: // Rotated 90° CW
        break;
    case 8: // Rotated 90° CCW
        break;
}
```

## Video Metadata

### Extracting Video Info

```csharp
using MJCZone.MediaMatic.Metadata;

var extractor = new VideoMetadataExtractor();

var metadata = await extractor.ExtractAsync("video.mp4");

// Video properties
Console.WriteLine($"Duration: {metadata.Duration}");
Console.WriteLine($"Width: {metadata.Width}");
Console.WriteLine($"Height: {metadata.Height}");
Console.WriteLine($"Frame Rate: {metadata.FrameRate} fps");
Console.WriteLine($"Bitrate: {metadata.Bitrate} kbps");
Console.WriteLine($"Codec: {metadata.VideoCodec}");

// Audio properties
Console.WriteLine($"Audio Codec: {metadata.AudioCodec}");
Console.WriteLine($"Audio Channels: {metadata.AudioChannels}");
Console.WriteLine($"Sample Rate: {metadata.SampleRate} Hz");

// File info
Console.WriteLine($"Format: {metadata.Format}");
Console.WriteLine($"Size: {metadata.FileSize} bytes");
```

### Video Metadata Properties

| Property | Description |
|----------|-------------|
| `Duration` | Total duration (TimeSpan) |
| `Width` | Video width in pixels |
| `Height` | Video height in pixels |
| `FrameRate` | Frames per second |
| `Bitrate` | Total bitrate in kbps |
| `VideoCodec` | Video codec (H.264, H.265, etc.) |
| `AudioCodec` | Audio codec (AAC, MP3, etc.) |
| `AudioChannels` | Number of audio channels |
| `SampleRate` | Audio sample rate in Hz |
| `Format` | Container format (MP4, WebM, etc.) |

## Audio Metadata

### Extracting Audio Tags

```csharp
using MJCZone.MediaMatic.Metadata;

var extractor = new AudioMetadataExtractor();

var metadata = extractor.Extract("song.mp3");

// Basic tags
Console.WriteLine($"Title: {metadata.Title}");
Console.WriteLine($"Artist: {metadata.Artist}");
Console.WriteLine($"Album: {metadata.Album}");
Console.WriteLine($"Year: {metadata.Year}");
Console.WriteLine($"Genre: {metadata.Genre}");
Console.WriteLine($"Track: {metadata.TrackNumber}");

// Audio properties
Console.WriteLine($"Duration: {metadata.Duration}");
Console.WriteLine($"Bitrate: {metadata.Bitrate} kbps");
Console.WriteLine($"Sample Rate: {metadata.SampleRate} Hz");
Console.WriteLine($"Channels: {metadata.Channels}");

// Album art
if (metadata.AlbumArt != null)
{
    File.WriteAllBytes("cover.jpg", metadata.AlbumArt);
}
```

### Audio Tag Properties

| Property | Description |
|----------|-------------|
| `Title` | Song title |
| `Artist` | Artist name |
| `Album` | Album name |
| `Year` | Release year |
| `Genre` | Genre |
| `TrackNumber` | Track number |
| `AlbumArt` | Embedded cover image |
| `Duration` | Song length |
| `Bitrate` | Audio bitrate |
| `SampleRate` | Sample rate in Hz |

## Privacy Considerations

### Stripping Metadata

Remove sensitive metadata before sharing:

```csharp
var options = new ImageProcessingOptions
{
    StripMetadata = true, // Remove all EXIF data
};

var result = await processor.ResizeAsync(stream, 800, null, options);
```

### Sensitive EXIF Data

Be aware of these privacy-sensitive fields:

| Field | Risk |
|-------|------|
| GPS coordinates | Location tracking |
| Date/time | Schedule tracking |
| Camera serial | Device identification |
| Software | System fingerprinting |

### Selective Stripping

Preserve some metadata while removing sensitive data:

```csharp
var options = new ImageProcessingOptions
{
    PreserveMetadata = new[]
    {
        MetadataType.ColorProfile,
        MetadataType.Orientation,
    },
    StripMetadata = new[]
    {
        MetadataType.Gps,
        MetadataType.Software,
        MetadataType.DateTime,
    },
};
```

## Common Use Cases

### File Validation

Validate uploaded files by content:

```csharp
var detector = new MimeTypeDetector();
var mimeType = detector.Detect(uploadStream);

var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };

if (!allowedTypes.Contains(mimeType))
{
    throw new InvalidOperationException($"File type {mimeType} not allowed");
}
```

### Photo Gallery

Extract and display photo information:

```csharp
var metadata = extractor.Extract(photoStream);

var photoInfo = new
{
    Dimensions = $"{metadata.Width}x{metadata.Height}",
    Camera = metadata.ExifData?.CameraModel,
    DateTaken = metadata.ExifData?.DateTaken,
    Location = metadata.GpsData != null
        ? $"{metadata.GpsData.Latitude}, {metadata.GpsData.Longitude}"
        : null,
};
```

### Video Streaming Platform

Generate video info cards:

```csharp
var metadata = await extractor.ExtractAsync(videoPath);

var videoCard = new
{
    Duration = FormatDuration(metadata.Duration),
    Quality = GetQualityLabel(metadata.Height), // "1080p", "720p", etc.
    FileSize = FormatFileSize(metadata.FileSize),
};
```

### Music Library

Build music database:

```csharp
var metadata = extractor.Extract(songPath);

var song = new Song
{
    Title = metadata.Title ?? Path.GetFileNameWithoutExtension(songPath),
    Artist = metadata.Artist ?? "Unknown Artist",
    Album = metadata.Album ?? "Unknown Album",
    Duration = metadata.Duration,
    Year = metadata.Year,
};
```

## Error Handling

```csharp
try
{
    var metadata = extractor.Extract(stream);
}
catch (UnknownFileTypeException)
{
    // File format not recognized
}
catch (CorruptedFileException)
{
    // File is damaged
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to extract metadata");
}
```

## Next Steps

- [Image Processing](image-processing.md) - Process images with metadata
- [Video Processing](video-processing.md) - Process videos
- [Browser Detection](browser-detection.md) - Format negotiation
