# MediaMatic Testing Strategy

## Overview

This test suite uses **xUnit**, **FluentAssertions**, and **Testcontainers** to provide comprehensive coverage of MediaMatic's file storage and media processing capabilities.

## Test Organization

### Provider Tests
- `LocalProviderTests.cs` - Tests for local filesystem storage (no container needed)
- `MemoryProviderTests.cs` - Tests for in-memory storage (no container needed)
- `S3ProviderTests.cs` - Tests for S3-compatible storage (uses LocalStack container)

### Test Fixtures

We use **xUnit Collection Fixtures** to share Testcontainers across multiple test classes, minimizing resource usage:

```csharp
[Collection("S3Provider")]
public class S3ProviderTests : IAsyncLifetime
{
    private readonly LocalStackFixture _localStackFixture;

    public S3ProviderTests(LocalStackFixture localStackFixture)
    {
        _localStackFixture = localStackFixture;
    }
}
```

**Key Benefits:**
- **Single Container Per Provider**: Only 1 LocalStack container is created and shared across ALL S3 tests
- **Fast Test Execution**: Containers start once and are reused
- **Proper Cleanup**: xUnit handles container lifecycle automatically

##Parameterized Tests

We use `[Theory]` with `[InlineData]` to maximize code coverage with minimal test code:

```csharp
[Theory]
[InlineData("file1.txt", "Content 1")]
[InlineData("file2.txt", "Content 2")]
[InlineData("file3.txt", "Content 3")]
public async Task UploadFile_Should_Work_For_Various_Files(string fileName, string content)
{
    // Single test method covers multiple scenarios
}
```

## Test Containers

### LocalStack (S3-Compatible)

- **Image**: `localstack/localstack:latest`
- **Services**: S3
- **Reuse Strategy**: Collection Fixture (shared across all S3 tests)
- **Lifecycle**: Starts once per test run

### Future Containers

When additional providers are implemented:
- **Azure Blob Storage**: Use `Testcontainers.Azurite`
- **FTP/SFTP**: Custom test containers

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific provider tests
dotnet test --filter "FullyQualifiedName~LocalProviderTests"
dotnet test --filter "FullyQualifiedName~S3ProviderTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Test Coverage Goals

- **Basic Storage Operations**: Upload, download, list, delete, exists
- **Image Processing**: Resize, format conversion, WebP/AVIF generation, thumbnails
- **Video Processing**: Thumbnail generation, transcoding, metadata extraction
- **Metadata Extraction**: MIME detection, EXIF data, video/audio metadata

## Current Coverage

| Provider | Basic Storage | Image Processing | Video Processing | Metadata |
|----------|--------------|------------------|------------------|----------|
| **Local** | ✅ Complete | ⏳ Pending | ⏳ Pending | ⏳ Pending |
| **Memory** | ✅ Complete | ⏳ Pending | ⏳ Pending | ⏳ Pending |
| **S3** | ✅ Complete | ⏳ Pending | ⏳ Pending | ⏳ Pending |

## Contributing Tests

When adding new tests:

1. **Use appropriate test fixtures** for testcontainers
2. **Leverage [Theory]/[InlineData]** for parameterized tests
3. **Follow FluentAssertions** for readable assertions
4. **Test both success and failure scenarios**
5. **Clean up resources** (use IDisposable/IAsyncLifetime)

## Example Test Pattern

```csharp
[Fact]
public async Task Operation_Should_Succeed_When_Valid()
{
    // Arrange
    var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);
    using var stream = CreateTestStream();

    // Act
    var result = await vfs.UploadFileAsync(stream, "test.txt");

    // Assert
    result.Should().Be("test.txt");
    (await vfs.ExistsAsync("test.txt")).Should().BeTrue();
}

[Fact]
public async Task Operation_Should_Throw_When_Invalid()
{
    // Arrange
    var vfs = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);

    // Act
    var act = async () => await vfs.UploadFileAsync(null!, "test.txt");

    // Assert
    await act.Should().ThrowAsync<ArgumentNullException>();
}
```

## Test Data Strategy

- **Unique file names**: Use `Guid.NewGuid()` to avoid conflicts between tests
- **Temporary directories**: LocalProviderTests creates isolated temp directories
- **Small test files**: Keep test data minimal for fast execution
- **Real media files**: Use actual image/video files for media processing tests (future)
