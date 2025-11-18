// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using FluentStorage.Blobs;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Represents a connection to a virtual file system.
/// </summary>
public abstract class VfsConnectionBase : IVfsConnection
{
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="VfsConnectionBase"/> class.
    /// </summary>
    /// <param name="providerType">The provider type for the virtual file system.</param>
    /// <param name="blobStorage">The blob storage for the virtual file system.</param>
    protected VfsConnectionBase(VfsProviderType providerType, IBlobStorage blobStorage)
    {
        ProviderType = providerType;
        BlobStorage = blobStorage;
    }

    /// <summary>
    /// Gets the provider type for the virtual file system.
    /// </summary>
    public VfsProviderType ProviderType { get; }

    /// <summary>
    /// Gets the connection string for the virtual file system.
    /// </summary>
    public abstract string ConnectionString { get; }

    /// <summary>
    /// Gets the blob storage for the virtual file system.
    /// </summary>
    internal IBlobStorage BlobStorage { get; }

    #region IDisposable Support

    /// <summary>
    /// Disposes the resources used by the <see cref="VfsConnectionBase"/> class.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="VfsConnectionBase"/> class.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                BlobStorage.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~VfsConnection()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    #endregion // IDisposable Support
}
