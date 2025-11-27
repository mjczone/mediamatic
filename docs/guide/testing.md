# Testing

MediaMatic provides  testing support using Testcontainers for integration tests and in-memory storage for unit tests.

## Testing Philosophy

Following DapperMatic's pattern, MediaMatic tests against real services when possible:

| Test Type | Storage | Speed | Reliability |
|-----------|---------|-------|-------------|
| Unit | In-Memory | Fast | High |
| Integration | Testcontainers | Medium | Very High |
| E2E | Real Services | Slow | Production-like |

## Test Project Setup

### Create Test Project

```bash
dotnet new xunit -n MyProject.Tests
cd MyProject.Tests
dotnet add reference ../MyProject/MyProject.csproj
```

### Required Packages

```bash
dotnet add package MJCZone.MediaMatic
dotnet add package FluentAssertions
dotnet add package Testcontainers.LocalStack
```

## Unit Testing

### Testing Image Processing

```csharp
using FluentAssertions;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;

public class ImageProcessorTests
{
    private readonly ImageProcessor _processor = new();

    [Fact]
    public async Task ResizeAsync_Should_Resize_By_Width()
    {
        // Arrange
        using var stream = CreateTestImage(800, 600);

        // Act
        var result = await _processor.ResizeAsync(stream, width: 400, height: null);

        // Assert
        result.width.Should().Be(400);
        result.height.Should().Be(300); // Aspect ratio preserved
    }

    [Fact]
    public async Task ResizeAsync_Cover_Should_Crop_To_Exact_Dimensions()
    {
        // Arrange
        using var stream = CreateTestImage(800, 600);
        var options = new ImageProcessingOptions { ResizeMode = ResizeMode.Cover };

        // Act
        var result = await _processor.ResizeAsync(stream, 400, 400, options);

        // Assert
        result.width.Should().Be(400);
        result.height.Should().Be(400);
    }

    [Theory]
    [InlineData(ResizeMode.Fit)]
    [InlineData(ResizeMode.Cover)]
    [InlineData(ResizeMode.Pad)]
    [InlineData(ResizeMode.Stretch)]
    public async Task ResizeAsync_All_Modes_Should_Produce_Valid_Output(ResizeMode mode)
    {
        // Arrange
        using var stream = CreateTestImage(640, 480);
        var options = new ImageProcessingOptions { ResizeMode = mode };

        // Act
        var result = await _processor.ResizeAsync(stream, 320, 240, options);

        // Assert
        result.fileSize.Should().BeGreaterThan(0);
    }

    private static Stream CreateTestImage(int width, int height)
    {
        // Create test image with SkiaSharp
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Blue);

        var stream = new MemoryStream();
        bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 100);
        stream.Position = 0;
        return stream;
    }
}
```

### Testing with In-Memory Storage

```csharp
using MJCZone.MediaMatic;
using MJCZone.MediaMatic;

public class StorageTests
{
    [Fact]
    public async Task Should_Upload_And_Download_File()
    {
        // Arrange
        using var vfs = VfsConnection.Create(
            VfsProviderType.Memory,
            "memory://name=test"
        );
        var content = "Hello, World!"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        await vfs.UploadFileAsync(stream, "test.txt");
        using var downloaded = await vfs.DownloadAsync("test.txt");
        using var ms = new MemoryStream();
        await downloaded.CopyToAsync(ms);

        // Assert
        ms.ToArray().Should().BeEquivalentTo(content);
    }
}
```

## Integration Testing

### LocalStack (S3)

```csharp
using Testcontainers.LocalStack;
using MJCZone.MediaMatic;
using MJCZone.MediaMatic;
using MJCZone.MediaMatic.Interfaces;

public class S3IntegrationTests : IAsyncLifetime
{
    private LocalStackContainer _localstack = null!;
    private IVfsConnection _vfs = null!;

    public async Task InitializeAsync()
    {
        _localstack = new LocalStackBuilder()
            .WithImage("localstack/localstack:latest")
            .Build();

        await _localstack.StartAsync();

        // Create VFS connection
        var endpoint = _localstack.GetConnectionString();
        _vfs = VfsConnection.Create(
            VfsProviderType.S3,
            $"s3://keyId=test;key=test;bucket=test;region=us-east-1;serviceUrl={endpoint}"
        );
    }

    public async Task DisposeAsync()
    {
        _vfs?.Dispose();
        await _localstack.DisposeAsync();
    }

    [Fact]
    public async Task Should_Upload_Image_To_S3()
    {
        // Arrange
        using var stream = CreateTestImage(800, 600);

        // Act
        await _vfs.UploadFileAsync(stream, "images/test.jpg");

        // Assert
        var exists = await _vfs.ExistsAsync("images/test.jpg");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Process_And_Upload_Image()
    {
        // Arrange
        using var stream = CreateTestImage(1920, 1080);

        // Act
        var result = await _vfs.UploadImageAsync(stream, "images/test.jpg", new ImageUploadOptions
        {
            GenerateThumbnails = true,
            ThumbnailSizes = [320, 640],
        });

        // Assert
        result.Success.Should().BeTrue();
        result.Variants.Should().HaveCount(2);
    }
}
```

