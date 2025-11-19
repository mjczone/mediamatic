// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Processors;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Partial class containing stream-based media processing operations.
/// These operations work for ALL VFS providers (Local, S3, Azure, etc.).
/// </summary>
public abstract partial class VfsMethodsBase
{
    #region Image processing methods

    /// <inheritdoc/>
    public virtual Task<ImageUploadResult> UploadImageAsync(
        IVfsConnection vfs,
        Stream stream,
        string path,
        ImageUploadOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using MimeDetector, MetadataReader, and ImageProcessor
        throw new NotImplementedException($"UploadImageAsync not implemented for {ProviderType}");
    }

    /// <inheritdoc/>
    public virtual Task<ImageProcessingResult> ProcessImageAsync(
        IVfsConnection vfs,
        string sourcePath,
        string destinationPath,
        ImageProcessingOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement using ImageProcessor
        throw new NotImplementedException($"ProcessImageAsync not implemented for {ProviderType}");
    }

    #endregion

    #region Media processor initialization

    /// <summary>
    /// Initializes media processor instances.
    /// </summary>
    partial void InitializeMediaProcessors()
    {
        _mimeDetector = new MimeTypeDetector();
        _metadataReader = new MetadataReader();
        _imageProcessor = new ImageProcessor();
    }

    #endregion
}
