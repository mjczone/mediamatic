// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Options;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;

namespace MJCZone.MediaMatic.AspNetCore.Repositories;

/// <summary>
/// Base class for implementations of IMediaMaticFilesourceRepository.
/// </summary>
public abstract class MediaMaticFilesourceRepositoryBase : IMediaMaticFilesourceRepository
{
    private readonly string? _encryptionKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMaticFilesourceRepositoryBase"/> class.
    /// </summary>
    /// <param name="options">The MediaMatic options.</param>
    protected MediaMaticFilesourceRepositoryBase(IOptions<MediaMaticOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _encryptionKey = options.Value.ConnectionStringEncryptionKey;
    }

    /// <inheritdoc />
    public virtual void Initialize()
    {
        // Default implementation does nothing
    }

    /// <inheritdoc />
    public abstract Task<bool> AddFilesourceAsync(FilesourceDto filesource);

    /// <inheritdoc />
    public abstract Task<bool> FilesourceExistsAsync(string id);

    /// <inheritdoc />
    public abstract Task<string?> GetConnectionStringAsync(string id);

    /// <inheritdoc />
    public abstract Task<FilesourceDto?> GetFilesourceAsync(string id);

    /// <inheritdoc />
    public abstract Task<List<FilesourceDto>> GetFilesourcesAsync(string? tag = null);

    /// <inheritdoc />
    public abstract Task<bool> RemoveFilesourceAsync(string id);

    /// <inheritdoc />
    public abstract Task<bool> UpdateFilesourceAsync(FilesourceDto filesource);

    /// <summary>
    /// Encrypts a connection string for secure storage.
    /// </summary>
    /// <param name="connectionString">The plain text connection string.</param>
    /// <returns>The encrypted connection string.</returns>
    public virtual string EncryptConnectionString(string connectionString)
    {
        return CryptoUtils.EncryptToBase64(
            connectionString,
            _encryptionKey ?? throw new InvalidOperationException("Encryption key is not configured.")
        );
    }

    /// <summary>
    /// /// Decrypts an encrypted connection string for internal use.
    /// </summary>
    /// <param name="encryptedConnectionString">The encrypted connection string.</param>
    /// <returns>The decrypted plain text connection string.</returns>
    public virtual string DecryptConnectionString(string encryptedConnectionString)
    {
        return CryptoUtils.DecryptFromBase64(
            encryptedConnectionString,
            _encryptionKey ?? throw new InvalidOperationException("Encryption key is not configured.")
        );
    }
}
