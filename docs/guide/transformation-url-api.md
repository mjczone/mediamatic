# Transformation URL API

MediaMatic provides a powerful URL-based image transformation API that allows you to resize, crop, convert formats, and optimize images on-the-fly using URL parameters. This API is designed to compete with services like Cloudinary while giving you complete control over your media pipeline.

## Overview

Instead of pre-generating multiple image sizes and formats, you can request transformed images directly via URL parameters. MediaMatic will process the image on-demand and cache the result for subsequent requests.

**Key Benefits:**
- 🚀 **On-Demand Processing** - Generate image variants as needed
- 💰 **Cost Effective** - No vendor lock-in, run on your infrastructure
- ⚡ **Fast** - Intelligent caching with CDN support
- 🌐 **Browser-Aware** - Automatically serve optimal formats (AVIF, WebP, JPEG)
- 🎨 **Flexible** - Extensive transformation options
- 📱 **Responsive** - Device pixel ratio support for retina displays

---

## Quick Start

### Basic Usage

Transform an image using the `/transform` endpoint:

```
Original:
GET /api/mm/fs/default/files/products/shoe.jpg

Transformed (400x300, WebP format):
GET /api/mm/fs/default/transform/w_400,h_300,f_webp/products/shoe.jpg
```

### Common Examples

```bash
# Thumbnail (200x200, cover crop)
GET /transform/w_200,h_200,c_cover,q_auto,f_auto/products/shoe.jpg

# Responsive image (16:9, fit to width)
GET /transform/w_1920,ar_16:9,c_fit,q_85,f_webp/hero.jpg

# Retina display (2x DPR)
GET /transform/w_400,h_400,dpr_2,f_png/logo.png

# Browser-optimized (auto format selection)
GET /transform/w_800,c_fit,q_auto,f_auto/gallery/photo.jpg
```

---

## URL Structure

### Endpoint Format

```
GET /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}
```

**Components:**
- `{filesourceId}` - Your filesource identifier (e.g., "default", "s3", "cdn")
- `{transformations}` - Comma-separated transformation parameters (e.g., "w_400,h_300,c_fill")
- `{*filePath}` - Path to the original file (e.g., "products/shoe.jpg")

**Why transformations before path?** This prevents conflicts with user folder names (like "t", "meta", "transform") and keeps the catch-all parameter at the end of the URL where ASP.NET Core routing requires it.

### With Buckets (S3/Azure)

For storage providers that support buckets:

```
GET /api/mm/fs/{filesourceId}/bu/{bucketName}/transform/{transformations}/{*filePath}
```

**Example:**
```
GET /api/mm/fs/s3-prod/bu/media-assets/transform/w_1920,h_1080,c_fill,f_webp/images/hero.jpg
```

---

## Transformation Parameters

### Dimensions

#### Width (`w_`)
Set the target width in pixels.

```
w_400           - Width of 400 pixels
w_1920          - Width of 1920 pixels
```

#### Height (`h_`)
Set the target height in pixels.

```
h_300           - Height of 300 pixels
h_1080          - Height of 1080 pixels
```

#### Aspect Ratio (`ar_`)
Set the aspect ratio (width:height or decimal).

```
ar_16:9         - 16:9 aspect ratio (widescreen)
ar_4:3          - 4:3 aspect ratio (standard)
ar_1:1          - 1:1 aspect ratio (square)
ar_1.5          - 1.5 aspect ratio (decimal notation)
```

**Note:** When using `ar_` with only `w_` or `h_`, the missing dimension is calculated automatically.

**Examples:**
```bash
# Width 1920, height calculated for 16:9
GET /transform/w_1920,ar_16:9/hero.jpg

# Height 1080, width calculated for 16:9
GET /transform/h_1080,ar_16:9/hero.jpg

# Square thumbnail, width 300
GET /transform/w_300,ar_1:1/thumbnail.jpg
```

#### Device Pixel Ratio (`dpr_`)
Multiply dimensions for high-density displays (retina).

```
dpr_1           - Standard density [default]
dpr_2           - 2x density (retina displays)
dpr_3           - 3x density (some mobile devices)
```

**Example:**
```bash
# Request 400x300, but receive 800x600 for retina
GET /transform/w_400,h_300,dpr_2,f_png/logo.png
```

---

### Crop Modes (`c_`)

Control how the image is resized when aspect ratio doesn't match.

