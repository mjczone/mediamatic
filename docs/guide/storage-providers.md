# Storage Providers

MediaMatic provides a Virtual File System (VFS) abstraction supporting multiple storage providers with a unified API.

## Supported Providers

| Provider | VfsProviderType | Connection String Prefix |
|----------|-----------------|--------------------------|
| AWS S3 | `S3` | `s3://` |
| Google Cloud Storage | `GCP` | `gcp://` |
| MinIO | `Minio` | `minio://` |
| Backblaze B2 | `B2` | `b2://` |
| Local File System | `Local` | (path) |
| In-Memory | `Memory` | `memory://` |
| SFTP | `SFTP` | `sftp://` |
| Zip File | `ZipFile` | (path to .zip) |

## Creating Connections

Use `VfsConnection.Create()` to create connections:

```csharp
using MJCZone.MediaMatic;

// Create a connection
using var vfs = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=...;key=...;bucket=my-bucket;region=us-east-1"
);
```

## AWS S3

```csharp
using var s3 = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://keyId=AKIAIOSFODNN7EXAMPLE;key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;bucket=my-bucket;region=us-east-1"
);
```

### Connection String Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| `keyId` | AWS Access Key ID | Yes |
| `key` | AWS Secret Access Key | Yes |
| `bucket` | S3 bucket name | Yes |
| `region` | AWS region (e.g., `us-east-1`) | Yes |

### Using IAM Roles

For EC2/ECS with IAM roles, credentials can be omitted if the environment is configured:

```csharp
using var s3 = VfsConnection.Create(
    VfsProviderType.S3,
    "s3://bucket=my-bucket;region=us-east-1"
);
```

## Google Cloud Storage

```csharp
using var gcp = VfsConnection.Create(
    VfsProviderType.GCP,
    "gcp://projectId=my-project;bucket=my-bucket;jsonKeyPath=/path/to/service-account.json"
);
```

### Connection String Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| `projectId` | GCP project ID | Yes |
| `bucket` | GCS bucket name | Yes |
| `jsonKeyPath` | Path to service account JSON | Yes* |

## Local File System

```csharp
// Absolute path
using var local = VfsConnection.Create(
    VfsProviderType.Local,
    "/var/media"
);

// Relative path
using var local = VfsConnection.Create(
    VfsProviderType.Local,
    "./uploads"
);
```

::: warning
Ensure the application has read/write permissions to the directory.
:::

## In-Memory Storage

Useful for testing. Data persists only during application lifetime.

```csharp
// Default instance
using var memory = VfsConnection.Create(
    VfsProviderType.Memory,
    "memory://"
);

// Named instance (useful for test isolation)
using var memory = VfsConnection.Create(
    VfsProviderType.Memory,
    "memory://name=test-instance"
);
```

::: tip
Named instances share storage across connections with the same name. Use unique names for test isolation.
:::

## MinIO

MinIO is an S3-compatible object storage server:

```csharp
using var minio = VfsConnection.Create(
    VfsProviderType.Minio,
    "minio://endpoint=localhost:9000;accessKey=minioadmin;secretKey=minioadmin;bucket=my-bucket"
);
```

### Connection String Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| `endpoint` | MinIO server endpoint | Yes |
| `accessKey` | Access key | Yes |
| `secretKey` | Secret key | Yes |
| `bucket` | Bucket name | Yes |
| `secure` | Use HTTPS (default: false) | No |

## Backblaze B2

```csharp
using var b2 = VfsConnection.Create(
    VfsProviderType.B2,
    "b2://keyId=...;applicationKey=...;bucketId=..."
);
```

## SFTP

```csharp
using var sftp = VfsConnection.Create(
    VfsProviderType.SFTP,
    "sftp://host=sftp.example.com;port=22;username=user;password=pass;path=/uploads"
);
```

### Connection String Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| `host` | SFTP server hostname | Yes |
| `port` | SFTP port (default: 22) | No |
| `username` | Username | Yes |
| `password` | Password | Yes* |
| `path` | Base path on server | No |

## VFS Operations

All providers support the same operations via extension methods:

### Upload Files

