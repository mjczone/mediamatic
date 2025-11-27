// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Auditing;
using MJCZone.MediaMatic.AspNetCore.Repositories;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Processors;
using IVfsConnectionFactory = MJCZone.MediaMatic.AspNetCore.Factories.IVfsConnectionFactory;

namespace MJCZone.MediaMatic.AspNetCore.Services;

/// <summary>
/// Implementation of IMediaMaticService for ASP.NET Core applications.
/// </summary>
public partial class MediaMaticService : IMediaMaticService
{
    private readonly IMediaMaticFilesourceRepository _filesourceRepository;
    private readonly IVfsConnectionFactory _vfsConnectionFactory;
    private readonly IMediaMaticPermissions _permissions;
    private readonly IMediaMaticAuditLogger _auditLogger;
    private readonly IImageProcessor _imageProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMaticService"/> class.
    /// </summary>
    /// <param name="filesourceRepository">The filesource repository.</param>
    /// <param name="vfsConnectionFactory">The VFS connection factory.</param>
    /// <param name="permissions">The permissions manager.</param>
    /// <param name="auditLogger">The audit logger.</param>
    /// <param name="imageProcessor">The image processor.</param>
    public MediaMaticService(
        IMediaMaticFilesourceRepository filesourceRepository,
        IVfsConnectionFactory vfsConnectionFactory,
        IMediaMaticPermissions permissions,
        IMediaMaticAuditLogger auditLogger,
        IImageProcessor imageProcessor
    )
    {
        _filesourceRepository = filesourceRepository;
        _vfsConnectionFactory = vfsConnectionFactory;
        _permissions = permissions;
        _auditLogger = auditLogger;
        _imageProcessor = imageProcessor;
    }

    #region Shared Protected Methods

    /// <summary>
    /// Asserts that the current user has permission to perform the operation.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authorized.</exception>
    protected async Task AssertPermissionsAsync(IOperationContext context)
    {
        if (!await _permissions.IsAuthorizedAsync(context).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException($"User is not authorized to perform operation: {context.Operation}");
        }
    }

    /// <summary>
    /// Logs an audit event for the operation.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="description">A description of the operation.</param>
    /// <param name="errorMessage">Error message if the operation failed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task LogAuditEventAsync(
        IOperationContext context,
        bool success,
        string description,
        string? errorMessage = null
    )
    {
        var message = errorMessage != null ? $"{description}: {errorMessage}" : description;

        var auditEvent = new MediaMaticAuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            UserIdentifier = context.User?.Identity?.Name ?? "Anonymous",
            Operation = context.Operation ?? "Unknown",
            FilesourceId = context.FilesourceId,
            BucketName = context.BucketName,
            FolderPath = context.FolderPath,
            FilePath = context.FilePath,
            Success = success,
            Message = message,
            RequestId = context.RequestId,
            IpAddress = context.IpAddress,
        };

        await _auditLogger.LogOperationAsync(auditEvent).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a VFS connection for the specified filesource.
    /// </summary>
    /// <param name="filesourceId">The filesource ID.</param>
    /// <returns>A VFS connection.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the filesource is not found.</exception>
    protected async Task<IVfsConnection> GetVfsConnectionAsync(string filesourceId)
    {
        var connectionString = await _filesourceRepository.GetConnectionStringAsync(filesourceId).ConfigureAwait(false);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new KeyNotFoundException($"Filesource not found: {filesourceId}");
        }

        var filesource =
            await _filesourceRepository.GetFilesourceAsync(filesourceId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Filesource not found: {filesourceId}");

        if (string.IsNullOrEmpty(filesource.Provider))
        {
            throw new InvalidOperationException($"Filesource {filesourceId} has no provider configured");
        }

        return _vfsConnectionFactory.CreateConnection(filesource.Provider, connectionString);
    }

    /// <summary>
    /// Prepends bucket name to path if specified.
    /// If bucket is in connection string, it's used automatically by the provider.
    /// If bucket is specified here, prepend it to the path for runtime bucket switching.
    /// </summary>
    /// <param name="bucketName">Optional bucket name.</param>
    /// <param name="path">The path.</param>
    /// <returns>The combined path.</returns>
    protected static string CombineBucketAndPath(string? bucketName, string? path)
    {
        if (string.IsNullOrEmpty(bucketName))
        {
            return path ?? string.Empty;
        }

        if (string.IsNullOrEmpty(path))
        {
            return bucketName;
        }

        return $"{bucketName}/{path.TrimStart('/')}";
    }

    #endregion // Shared Protected Methods
}