```
c_fit           - Fit within bounds (letterbox/pillarbox) [default]
c_fill          - Fill bounds (crop excess) - alias for cover
c_cover         - Cover entire area (crop excess, no empty space)
c_pad           - Pad to exact dimensions (add background)
c_stretch       - Stretch to exact dimensions (ignore aspect ratio)
c_scale         - Alias for fit
c_thumb         - Thumbnail mode (optimized for small sizes)
```

#### `c_fit` (Letterbox/Pillarbox)
Resize to fit within bounds while preserving aspect ratio. May result in empty space.

```bash
# Landscape image to portrait bounds
GET /transform/w_400,h_600,c_fit/landscape.jpg
# Result: Image is 400x267, centered vertically with empty space
```

#### `c_fill` / `c_cover` (Crop to Fill)
Resize to completely fill bounds, cropping excess content.

```bash
# Portrait image to square
GET /transform/w_400,h_400,c_fill/portrait.jpg
# Result: Image is 400x400, cropped from center
```

#### `c_pad` (Add Background)
Resize to fit, then pad to exact dimensions with background color.

```bash
# Add white padding to landscape image
GET /transform/w_600,h_600,c_pad,bg_ffffff/landscape.jpg
# Result: Image is 600x600 with white bars top/bottom
```

#### `c_stretch` (Distort)
Stretch image to exact dimensions, ignoring aspect ratio.

```bash
# Stretch to specific dimensions
GET /transform/w_800,h_400,c_stretch/image.jpg
# Result: Image may appear distorted
```

---

### Gravity / Focal Point (`g_`)

Control which part of the image to keep when cropping (used with `c_fill`, `c_cover`, `c_thumb`).

#### Named Gravity

```
g_center        - Center of image [default]
g_north         - Top center
g_south         - Bottom center
g_east          - Right center
g_west          - Left center
g_northeast     - Top right
g_northwest     - Top left
g_southeast     - Bottom right
g_southwest     - Bottom left
```

**Examples:**
```bash
# Crop to top of image
GET /transform/w_400,h_400,c_fill,g_north/portrait.jpg

# Crop to bottom-right corner
GET /transform/w_600,h_400,c_fill,g_southeast/landscape.jpg
```

#### Custom Focal Point (`g_xy`)

Use `x_` and `y_` parameters to specify exact focal point (0.0 to 1.0).

```
g_xy            - Enable custom focal point
x_0.3           - X focal point: 30% from left
y_0.4           - Y focal point: 40% from top
```

**Coordinate System:**
- `x_0.0` = Left edge, `x_1.0` = Right edge
- `y_0.0` = Top edge, `y_1.0` = Bottom edge
- `x_0.5,y_0.5` = Center (equivalent to `g_center`)

**Examples:**
```bash
# Focus on face in portrait (30% from left, 40% from top)
GET /transform/w_400,h_400,c_fill,g_xy,x_0.3,y_0.4/portrait.jpg

# Focus on product in corner (70% from left, 60% from top)
GET /transform/w_600,h_600,c_cover,g_xy,x_0.7,y_0.6/product.jpg
```

---

### Quality (`q_`)

Control image compression quality (0-100).

```
q_auto          - Auto optimize quality based on format
q_100           - Maximum quality (largest file size)
q_85            - High quality [recommended for most images]
q_80            - Good quality (good balance)
q_70            - Economy quality (smaller files)
q_best          - Alias for 100
q_good          - Alias for 85
q_eco           - Alias for 70
```

**Quality Guidelines:**
- **JPEG:** 80-90 for photos, 70 for thumbnails
- **WebP:** 85-95 (better compression than JPEG)
- **AVIF:** 80-90 (best compression)
- **PNG:** Quality parameter ignored (lossless)

**Examples:**
```bash
# Auto-optimize based on format
GET /transform/w_800,q_auto,f_auto/photo.jpg

# High quality JPEG
GET /transform/w_1920,q_90,f_jpeg/photo.jpg

# Economy quality for thumbnails
GET /transform/w_200,h_200,q_70,f_jpeg/thumb.jpg
```

---

### Format Conversion (`f_`)

Convert images to different formats.

```
f_auto          - Auto select format based on Accept header
f_jpeg          - Convert to JPEG
f_jpg           - Alias for JPEG
f_png           - Convert to PNG
f_webp          - Convert to WebP (better compression than JPEG)
f_avif          - Convert to AVIF (best compression, modern browsers)
f_bmp           - Convert to BMP
f_gif           - Convert to GIF
```