### Testing Video Processing

```csharp
public class VideoProcessorIntegrationTests : IAsyncLifetime
{
    private LocalStackContainer _localstack = null!;
    private string _testVideoPath = null!;

    public async Task InitializeAsync()
    {
        _localstack = new LocalStackBuilder().Build();
        await _localstack.StartAsync();

        // Create test video
        _testVideoPath = await CreateTestVideoAsync();
    }

    public async Task DisposeAsync()
    {
        if (File.Exists(_testVideoPath))
            File.Delete(_testVideoPath);

        await _localstack.DisposeAsync();
    }

    [Fact]
    public async Task Should_Generate_Thumbnails()
    {
        // Arrange
        var processor = new VideoProcessor();
        var options = new ThumbnailOptions
        {
            Count = 3,
            Width = 320,
            OutputPath = Path.GetTempPath(),
        };

        // Act
        var thumbnails = await processor.GenerateThumbnailsAsync(_testVideoPath, options);

        // Assert
        thumbnails.Should().HaveCount(3);
        thumbnails.Should().AllSatisfy(t => File.Exists(t).Should().BeTrue());
    }

    private async Task<string> CreateTestVideoAsync()
    {
        // Use FFmpeg to create test video
        var path = Path.GetTempFileName() + ".mp4";

        await FFMpegArguments
            .FromFileInput("testsrc=duration=5:size=640x480:rate=30", false, opt =>
                opt.ForceFormat("lavfi"))
            .OutputToFile(path, true, opt =>
                opt.WithVideoCodec("libx264"))
            .ProcessAsynchronously();

        return path;
    }
}
```

## Test Helpers

### TestDataHelper

Create a helper class for generating test data:

```csharp
public static class TestDataHelper
{
    public static Stream CreateTestJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Create colored test pattern
        canvas.Clear(SKColors.Blue);

        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawRect(0, 0, width / 2, height / 2, paint);

        var stream = new MemoryStream();
        bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 85);
        stream.Position = 0;
        return stream;
    }

    public static Stream CreateTestPng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        var stream = new MemoryStream();
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        stream.Position = 0;
        return stream;
    }
}
```

### Test Fixtures

Share expensive resources across tests:

```csharp
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic;

public class LocalStackFixture : IAsyncLifetime
{
    public LocalStackContainer Container { get; private set; } = null!;
    public IVfsConnection Vfs { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Container = new LocalStackBuilder().Build();
        await Container.StartAsync();

        Vfs = VfsConnection.Create(
            VfsProviderType.S3,
            $"s3://keyId=test;key=test;bucket=test;region=us-east-1;serviceUrl={Container.GetConnectionString()}"
        );
    }

    public async Task DisposeAsync()
    {
        Vfs?.Dispose();
        await Container.DisposeAsync();
    }
}

[CollectionDefinition("LocalStack")]
public class LocalStackCollection : ICollectionFixture<LocalStackFixture> { }

[Collection("LocalStack")]
public class S3Tests
{
    private readonly LocalStackFixture _fixture;

    public S3Tests(LocalStackFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_With_Shared_LocalStack()
    {
        var storage = _fixture.Storage;
        // ...
    }
}
```

## Test Organization

### Recommended Structure

```
tests/
└── MyProject.Tests/
    ├── ProcessorTests/
    │   ├── ImageProcessorTests.cs
    │   └── VideoProcessorTests.cs
    ├── StorageTests/
    │   ├── LocalStorageTests.cs
    │   ├── S3StorageTests.cs
    │   └── SftpStorageTests.cs
    ├── MetadataTests/
    │   ├── MimeTypeTests.cs
    │   └── ExifTests.cs
    ├── TestHelpers/
    │   └── TestDataHelper.cs
    └── Fixtures/
        └── LocalStackFixture.cs
```

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~ImageProcessorTests"

# Run with verbose output
dotnet test -v n

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD (GitHub Actions)

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Install FFmpeg
        run: sudo apt-get install -y ffmpeg

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal
```

## Best Practices

1. **Use In-Memory for Unit Tests** - Fast and reliable
2. **Use Testcontainers for Integration** - Real services without external dependencies
3. **Create Helper Methods** - Reduce boilerplate for test data
4. **Share Fixtures** - Expensive resources like containers
5. **Clean Up** - Delete temp files and dispose resources
6. **Test Edge Cases** - Empty files, large files, invalid input

## Next Steps

- [Getting Started](getting-started.md) - Quick start guide
- [Configuration](configuration.md) - Configure options
- [Storage Providers](storage-providers.md) - Provider setup
