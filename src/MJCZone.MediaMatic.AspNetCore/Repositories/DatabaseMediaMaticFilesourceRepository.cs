// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic;
using MJCZone.DapperMatic.AspNetCore.Factories;
using MJCZone.DapperMatic.DataAnnotations;
using MJCZone.DapperMatic.Models;
using MJCZone.MediaMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Repositories;

/// <summary>
/// Database-based implementation of IMediaMaticFilesourceRepository that stores filesources in a vfs table with encrypted connection strings.
/// </summary>
public sealed class DatabaseMediaMaticFilesourceRepository : MediaMaticFilesourceRepositoryBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IFilesourceIdFactory _filesourceIdFactory;
    private readonly ILogger<DatabaseMediaMaticFilesourceRepository> _logger;
    private readonly string _provider;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMediaMaticFilesourceRepository"/> class.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <param name="connectionFactory">The connection factory for creating database connections to store filesource connection information.</param>
    /// <param name="filesourceIdFactory">The factory for generating filesource IDs.</param>
    /// <param name="options">The MediaMatic options containing the encryption key.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseMediaMaticFilesourceRepository(
        string provider,
        string connectionString,
        IDbConnectionFactory connectionFactory,
        IFilesourceIdFactory filesourceIdFactory,
        IOptions<MediaMaticOptions> options,
        ILogger<DatabaseMediaMaticFilesourceRepository> logger
    )
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(connectionString);

        _provider = provider;
        _connectionString = connectionString;
        _connectionFactory = connectionFactory;
        _filesourceIdFactory = filesourceIdFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        var tableModel = DmTableFactory.GetTable(typeof(DatabaseFilesource));
        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);
        connection.CreateTableIfNotExistsAsync(tableModel).GetAwaiter().GetResult();
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

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql =
            @"
                INSERT INTO mm_filesources (id, provider, encrypted_connection_string, display_name, description, tags, is_enabled, created_at, updated_at)
                VALUES (@Id, @Provider, @EncryptedConnectionString, @DisplayName, @Description, @Tags, @IsEnabled, @CreatedAt, @UpdatedAt)";

        var parameters = new
        {
            Id = filesource.Id.ToLowerInvariant(),
            filesource.Provider,
            EncryptedConnectionString = EncryptConnectionString(filesource.ConnectionString),
            filesource.DisplayName,
            filesource.Description,
            Tags = $";{string.Join(";", filesource.Tags ?? [])};".Replace(
                ";;",
                string.Empty,
                StringComparison.OrdinalIgnoreCase
            ),
            filesource.IsEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var rowsAffected = await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public override async Task<bool> UpdateFilesourceAsync(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        if (string.IsNullOrWhiteSpace(filesource.Id))
        {
            throw new ArgumentException("Filesource ID is required.", nameof(filesource));
        }

        if (!await FilesourceExistsAsync(filesource.Id).ConfigureAwait(false))
        {
            return false; // Doesn't exist
        }

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = new StringBuilder();
        sql.Append("UPDATE mm_filesources SET ");

        if (filesource.Provider != null)
        {
            sql.Append("provider = @Provider, ");
        }

        if (!string.IsNullOrWhiteSpace(filesource.ConnectionString))
        {
            sql.Append("encrypted_connection_string = @EncryptedConnectionString, ");
        }

        if (!string.IsNullOrWhiteSpace(filesource.DisplayName))
        {
            sql.Append("display_name = @DisplayName, ");
        }

        if (!string.IsNullOrWhiteSpace(filesource.Description))
        {
            sql.Append("description = @Description, ");
        }

        if (filesource.Tags != null && filesource.Tags.Count != 0)
        {
            sql.Append("tags = @Tags, ");
        }

        if (filesource.IsEnabled != null)
        {
            sql.Append("is_enabled = @IsEnabled, ");
        }

        sql.Append("updated_at = @UpdatedAt ");
        sql.Append("WHERE id = @Id");

        var parameters = new
        {
            Id = filesource.Id.ToLowerInvariant(),
            filesource.Provider,
            EncryptedConnectionString = string.IsNullOrWhiteSpace(filesource.ConnectionString)
                ? null
                : EncryptConnectionString(filesource.ConnectionString),
            DisplayName = string.IsNullOrWhiteSpace(filesource.DisplayName) ? null : filesource.DisplayName,
            Description = string.IsNullOrWhiteSpace(filesource.Description) ? null : filesource.Description,
            Tags = $";{string.Join(";", filesource.Tags ?? [])};".Replace(
                ";;",
                string.Empty,
                StringComparison.OrdinalIgnoreCase
            ),
            filesource.IsEnabled,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var rowsAffected = await connection.ExecuteAsync(sql.ToString(), parameters).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public override async Task<bool> RemoveFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = "DELETE FROM mm_filesources WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id.ToLowerInvariant() }).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public override async Task<List<FilesourceDto>> GetFilesourcesAsync(string? tag = null)
    {
        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = "SELECT * FROM mm_filesources";
        if (!string.IsNullOrWhiteSpace(tag))
        {
            sql += " WHERE LOWER(tags) LIKE @TagPattern ORDER BY display_name";
        }
        var results = await connection
            .QueryAsync<DatabaseFilesource>(sql, new { TagPattern = $"%;{tag};%" })
            .ConfigureAwait(false);

        return
        [
            .. results
                .Select(r => new FilesourceDto
                {
                    Id = r.id!,
                    Provider = r.provider,
                    ConnectionString = null, // Do not expose connection string
                    DisplayName = r.display_name,
                    Description = r.description,
                    Tags = !string.IsNullOrEmpty(r.tags)
                        ? r
                            .tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList()
                        : null,
                    IsEnabled = r.is_enabled,
                    CreatedAt = r.created_at,
                    UpdatedAt = r.updated_at,
                })
                .OrderBy(d => d.Id),
        ];
    }

    /// <inheritdoc />
    public override async Task<FilesourceDto?> GetFilesourceAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = "SELECT * FROM mm_filesources WHERE id = @Id";
        var result = await connection
            .QuerySingleOrDefaultAsync<DatabaseFilesource>(sql, new { Id = id.ToLowerInvariant() })
            .ConfigureAwait(false);

        if (result == null)
        {
            return null;
        }

        return new FilesourceDto
        {
            Id = result.id!,
            Provider = result.provider,
            ConnectionString = null, // Do not expose connection string
            DisplayName = result.display_name,
            Description = result.description,
            Tags = !string.IsNullOrEmpty(result.tags)
                ? result
                    .tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
                : null,
            IsEnabled = result.is_enabled,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at,
        };
    }

    /// <inheritdoc />
    public override async Task<bool> FilesourceExistsAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = "SELECT COUNT(1) FROM mm_filesources WHERE id = @Id";
        var count = await connection
            .ExecuteScalarAsync<int>(sql, new { Id = id.ToLowerInvariant() })
            .ConfigureAwait(false);
        return count > 0;
    }

    /// <inheritdoc />
    public override async Task<string?> GetConnectionStringAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var connection = _connectionFactory.CreateConnection(_provider, _connectionString);

        var sql = "SELECT encrypted_connection_string FROM mm_filesources WHERE id = @Id";
        var encryptedConnectionString = await connection
            .QuerySingleOrDefaultAsync<string>(sql, new { Id = id.ToLowerInvariant() })
            .ConfigureAwait(false);

        try
        {
            return encryptedConnectionString != null ? DecryptConnectionString(encryptedConnectionString) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt connection string for filesource ID {FilesourceId}", id);
            return null;
        }
    }

#pragma warning disable SA1600
#pragma warning disable SA1300
#pragma warning disable IDE1006
    /// <summary>
    /// Represents a vfs table for storing MediaMatic filesources.
    /// </summary>
    [DmTable(null, "mm_filesources")]
    private sealed class DatabaseFilesource
    {
        [DmColumn("id", isPrimaryKey: true, isNullable: false)]
        public required string id { get; set; }

        [DmColumn("provider", isNullable: false)]
        public required string provider { get; set; }

        [DmColumn("encrypted_connection_string", length: int.MaxValue, isNullable: false)]
        public required string encrypted_connection_string { get; set; }

        [DmColumn("display_name", length: 256, isNullable: false)]
        public required string display_name { get; set; }

        [DmColumn("description", length: 512, isNullable: true)]
        public string? description { get; set; }

        [DmColumn("tags", length: 2048, isNullable: true)]
        public string? tags { get; set; }

        [DmColumn("is_enabled", isNullable: false)]
        public bool is_enabled { get; set; }

        public DateTimeOffset created_at { get; set; }

        public DateTimeOffset updated_at { get; set; }
    }
#pragma warning restore IDE1006
#pragma warning restore SA1300
#pragma warning restore SA1600
}