#### `f_auto` - Browser-Aware Format Selection

MediaMatic automatically detects browser capabilities and serves the optimal format:

**Format Priority:**
1. **AVIF** - Newest browsers (Chrome 85+, Firefox 93+, Safari 16+)
2. **WebP** - Modern browsers (Chrome, Firefox, Edge, Opera)
3. **JPEG/PNG** - Legacy browsers and fallback

**Detection Method:**
- Parses `Accept` header from browser
- Uses `DeviceDetector.NET` for User-Agent analysis
- Falls back to original format if no better option

**Example:**
```bash
# Same URL returns different formats based on browser
GET /transform/w_800,f_auto/photo.jpg

# Chrome 100: Returns AVIF (smallest)
# Safari 15: Returns WebP
# IE 11: Returns JPEG (original format)
```

#### Format Comparison

| Format | Compression | Quality | Transparency | Browser Support | Use Case |
|--------|-------------|---------|--------------|-----------------|----------|
| **AVIF** | Best | Excellent | ✅ | Modern | New sites, progressive enhancement |
| **WebP** | Very Good | Excellent | ✅ | Modern | Wide compatibility |
| **JPEG** | Good | Good | ❌ | Universal | Photos, maximum compatibility |
| **PNG** | Lossless | Perfect | ✅ | Universal | Graphics, logos, transparency |
| **BMP** | None | Perfect | ❌ | Universal | Uncompressed (large files) |
| **GIF** | Good | Limited | ✅ | Universal | Animations, simple graphics |

---

### Background Color (`bg_`)

Set background color for `c_pad` mode or transparent areas.

```
bg_ffffff       - White background (hex RGB)
bg_000000       - Black background
bg_ff0000       - Red background
bg_00ff00       - Green background
bg_0000ff       - Blue background
bg_transparent  - Transparent background (PNG/WebP/AVIF only)
```

**Format:** 6-character hex RGB (without `#` prefix).

**Examples:**
```bash
# Pad with white background
GET /transform/w_600,h_600,c_pad,bg_ffffff,f_png/logo.png

# Pad with black background
GET /transform/ar_16:9,w_1920,c_pad,bg_000000,f_jpeg/image.jpg

# Keep transparency (PNG/WebP/AVIF)
GET /transform/w_400,h_400,c_pad,bg_transparent,f_webp/logo.png
```

---

### Metadata Handling (`m_`)

Control whether to strip or preserve image metadata (EXIF, GPS, etc.).

```
m_keep          - Keep all metadata [default for non-optimized]
m_strip         - Strip all metadata (EXIF, GPS, camera info)
```

**Why Strip Metadata?**
- **Privacy** - Remove GPS coordinates, camera info, timestamps
- **File Size** - EXIF data can add 50-500KB to file size
- **Security** - Prevent leaking sensitive information

**Examples:**
```bash
# Strip metadata for privacy
GET /transform/w_800,q_80,m_strip,f_jpeg/user-upload.jpg

# Keep metadata for archival
GET /transform/w_1920,q_90,m_keep,f_jpeg/professional-photo.jpg
```

---

### Effects (`e_`) - Coming Soon

**Note:** Effects are planned for Phase 3 implementation.

```
e_grayscale     - Convert to grayscale
e_sepia         - Apply sepia tone effect
e_blur_10       - Blur with radius 10
e_sharpen_5     - Sharpen with amount 5
e_brightness_20 - Adjust brightness (+/- 100)
e_contrast_15   - Adjust contrast (+/- 100)
e_saturation_-30 - Adjust saturation (+/- 100)
```

---

### Cache Control (`v_`)

Version identifier for cache busting.

```
v_1             - Version 1
v_20250124      - Date-based version
v_abc123        - Git commit hash
```

**Example:**
```bash
# Original request (cached by browser/CDN)
GET /transform/w_400,f_png,v_1/logo.png

# Update logo, increment version to bust cache
GET /transform/w_400,f_png,v_2/logo.png
```

---

## Query Parameters

### Force Download (`download`)

Force the browser to download the transformed image instead of displaying it inline:

```bash
# Display image inline (default)
GET /transform/w_800,f_webp/photo.jpg

# Force download
GET /transform/w_800,f_webp/photo.jpg?download=true
```

