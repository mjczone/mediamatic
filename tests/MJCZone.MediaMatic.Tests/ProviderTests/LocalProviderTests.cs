// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers;

namespace MJCZone.MediaMatic.Tests.ProviderTests;

/// <summary>
/// Tests for Local provider basic file operations.
/// No testcontainer needed - uses temporary local filesystem.
/// </summary>
public class LocalProviderTests : VfsProviderTestsBase, IDisposable
{
    private readonly string _tempDirectory;

    public LocalProviderTests()
    {
        // Create a unique temp directory for this test run
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mediamatic-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        // Cleanup temp directory after tests
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    protected override Task<IVfsConnection> CreateConnectionAsync()
    {
        var connectionString = $"local://path={_tempDirectory}";
        var connection = VfsProviderFactories.CreateConnection(VfsProviderType.Local, connectionString);
        return Task.FromResult(connection);
    }

    #region Additional Local Provider Specific Tests

    [Fact]
    public async Task UploadFile_Should_Throw_When_File_Exists_And_Overwrite_False()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var testContent = "Initial content";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        await vfs.UploadFileAsync(stream1, "test.txt");

        // Act
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("New content"));
        var act = async () => await vfs.UploadFileAsync(stream2, "test.txt", overwrite: false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task UploadFile_Should_Overwrite_When_Overwrite_True()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var initialContent = "Initial content";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(initialContent));
        await vfs.UploadFileAsync(stream1, "test.txt");

        // Act
        var newContent = "New content";
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
        await vfs.UploadFileAsync(stream2, "test.txt", overwrite: true);

        // Assert
        var filePath = Path.Combine(_tempDirectory, "test.txt");
        File.ReadAllText(filePath).Should().Be(newContent);
    }

    [Fact]
    public async Task UploadFile_Should_Create_File_At_Specified_Path()
    {
        // Arrange
        using var vfs = await CreateConnectionAsync();

        var testContent = "Hello, MediaMatic!";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = await vfs.UploadFileAsync(stream, "test.txt");

        // Assert
        result.Should().Be("test.txt");
        var filePath = Path.Combine(_tempDirectory, "test.txt");
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be(testContent);
    }

    #endregion
}