```csharp
using MJCZone.MediaMatic;

// Upload a file
using var stream = File.OpenRead("photo.jpg");
await vfs.UploadFileAsync(stream, "images/photo.jpg");

// Upload with overwrite
await vfs.UploadFileAsync(stream, "images/photo.jpg", overwrite: true);

// Upload an image with processing
var result = await vfs.UploadImageAsync(stream, "images/photo.jpg", new ImageUploadOptions
{
    GenerateThumbnails = true,
    ThumbnailSizes = [320, 640, 1024],
    GenerateFormats = true,
    Formats = [ImageFormat.WebP],
});
```

### Download Files

```csharp
// Download to stream
using var stream = await vfs.DownloadAsync("images/photo.jpg");

// Copy to file
using var fileStream = File.Create("downloaded.jpg");
await stream.CopyToAsync(fileStream);
```

### List Files and Folders

```csharp
// List files in a folder
var files = await vfs.ListFilesAsync("images/");

// List folders
var folders = await vfs.ListFoldersAsync("images/");
```

### Create Folders

```csharp
// Create a folder
await vfs.CreateFolderAsync("images/gallery/");

// Create nested folders (creates parent folders if needed)
await vfs.CreateFolderAsync("uploads/2025/01/photos/");
```

### Delete Files and Folders

```csharp
// Delete a file
await vfs.DeleteAsync("images/photo.jpg");

// Delete a folder and all contents
await vfs.DeleteFolderAsync("images/gallery/");
```

### Check Existence

```csharp
var exists = await vfs.ExistsAsync("images/photo.jpg");
```

### Get Metadata

```csharp
var metadata = await vfs.GetMetadataAsync("images/photo.jpg");
Console.WriteLine($"MIME Type: {metadata.MimeType}");
Console.WriteLine($"Dimensions: {metadata.Width}x{metadata.Height}");
```

### Process Images

```csharp
// Resize an image
var result = await vfs.ProcessImageAsync(
    "images/photo.jpg",
    "images/photo_thumb.jpg",
    new ImageProcessingOptions
    {
        Width = 300,
        Format = ImageFormat.WebP,
        Quality = 80,
    }
);
```

## Testing with Testcontainers

MediaMatic includes test fixtures for Testcontainers:

### LocalStack (S3)

```csharp
await using var localstack = new LocalStackBuilder()
    .WithImage("localstack/localstack:latest")
    .Build();

await localstack.StartAsync();

using var s3 = VfsConnection.Create(
    VfsProviderType.S3,
    $"s3://keyId=test;key=test;bucket=test;region=us-east-1;serviceUrl={localstack.GetConnectionString()}"
);
```

### MinIO Container

```csharp
await using var minio = new MinioBuilder()
    .WithImage("minio/minio:latest")
    .Build();

await minio.StartAsync();

using var vfs = VfsConnection.Create(
    VfsProviderType.Minio,
    $"minio://endpoint={minio.GetConnectionString()};accessKey=minioadmin;secretKey=minioadmin;bucket=test"
);
```

## Best Practices

### Environment-Based Configuration

```csharp
var provider = Enum.Parse<VfsProviderType>(
    builder.Configuration["Storage:Provider"] ?? "Local"
);
var connectionString = builder.Configuration["Storage:ConnectionString"]!;

using var vfs = VfsConnection.Create(provider, connectionString);
```

```json
{
  "Storage": {
    "Provider": "S3",
    "ConnectionString": "s3://keyId=...;key=...;bucket=...;region=..."
  }
}
```

### ASP.NET Core Integration

For web applications, use the MediaMatic ASP.NET Core package with filesources:

```csharp
builder.Services.AddMediaMatic(options =>
{
    options.UseInMemoryFilesourceRepository();
});

// Configure filesources via REST API or programmatically
```

### Error Handling

```csharp
try
{
    await vfs.UploadFileAsync(stream, path);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
{
    logger.LogWarning("File already exists at {Path}", path);
}
catch (KeyNotFoundException ex)
{
    logger.LogError("File not found: {Path}", path);
}
```

## Next Steps

- [Image Processing](image-processing.md) - Process images with SkiaSharp
- [Video Processing](video-processing.md) - Process videos with FFMpegCore
- [ASP.NET Core Integration](aspnetcore-integration.md) - Web application integration
- [Testing](testing.md) - Test with Testcontainers