### Save Transformed Result (`saveTo`)

Save the transformed image to a specific path for caching/pre-generation:

```bash
# Transform and save to thumbs folder
GET /transform/w_400,f_webp/images/photo.jpg?saveTo=thumbs/photo_400.webp
```

**Behavior:**
- Transforms the image
- Saves to the specified path (overwrites if exists)
- Returns the transformed image

---

## POST Transform Endpoints (CMS Pre-Generation)

For CMS workflows where you want to pre-generate thumbnails without streaming the image back, use the POST endpoints. These return metadata instead of the image, making them more efficient for batch operations.

### Single Transform

```
POST /api/mm/fs/{filesourceId}/transform/{transformations}/{*filePath}
```

**Request Body:**
```json
{
  "saveTo": "thumbs/photo_400.webp"
}
```

**Response (201 Created):**
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

**Example:**
```bash
curl -X POST "https://example.com/api/mm/fs/default/transform/w_400,h_300,f_webp/images/photo.jpg" \
  -H "Content-Type: application/json" \
  -d '{"saveTo": "thumbs/photo_400x300.webp"}'
```

### Batch Transform

Generate multiple variants of a source image in one request:

```
POST /api/mm/fs/{filesourceId}/transform-batch/
```

**Request Body:**
```json
{
  "source": "images/photo.jpg",
  "variants": [
    { "transformations": "w_400,f_webp", "saveTo": "thumbs/photo_400.webp" },
    { "transformations": "w_800,f_webp", "saveTo": "thumbs/photo_800.webp" },
    { "transformations": "w_1200,f_webp", "saveTo": "thumbs/photo_1200.webp" },
    { "transformations": "w_200,h_200,c_cover,f_webp", "saveTo": "thumbs/photo_square.webp" }
  ]
}
```

**Response (201 Created):**
```json
{
  "results": [
    { "path": "thumbs/photo_400.webp", "size": 12345, "width": 400, "height": 300, "format": "webp", "success": true },
    { "path": "thumbs/photo_800.webp", "size": 34567, "width": 800, "height": 600, "format": "webp", "success": true },
    { "path": "thumbs/photo_1200.webp", "size": 56789, "width": 1200, "height": 900, "format": "webp", "success": true },
    { "path": "thumbs/photo_square.webp", "size": 8901, "width": 200, "height": 200, "format": "webp", "success": true }
  ]
}
```

### CMS Pre-Generation Workflow

A typical CMS workflow for handling image uploads:

```
1. User uploads original image
   POST /api/mm/fs/default/files/images/photo.jpg

2. CMS generates all required variants
   POST /api/mm/fs/default/transform-batch/
   Body: {
     "source": "images/photo.jpg",
     "variants": [
       { "transformations": "w_400,f_webp", "saveTo": "thumbs/photo_400.webp" },
       { "transformations": "w_800,f_webp", "saveTo": "thumbs/photo_800.webp" },
       { "transformations": "w_200,h_200,c_cover,f_webp", "saveTo": "thumbs/photo_square.webp" }
     ]
   }

3. Website serves pre-generated files directly (no runtime processing)
   GET /api/mm/fs/default/files/thumbs/photo_400.webp

4. When source image changes, CMS re-runs batch transform with same paths
```

**Benefits:**
- No runtime processing overhead for end users
- Consistent thumbnail generation across deployments
- Easy to regenerate all variants when source changes
- Metadata response allows CMS to track file sizes and dimensions

---

## Complete Examples

### Responsive Image Set

Generate a complete set of responsive images:

```bash
# Mobile (small)
GET /transform/w_640,ar_16:9,c_fill,q_80,f_auto/hero.jpg

# Tablet (medium)
GET /transform/w_1024,ar_16:9,c_fill,q_85,f_auto/hero.jpg

# Desktop (large)
GET /transform/w_1920,ar_16:9,c_fill,q_85,f_auto/hero.jpg

# Desktop Retina (2x)
GET /transform/w_1920,ar_16:9,c_fill,q_85,f_auto,dpr_2/hero.jpg
```

### Product Thumbnails

```bash
# Square thumbnail (centered crop)
GET /transform/w_300,h_300,c_cover,g_center,q_80,f_webp/products/shoe.jpg

# Square thumbnail (custom focal point)
GET /transform/w_300,h_300,c_cover,g_xy,x_0.6,y_0.4,q_80,f_webp/products/shoe.jpg
```

