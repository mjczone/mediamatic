# Storage Providers

MediaMatic uses [FluentStorage](https://github.com/robinrodricks/FluentStorage) for storage abstraction, providing access to 13+ storage providers with a unified API.

## Supported Providers

| Provider | Package | Connection String Prefix |
|----------|---------|--------------------------|
| AWS S3 | `FluentStorage.AWS` | `aws.s3://` |
| Google Cloud | `FluentStorage.GCP` | `gcs://` |
| MinIO | `FluentStorage.AWS` | `aws.s3://` |
| DigitalOcean Spaces | `FluentStorage.AWS` | `aws.s3://` |
| Backblaze B2 | `FluentStorage.AWS` | `aws.s3://` |
| Local File System | (built-in) | N/A |
| In-Memory | (built-in) | N/A |
| SFTP | `FluentStorage.SFTP` | `sftp://` |
| Zip File | (built-in) | N/A |

## AWS S3

### Installation

```bash
dotnet add package FluentStorage.AWS
```

### Configuration

```csharp
using FluentStorage;

var storage = StorageFactory.Blobs.FromConnectionString(
    "aws.s3://keyId=AKIAIOSFODNN7EXAMPLE;key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;bucket=my-bucket;region=us-east-1"
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

For EC2/ECS with IAM roles, omit credentials:

```csharp
var storage = StorageFactory.Blobs.FromConnectionString(
    "aws.s3://bucket=my-bucket;region=us-east-1"
);
```

## Google Cloud Storage

### Installation

```bash
dotnet add package FluentStorage.GCP
```

### Configuration

```csharp
var storage = StorageFactory.Blobs.FromConnectionString(
    "gcs://projectId=my-project;bucket=my-bucket;jsonKeyPath=/path/to/service-account.json"
);
```

### Connection String Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| `projectId` | GCP project ID | Yes |
| `bucket` | GCS bucket name | Yes |
| `jsonKeyPath` | Path to service account JSON | Yes* |

## Local File System

No additional packages required.

```csharp
// Absolute path
var storage = StorageFactory.Blobs.DirectoryFiles("/var/media");

// Relative path
var storage = StorageFactory.Blobs.DirectoryFiles("./uploads");
```

::: warning
Ensure the application has read/write permissions to the directory.
:::

## In-Memory Storage

No additional packages required. Useful for testing.

```csharp
var storage = StorageFactory.Blobs.InMemory();
```

::: tip
In-memory storage is perfect for unit tests as it doesn't require any external services.
:::

## MinIO

MinIO is S3-compatible, so use the AWS package with a custom endpoint:

```csharp
using Amazon.S3;
using FluentStorage;

var config = new AmazonS3Config
{
    ServiceURL = "http://localhost:9000",
    ForcePathStyle = true,
};

var client = new AmazonS3Client("minioadmin", "minioadmin", config);
var storage = StorageFactory.Blobs.FromAwsS3(client, "my-bucket");
```

## DigitalOcean Spaces

DigitalOcean Spaces is S3-compatible:

```csharp
using Amazon.S3;
using FluentStorage;

var config = new AmazonS3Config
{
    ServiceURL = "https://nyc3.digitaloceanspaces.com",
};

var client = new AmazonS3Client("DO_ACCESS_KEY", "DO_SECRET_KEY", config);
var storage = StorageFactory.Blobs.FromAwsS3(client, "my-space");
```

## SFTP

### Installation

```bash
dotnet add package FluentStorage.SFTP
```

### Configuration

```csharp
var storage = StorageFactory.Blobs.FromConnectionString(
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

## Provider Operations

All providers support the same operations:

### Write Files

```csharp
// From stream
await storage.WriteAsync("path/to/file.jpg", stream);

// From bytes
await storage.WriteAsync("path/to/file.jpg", bytes);

// From text
await storage.WriteTextAsync("path/to/file.txt", "content");
```

### Read Files

```csharp
// To stream
using var stream = await storage.OpenReadAsync("path/to/file.jpg");

// To bytes
var bytes = await storage.ReadBytesAsync("path/to/file.jpg");

// To text
var text = await storage.ReadTextAsync("path/to/file.txt");
```

### List Files

```csharp
// List all files
var files = await storage.ListAsync();

// List files with prefix
var images = await storage.ListAsync("images/");

// Recursive listing
var all = await storage.ListAsync(recurse: true);
```

### Delete Files

```csharp
await storage.DeleteAsync("path/to/file.jpg");
```

### Check Existence

```csharp
var exists = await storage.ExistsAsync("path/to/file.jpg");
```

## Testing with Testcontainers

MediaMatic uses Testcontainers for integration testing:

### LocalStack (S3)

```csharp
await using var localstack = new LocalStackBuilder()
    .WithImage("localstack/localstack:latest")
    .Build();

await localstack.StartAsync();

var storage = StorageFactory.Blobs.FromConnectionString(
    $"aws.s3://keyId=test;key=test;bucket=test;region=us-east-1;serviceUrl={localstack.GetConnectionString()}"
);
```

## Best Practices

### Environment-Based Configuration

```csharp
var connectionString = builder.Configuration.GetConnectionString("MediaStorage");
var storage = StorageFactory.Blobs.FromConnectionString(connectionString);
```

```json
{
  "ConnectionStrings": {
    "MediaStorage": "aws.s3://keyId=...;key=...;bucket=...;region=..."
  }
}
```

### Dependency Injection

```csharp
builder.Services.AddSingleton<IBlobStorage>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return StorageFactory.Blobs.FromConnectionString(
        config.GetConnectionString("MediaStorage")
    );
});
```

### Error Handling

```csharp
try
{
    await storage.WriteAsync(path, stream);
}
catch (StorageException ex)
{
    logger.LogError(ex, "Failed to upload to {Path}", path);
    throw;
}
```

## Next Steps

- [Image Processing](image-processing.md) - Process images with SkiaSharp
- [Video Processing](video-processing.md) - Process videos with FFMpegCore
- [Testing](testing.md) - Test with Testcontainers
