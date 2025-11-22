// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MJCZone.MediaMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Repositories;

/// <summary>
/// File-based implementation of IMediaMaticFilesourceRepository that stores filesources in a JSON file with encrypted connection strings.
/// </summary>
public sealed class FileMediaMaticFilesourceRepository : MediaMaticFilesourceRepositoryBase, IDisposable
{
    private readonly DataStore _dataStore;
    private readonly IFilesourceIdFactory _filesourceIdFactory;
    private readonly ILogger<FileMediaMaticFilesourceRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMediaMaticFilesourceRepository"/> class.
    /// </summary>
    /// <param name="filePath">The path to the JSON file where filesources will be stored.</param>
    /// <param name="filesourceIdFactory">The factory to generate filesource IDs.</param>
    /// <param name="options">The MediaMatic options containing the encryption key.</param>
    /// <param name="logger">The logger instance.</param>
    public FileMediaMaticFilesourceRepository(
        string filePath,
        IFilesourceIdFactory filesourceIdFactory,
        IOptions<MediaMaticOptions> options,
        ILogger<FileMediaMaticFilesourceRepository> logger
    )
        : base(options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        filePath = Path.GetFullPath(filePath);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _dataStore = new DataStore(filePath);
        _filesourceIdFactory = filesourceIdFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        // Ensure the collection exists
        GetCollection();
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

        // Store the connection string encrypted
        var filesourceCopy = new FilesourceDto
        {
            Id = filesource.Id,
            Provider = filesource.Provider,
            ConnectionString = EncryptConnectionString(filesource.ConnectionString),
            DisplayName = filesource.DisplayName,
            Description = filesource.Description,
            Tags = filesource.Tags?.ToList() ?? [],
            IsEnabled = filesource.IsEnabled.GetValueOrDefault(true),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return await GetCollection().InsertOneAsync(filesourceCopy).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<bool> UpdateFilesourceAsync(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        if (string.IsNullOrWhiteSpace(filesource.Id))
        {
            throw new ArgumentException("Filesource ID is required.", nameof(filesource));
        }

        // Fetch existing to preserve non-updated fields
        var existing = await GetFilesourceAsync(filesource.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return false; // Doesn't exist
        }

        if (filesource.Provider != null)
        {
            existing.Provider = filesource.Provider;
        }

        if (!string.IsNullOrWhiteSpace(filesource.ConnectionString))
        {
            existing.ConnectionString = EncryptConnectionString(filesource.ConnectionString);
        }

        if (!string.IsNullOrWhiteSpace(filesource.DisplayName))
        {
            existing.DisplayName = filesource.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(filesource.Description))
        {
            existing.Description = filesource.Description;
        }

        if (filesource.Tags != null && filesource.Tags.Count != 0)
        {
            existing.Tags = [.. filesource.Tags];
        }

        if (filesource.IsEnabled != null)
        {
            existing.IsEnabled = filesource.IsEnabled;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        return await GetCollection()
            .ReplaceOneAsync(
                d => d.Id != null && d.Id.Equals(existing.Id, StringComparison.OrdinalIgnoreCase),
                existing
            )
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<bool> RemoveFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var removed = await GetCollection()
            .DeleteOneAsync(d => d.Id != null && d.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            .ConfigureAwait(false);

        return removed;
    }

    /// <inheritdoc />
    public override Task<List<FilesourceDto>> GetFilesourcesAsync(string? tag = null)
    {
        var collection = GetCollection();

        List<FilesourceDto> filesources = string.IsNullOrWhiteSpace(tag)
            ? [.. collection.AsQueryable().OrderBy(d => d.DisplayName)]
            :
            [
                .. collection
                    .AsQueryable()
                    .Where(d => d.Tags != null && d.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(d => d.DisplayName),
            ];

        foreach (var ds in filesources)
        {
            // Do not return the encrypted connection string
            ds.ConnectionString = null;
        }

        return Task.FromResult(filesources);
    }

    /// <inheritdoc />
    public override Task<FilesourceDto?> GetFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var filesource = GetCollection()
            .AsQueryable()
            .FirstOrDefault(d => d.Id != null && d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (filesource != null)
        {
            // Do not return the encrypted connection string
            filesource.ConnectionString = null;
        }

        return Task.FromResult(filesource);
    }

    /// <inheritdoc />
    public override async Task<bool> FilesourceExistsAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return await GetFilesourceAsync(id).ConfigureAwait(false) != null;
    }

    /// <inheritdoc />
    public override Task<string?> GetConnectionStringAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var filesource = GetCollection()
            .AsQueryable()
            .FirstOrDefault(d => d.Id != null && d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (filesource == null)
        {
            return Task.FromResult<string?>(null);
        }

        var encryptedConnectionString = filesource.ConnectionString;

        try
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(encryptedConnectionString)
                    ? DecryptConnectionString(encryptedConnectionString)
                    : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt connection string for filesource ID {FilesourceId}", id);
            return Task.FromResult<string?>(null);
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="FileMediaMaticFilesourceRepository"/> class.
    /// </summary>
    public void Dispose()
    {
        _dataStore?.Dispose();
    }

    private IDocumentCollection<FilesourceDto> GetCollection()
    {
        return _dataStore.GetCollection<FilesourceDto>("filesources");
    }
}