### Profile Avatars

```bash
# Square avatar (200x200)
GET /transform/w_200,h_200,c_cover,g_north,q_85,f_webp,m_strip/avatars/user123.jpg

# Retina avatar (400x400 actual pixels)
GET /transform/w_200,h_200,c_cover,g_north,q_85,f_webp,m_strip,dpr_2/avatars/user123.jpg
```

### Hero Images

```bash
# Full-width hero (16:9)
GET /transform/w_1920,ar_16:9,c_fill,g_center,q_90,f_auto/hero.jpg

# Ultra-wide hero (21:9)
GET /transform/w_2560,ar_21:9,c_fill,g_center,q_90,f_auto/hero.jpg
```

### Logo with Padding

```bash
# Logo on white background (square)
GET /transform/w_400,h_400,c_pad,bg_ffffff,f_png/logos/brand.png

# Logo on transparent background
GET /transform/w_400,h_400,c_pad,bg_transparent,f_webp/logos/brand.png
```

### Privacy-Conscious User Uploads

```bash
# Strip all metadata, optimize
GET /transform/w_1200,c_fit,q_80,f_jpeg,m_strip/user-photos/vacation.jpg
```

---

## Integration Examples

### HTML `<img>` Tags

```html
<!-- Basic responsive image -->
<img src="/api/mm/fs/default/transform/w_800,c_fit,q_auto,f_auto/products/shoe.jpg"
     alt="Product"
     width="800"
     height="600">

<!-- Retina-ready image -->
<img src="/api/mm/fs/default/transform/w_200,h_200,dpr_2,f_png/logo.png"
     alt="Logo"
     width="200"
     height="200">
```

### HTML `<picture>` Element

```html
<picture>
  <!-- Modern browsers: AVIF -->
  <source srcset="/api/mm/fs/default/transform/w_1920,ar_16:9,c_fill,f_avif/hero.jpg"
          type="image/avif">

  <!-- Modern browsers: WebP -->
  <source srcset="/api/mm/fs/default/transform/w_1920,ar_16:9,c_fill,f_webp/hero.jpg"
          type="image/webp">

  <!-- Fallback: JPEG -->
  <img src="/api/mm/fs/default/transform/w_1920,ar_16:9,c_fill,f_jpeg/hero.jpg"
       alt="Hero Image">
</picture>
```

### Responsive Images with `srcset`

```html
<img
  src="/api/mm/fs/default/transform/w_800,c_fit,q_auto,f_auto/product.jpg"
  srcset="
    /api/mm/fs/default/transform/w_400,c_fit,q_auto,f_auto/product.jpg 400w,
    /api/mm/fs/default/transform/w_800,c_fit,q_auto,f_auto/product.jpg 800w,
    /api/mm/fs/default/transform/w_1200,c_fit,q_auto,f_auto/product.jpg 1200w,
    /api/mm/fs/default/transform/w_1600,c_fit,q_auto,f_auto/product.jpg 1600w
  "
  sizes="(max-width: 600px) 400px,
         (max-width: 1200px) 800px,
         1200px"
  alt="Product Image">
```

### CSS Background Images

```css
/* Standard display */
.hero {
  background-image: url('/api/mm/fs/default/transform/w_1920,ar_16:9,c_fill,q_85,f_auto/hero.jpg');
}

/* Retina display */
@media (-webkit-min-device-pixel-ratio: 2), (min-resolution: 192dpi) {
  .hero {
    background-image: url('/api/mm/fs/default/transform/w_1920,ar_16:9,c_fill,q_85,f_auto,dpr_2/hero.jpg');
  }
}
```

### React/Next.js Image Component

```jsx
import Image from 'next/image';

function ProductImage({ src, alt }) {
  const baseUrl = '/api/mm/fs/default';

  const buildTransformUrl = (path, transforms) =>
    `${baseUrl}/transform/${transforms}/${path}`;

  return (
    <Image
      src={buildTransformUrl(src, 'w_800,c_fit,q_auto,f_auto')}
      alt={alt}
      width={800}
      height={600}
      srcSet={`
        ${buildTransformUrl(src, 'w_400,c_fit,q_auto,f_auto')} 400w,
        ${buildTransformUrl(src, 'w_800,c_fit,q_auto,f_auto')} 800w,
        ${buildTransformUrl(src, 'w_1200,c_fit,q_auto,f_auto')} 1200w
      `}
    />
  );
}
```

