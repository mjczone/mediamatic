// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage.Blobs;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Represents the base class for VFS methods.
/// </summary>
public abstract partial class VfsMethodsBase : IVfsMethods
{
    #region Media processor fields (used by VfsMethodsBase.Media.cs)

    private IMimeTypeDetector? _mimeDetector;
    private IMetadataReader? _metadataReader;
    private IImageProcessor? _imageProcessor;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VfsMethodsBase"/> class.
    /// </summary>
    /// <param name="providerType">The VFS provider type.</param>
    internal VfsMethodsBase(VfsProviderType providerType)
    {
        ProviderType = providerType;
        InitializeMediaProcessors();
    }

    /// <summary>
    /// Gets the type of the VFS provider.
    /// </summary>
    public VfsProviderType ProviderType { get; }

    /// <summary>
    /// Gets a value indicating whether the provider supports buckets.
    /// </summary>
    public virtual bool SupportsBuckets => false;

    /// <summary>
    /// Gets a value indicating whether the provider supports encrypted storage.
    /// </summary>
    public virtual bool SupportsNativeEncryptedStorage => false;

    /// <summary>
    /// Gets a value indicating whether the provider supports streaming.
    /// </summary>
    public virtual bool SupportsStreaming => false;

    #region Media processor properties (used by VfsMethodsBase.Media.cs)

    private IMimeTypeDetector MimeDetector =>
        _mimeDetector ?? throw new InvalidOperationException("Media processors not initialized");

    private IMetadataReader MetadataReader =>
        _metadataReader ?? throw new InvalidOperationException("Media processors not initialized");

    private IImageProcessor ImageProcessor =>
        _imageProcessor ?? throw new InvalidOperationException("Media processors not initialized");

    #endregion

    #region Basic file operations - Generic implementations using IBlobStorage

    /// <inheritdoc/>
    public virtual async Task<string> UploadFileAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        bool overwrite = false,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);

        if (!overwrite && await blobStorage.ExistsAsync(path, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"File already exists at path: {path}");
        }

        await blobStorage.WriteAsync(path, stream, false, cancellationToken).ConfigureAwait(false);
        return path;
    }

    /// <inheritdoc/>
    public virtual Task<VideoUploadResult> UploadVideoAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        VideoUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException($"UploadVideoAsync not implemented for {ProviderType}");
    }

    /// <inheritdoc/>
    public virtual async Task<Stream> DownloadAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        return await blobStorage.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        return await blobStorage.ExistsAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<string>> ListFilesAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = false,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return blobs.Where(b => !b.IsFolder).Select(b => b.FullPath);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<string>> ListFoldersAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = false,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return blobs.Where(b => b.IsFolder).Select(b => b.FullPath);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);
        await blobStorage.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteFolderAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorage = GetBlobStorage(vfs);

        // List all files in the folder recursively
        var blobs = await blobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = path,
                    Recurse = true,
                    FilePrefix = null,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // Delete all files
        var filePaths = blobs.Where(b => !b.IsFolder).Select(b => b.FullPath).ToList();
        if (filePaths.Count != 0)
        {
            await blobStorage.DeleteAsync(filePaths, cancellationToken).ConfigureAwait(false);
        }

        // Delete all folders
        var folderPaths = blobs.Where(b => b.IsFolder).Select(b => b.FullPath).ToList();
        if (folderPaths.Count != 0)
        {
            await blobStorage.DeleteAsync(folderPaths, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion

    #region Media processing operations - Stub implementations (override in provider classes)

    /// <inheritdoc/>
    public virtual Task<List<VideoThumbnail>> GenerateThumbnailsAsync(
        IVfsConnection vfs,
        string sourcePath,
        ThumbnailOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException($"GenerateThumbnailsAsync not implemented for {ProviderType}");
    }

    /// <inheritdoc/>
    public virtual Task<VideoProcessingResult> TranscodeVideoAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        TranscodeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException($"TranscodeVideoAsync not implemented for {ProviderType}");
    }

    /// <inheritdoc/>
    public virtual Task<MediaMetadata> GetMetadataAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException($"GetMetadataAsync not implemented for {ProviderType}");
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Gets the blob storage from the VFS connection.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <returns>The blob storage instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection type is invalid.</exception>
    protected static IBlobStorage GetBlobStorage(IVfsConnection vfs)
    {
        if (vfs is not VfsConnectionBase connectionBase)
        {
            throw new InvalidOperationException("Invalid VFS connection type");
        }

        return connectionBase.BlobStorage;
    }

    /// <summary>
    /// Initializes media processor instances (implemented in VfsMethodsBase.Media.cs).
    /// </summary>
    partial void InitializeMediaProcessors();

    #endregion
}
