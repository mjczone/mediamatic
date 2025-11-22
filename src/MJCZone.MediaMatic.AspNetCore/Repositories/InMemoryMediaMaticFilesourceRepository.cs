// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MJCZone.MediaMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Repositories;

/// <summary>
/// In-memory implementation of IMediaMaticFilesourceRepository.
/// Provides thread-safe storage of filesource registrations with secure connection string handling.
/// Connection strings are stored but never exposed through public APIs.
/// </summary>
internal sealed class InMemoryMediaMaticFilesourceRepository : MediaMaticFilesourceRepositoryBase
{
    private readonly ConcurrentDictionary<string, FilesourceDto> _filesources = new();
    private readonly IFilesourceIdFactory _filesourceIdFactory;
    private readonly ILogger<InMemoryMediaMaticFilesourceRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMediaMaticFilesourceRepository"/> class.
    /// </summary>
    /// <param name="options">The MediaMatic options containing the encryption key.</param>
    /// <param name="filesourceIdFactory">The factory to generate filesource IDs.</param>
    /// <param name="logger">The logger instance.</param>
    public InMemoryMediaMaticFilesourceRepository(
        IOptions<MediaMaticOptions> options,
        IFilesourceIdFactory filesourceIdFactory,
        ILogger<InMemoryMediaMaticFilesourceRepository> logger
    )
        : base(options)
    {
        _filesourceIdFactory = filesourceIdFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task<bool> AddFilesourceAsync(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        if (string.IsNullOrWhiteSpace(filesource.Id))
        {
            filesource.Id = _filesourceIdFactory.GenerateId(filesource);
            if (string.IsNullOrWhiteSpace(filesource.Id))
            {
                throw new ArgumentException("Filesource ID is required.", nameof(filesource));
            }
        }

        if (filesource.Provider == null)
        {
            throw new ArgumentException("Filesource provider is required.", nameof(filesource));
        }

        if (string.IsNullOrWhiteSpace(filesource.DisplayName))
        {
            throw new ArgumentException("Filesource display name is required.", nameof(filesource));
        }

        if (string.IsNullOrWhiteSpace(filesource.ConnectionString))
        {
            throw new ArgumentException("Filesource connection string is required.", nameof(filesource));
        }

        if (await FilesourceExistsAsync(filesource.Id).ConfigureAwait(false))
        {
            return false; // Already exists
        }

        // Create a copy to avoid external modification
        FilesourceDto filesourceCopy = new FilesourceDto
        {
            Id = filesource.Id,
            Provider = filesource.Provider,
            ConnectionString = EncryptConnectionString(filesource.ConnectionString),
            DisplayName = filesource.DisplayName,
            Description = filesource.Description,
            Tags = filesource.Tags?.ToList() ?? [],
            IsEnabled = filesource.IsEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return _filesources.TryAdd(filesource.Id.ToLowerInvariant(), filesourceCopy);
    }

    /// <inheritdoc />
    public override Task<bool> UpdateFilesourceAsync(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        if (string.IsNullOrWhiteSpace(filesource.Id))
        {
            throw new ArgumentException("Filesource ID is required.", nameof(filesource));
        }

        // Get the current stored filesource for comparison
        if (!_filesources.TryGetValue(filesource.Id.ToLowerInvariant(), out FilesourceDto? storedFilesource))
        {
            return Task.FromResult(false); // Doesn't exist
        }

        // Create updated version based on stored filesource
        FilesourceDto updatedFilesource = new FilesourceDto
        {
            Id = storedFilesource.Id,
            Provider = filesource.Provider ?? storedFilesource.Provider,
            ConnectionString = !string.IsNullOrWhiteSpace(filesource.ConnectionString)
                ? EncryptConnectionString(filesource.ConnectionString)
                : storedFilesource.ConnectionString,
            DisplayName = !string.IsNullOrWhiteSpace(filesource.DisplayName)
                ? filesource.DisplayName
                : storedFilesource.DisplayName,
            Description = !string.IsNullOrWhiteSpace(filesource.Description)
                ? filesource.Description
                : storedFilesource.Description,
            Tags =
                filesource.Tags != null && filesource.Tags.Count != 0
                    ? [.. filesource.Tags]
                    : storedFilesource.Tags?.ToList() ?? [],
            IsEnabled = filesource.IsEnabled ?? storedFilesource.IsEnabled,
            CreatedAt = storedFilesource.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        bool result = _filesources.TryUpdate(filesource.Id.ToLowerInvariant(), updatedFilesource, storedFilesource);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public override Task<bool> RemoveFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        bool removed = _filesources.TryRemove(id.ToLowerInvariant(), out _);

        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public override Task<List<FilesourceDto>> GetFilesourcesAsync(string? tag = null)
    {
        IEnumerable<FilesourceDto> collection = _filesources.Values.AsEnumerable();

        List<FilesourceDto> sourceFilesources = string.IsNullOrWhiteSpace(tag)
            ? [.. collection.OrderBy(d => d.DisplayName)]
            :
            [
                .. collection
                    .Where(d => d.Tags != null && d.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(d => d.DisplayName),
            ];

        // Create copies to avoid modifying the stored objects
        List<FilesourceDto> filesources = sourceFilesources
            .Select(ds => new FilesourceDto
            {
                Id = ds.Id,
                Provider = ds.Provider,
                ConnectionString = null, // Do not return the encrypted connection string
                DisplayName = ds.DisplayName,
                Description = ds.Description,
                Tags = ds.Tags?.ToList() ?? [],
                IsEnabled = ds.IsEnabled,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt,
            })
            .ToList();

        return Task.FromResult(filesources);
    }

    /// <inheritdoc />
    public override Task<FilesourceDto?> GetFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!_filesources.TryGetValue(id.ToLowerInvariant(), out FilesourceDto? filesource) || filesource == null)
        {
            return Task.FromResult<FilesourceDto?>(null);
        }

        // Create a copy to avoid modifying the stored object
        FilesourceDto result = new()
        {
            Id = filesource.Id,
            Provider = filesource.Provider,
            ConnectionString = null, // Do not return the encrypted connection string
            DisplayName = filesource.DisplayName,
            Description = filesource.Description,
            Tags = filesource.Tags?.ToList() ?? [],
            IsEnabled = filesource.IsEnabled,
            CreatedAt = filesource.CreatedAt,
            UpdatedAt = filesource.UpdatedAt,
        };

        return Task.FromResult<FilesourceDto?>(result);
    }

    /// <inheritdoc />
    public override Task<bool> FilesourceExistsAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        bool exists = _filesources.ContainsKey(id.ToLowerInvariant());
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public override Task<string?> GetConnectionStringAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _filesources.TryGetValue(id.ToLowerInvariant(), out FilesourceDto? filesource);
        string? encryptedConnectionString = filesource?.ConnectionString;

        try
        {
            return Task.FromResult(
                encryptedConnectionString != null ? DecryptConnectionString(encryptedConnectionString) : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt connection string for filesource ID {FilesourceId}", id);
            return Task.FromResult<string?>(null);
        }
    }
}