---

## Caching & Performance

### HTTP Caching Headers

MediaMatic automatically sets appropriate cache headers:

```http
HTTP/1.1 200 OK
Content-Type: image/webp
Cache-Control: public, max-age=31536000, immutable
ETag: "abc123def456"
Last-Modified: Mon, 20 Jan 2025 12:00:00 GMT
```

**Header Explanation:**
- `Cache-Control: public, max-age=31536000, immutable` - Cache for 1 year (transformed images don't change)
- `ETag` - Fingerprint for cache validation
- `Last-Modified` - Timestamp for conditional requests

### Conditional Requests

MediaMatic supports efficient cache validation:

```http
# First request
GET /transform/w_800,f_webp/photo.jpg
→ 200 OK (full response)

# Subsequent request with ETag
GET /transform/w_800,f_webp/photo.jpg
If-None-Match: "abc123def456"
→ 304 Not Modified (no body, saves bandwidth)

# Or with Last-Modified
GET /transform/w_800,f_webp/photo.jpg
If-Modified-Since: Mon, 20 Jan 2025 12:00:00 GMT
→ 304 Not Modified
```

### Server-Side Caching

MediaMatic caches transformed images on disk to avoid re-processing:

```
Cache Location: ./cache/transformations/{filesourceId}/{filePath}/{transformations}.{ext}

Example:
./cache/transformations/default/products/shoe.jpg/w_400,h_300,c_fill,q_80,f_webp.webp
```

**Cache Configuration:**

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.TransformationCache = new TransformationCacheSettings
    {
        Enabled = true,
        CacheDirectory = "./cache/transformations",
        MaxCacheSizeBytes = 10_000_000_000, // 10GB
        CacheExpiration = TimeSpan.FromDays(30),
    };
});
```

### CDN Integration

Transformation URLs are CDN-friendly:

**Cloudflare:**
```
Cache Everything Page Rule:
  URL Pattern: /api/mm/fs/*/transform/*/*
  Cache Level: Cache Everything
  Edge Cache TTL: 1 month
```

**AWS CloudFront:**
```json
{
  "PathPattern": "/api/mm/fs/*/transform/*/*",
  "TargetOriginId": "MediaMaticOrigin",
  "ViewerProtocolPolicy": "redirect-to-https",
  "CachePolicyId": "658327ea-f89d-4fab-a63d-7e88639e58f6",
  "Compress": true
}
```

**Cache Key Optimization:**
- Include `Accept` header in cache key for `f_auto`
- Normalize transformation order (MediaMatic does this automatically)
- Strip unnecessary query parameters

---

## Best Practices

### 1. Use `f_auto` for Browser-Aware Serving

Let MediaMatic automatically choose the best format:

```bash
# ✅ Good: Auto format selection
GET /transform/w_800,c_fit,q_auto,f_auto/photo.jpg

# ❌ Avoid: Hardcoding format
GET /transform/w_800,c_fit,q_auto,f_jpeg/photo.jpg
```

### 2. Use `q_auto` for Quality Optimization

Let MediaMatic optimize quality based on format:

```bash
# ✅ Good: Auto quality
GET /transform/w_800,f_auto,q_auto/photo.jpg

# ❌ Avoid: Fixed quality across formats
GET /transform/w_800,f_auto,q_80/photo.jpg
```

### 3. Generate Responsive Image Sets

Provide multiple sizes for different screen sizes:

```html
<img
  src="/transform/w_800,c_fit,q_auto,f_auto"
  srcset="
    /transform/w_400,c_fit,q_auto,f_auto 400w,
    /transform/w_800,c_fit,q_auto,f_auto 800w,
    /transform/w_1200,c_fit,q_auto,f_auto 1200w,
    /transform/w_1600,c_fit,q_auto,f_auto 1600w
  "
  sizes="(max-width: 600px) 400px,
         (max-width: 1200px) 800px,
         1200px"
  alt="Hero">
```

### 4. Use Retina Images Sparingly

Only use `dpr_2` for critical images (logos, UI elements):

```bash
# ✅ Good: Logo (small, visible quality difference)
GET /transform/w_200,h_200,dpr_2,f_png/logo.png

# ⚠️ Caution: Hero image (large, minimal quality difference)
GET /transform/w_1920,h_1080,dpr_2,f_jpeg/hero.jpg  # Results in 3840x2160!
```

### 5. Strip Metadata for User Uploads

Protect user privacy by stripping metadata:

```bash
# ✅ Good: Strip metadata from user uploads
GET /transform/w_1200,c_fit,q_80,m_strip,f_jpeg/user-photos/vacation.jpg

# ❌ Avoid: Keeping metadata (may expose GPS, camera info)
GET /transform/w_1200,c_fit,q_80,f_jpeg/user-photos/vacation.jpg
```

### 6. Use Appropriate Crop Modes

Choose the right crop mode for your use case:

```bash
# Product photos: Fit entire product (may have empty space)
GET /transform/w_600,h_600,c_fit,bg_ffffff/products/shoe.jpg

# Thumbnails: Fill entire area (crop excess)
GET /transform/w_300,h_300,c_cover,g_center/products/shoe.jpg

# Banners: Fill width, specific aspect ratio
GET /transform/w_1920,ar_16:9,c_fill,g_center/banners/sale.jpg
```

### 7. Use Version Parameters for Cache Busting

When images change, increment version to force cache refresh:

```bash
# Original
GET /transform/w_400,f_png,v_1/logo.png

# Update logo.png on server, then increment version
GET /transform/w_400,f_png,v_2/logo.png
```

### 8. Optimize for Perceived Performance

Load critical images first, lazy-load below-the-fold images:

```html
<!-- Above the fold: Load immediately -->
<img src="/transform/w_1920,ar_16:9,c_fill,f_auto,q_auto"
     alt="Hero"
     loading="eager">

<!-- Below the fold: Lazy load -->
<img src="/transform/w_400,c_fit,f_auto,q_auto"
     alt="Product"
     loading="lazy">
```

---

## Comparison to Cloudinary

MediaMatic's transformation API is designed to be familiar to Cloudinary users while offering some advantages:

| Feature | Cloudinary | MediaMatic | Notes |
|---------|-----------|------------|-------|
| **URL Syntax** | `/image/upload/w_400,h_300,c_fill/...` | `/fi/{path}/t/w_400,h_300,c_fill` | Similar parameter style |
| **Auto Format** | `f_auto` | `f_auto` | ✅ Same behavior |
| **Crop Modes** | Multiple | `fit`, `fill`, `cover`, `pad`, `stretch` | ✅ All common modes |
| **Quality** | `q_auto`, `q_80` | `q_auto`, `q_80` | ✅ Same |
| **Gravity** | `g_center`, `g_face` | `g_center`, `g_xy` (custom) | ⚠️ No face detection (yet) |
| **DPR** | `dpr_2` | `dpr_2` | ✅ Same |
| **Effects** | Many | Coming soon | ⏳ Grayscale, blur, sharpen planned |
| **Cost** | $89-249+/month | Self-hosted (free) | 💰 Significant savings |
| **Data Ownership** | Cloudinary | Your infrastructure | ✅ Full control |
| **CDN** | Included | Bring your own | ⚠️ Requires CDN setup |
| **Face Detection** | ✅ | ❌ (planned) | 🔮 Future with ML.NET |
| **Video Transforms** | ✅ | ⏳ (thumbnails available) | 📋 Transcode via separate endpoint |

### Migration from Cloudinary

If you're migrating from Cloudinary, most URLs can be adapted with minimal changes:

**Cloudinary:**
```
https://res.cloudinary.com/demo/image/upload/w_400,h_300,c_fill,q_auto,f_auto/sample.jpg
```

**MediaMatic:**
```
https://yourdomain.com/api/mm/fs/default/transform/w_400,h_300,c_fill,q_auto,f_auto/sample.jpg
```

**Common Parameter Mapping:**
- `w_` → `w_` ✅ Same
- `h_` → `h_` ✅ Same
- `c_fill` → `c_fill` ✅ Same
- `c_scale` → `c_fit` ⚠️ Different name
- `g_face` → `g_xy,x_0.5,y_0.4` ⚠️ Manual focal point (no face detection yet)
- `q_auto` → `q_auto` ✅ Same
- `f_auto` → `f_auto` ✅ Same

---

## Troubleshooting

### Image Not Transforming

**Problem:** Original image is returned instead of transformed version.

**Solutions:**
1. Check transformation syntax: `w_400` not `w=400`
2. Verify file path is correct
3. Check filesource ID exists
4. Ensure image format is supported (JPEG, PNG, WebP, AVIF, BMP, GIF)

### Poor Image Quality

**Problem:** Transformed images appear pixelated or blurry.

**Solutions:**
1. Check original image resolution (can't upscale beyond original quality)
2. Increase quality: `q_85` or `q_90`
3. Use `f_auto` to let MediaMatic choose optimal format
4. Avoid excessive upscaling (keep `dpr_` reasonable)

### Slow First Request

**Problem:** First request for transformation is slow.

**Explanation:** MediaMatic processes on-demand. First request generates and caches the image.

**Solutions:**
1. Pre-warm cache for critical images
2. Use CDN with long cache times
3. Consider pre-generating common sizes on upload

### 404 Not Found

**Problem:** Transformed image returns 404.

**Solutions:**
1. Verify original file exists: `GET /transform/...`)/{path}` (without `
2. Check filesource ID is correct
3. Verify bucket name (if using S3/Azure)
4. Check file permissions

### Browser Shows Wrong Format

**Problem:** `f_auto` not working, always returns JPEG.

**Solutions:**
1. Check browser sends `Accept: image/webp,image/avif,*/*` header
2. Verify User-Agent is not blocked
3. Test with `f_webp` explicitly to confirm format conversion works
4. Clear browser cache

---

## Advanced Topics

### URL Signing (Coming Soon)

Prevent unauthorized transformations with signed URLs:

```bash
# Unsigned (anyone can generate transformations)
GET /transform/w_800,f_webp/photo.jpg

# Signed (requires secret key)
GET /transform/w_800,f_webp?s=abc123def456&e=1706198400/photo.jpg
```

### Named Transformations (Coming Soon)

Define preset transformations for consistency:

```csharp
// Configuration
builder.Services.AddMediaMatic(options =>
{
    options.NamedTransformations.Add("thumbnail", "w_300,h_300,c_cover,g_center,q_80,f_auto");
    options.NamedTransformations.Add("hero", "w_1920,ar_16:9,c_fill,g_center,q_90,f_auto");
});
```

```bash
# Use named transformation
GET /transform/preset_thumbnail/photo.jpg
GET /transform/preset_hero/banner.jpg
```

### Chained Transformations (Coming Soon)

Apply multiple transformation steps sequentially:

```bash
# First: Resize, Second: Apply blur, Third: Convert format
GET /transform/w_1200,h_800,c_fill/t/e_blur_5/t/q_80,f_webp/image.jpg
```

---

## API Reference Summary

### Quick Parameter Reference

```
# Dimensions
w_400                 - Width (pixels)
h_300                 - Height (pixels)
ar_16:9               - Aspect ratio (width:height or decimal)
dpr_2                 - Device pixel ratio (retina)

# Crop Modes
c_fit                 - Fit within bounds [default]
c_fill / c_cover      - Fill bounds (crop excess)
c_pad                 - Pad to exact dimensions (add background)
c_stretch             - Stretch to exact dimensions (distort)

# Gravity / Focal Point
g_center              - Center crop [default]
g_north / g_south     - Top / Bottom
g_east / g_west       - Right / Left
g_northeast, etc.     - Corners
g_xy,x_0.3,y_0.4      - Custom focal point (0.0-1.0)

# Quality
q_auto                - Auto optimize [recommended]
q_80                  - Quality 0-100
q_best / q_good / q_eco - Presets (100 / 85 / 70)

# Format
f_auto                - Auto format (AVIF > WebP > JPEG) [recommended]
f_webp / f_avif       - Specific formats
f_jpeg / f_png        - Legacy formats

# Other
bg_ffffff             - Background color (hex RGB, for c_pad)
m_strip               - Strip metadata (EXIF, GPS)
m_keep                - Keep metadata [default]
v_1                   - Version (cache busting)
```

---

## See Also

- [Image Processing Guide](./image-processing.md) - Upload images with variants
- [Video Processing Guide](./video-processing.md) - Video thumbnails and transcoding
- [Browser Detection](./browser-detection.md) - How `f_auto` works
- [Storage Providers](./storage-providers.md) - S3, Azure, Local, SFTP configuration
- [ASP.NET Core Integration](./aspnetcore-integration.md) - Setup and configuration

---

**Ready to transform your images?** Start with the [Getting Started](./getting-started.md) guide to set up MediaMatic!
