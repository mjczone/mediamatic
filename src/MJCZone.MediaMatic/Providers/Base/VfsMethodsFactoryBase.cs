// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using MJCZone.MediaMatic.Interfaces;

namespace MJCZone.MediaMatic.Providers.Base;

/// <summary>
/// Base class for VFS methods factories.
/// </summary>
public abstract class VfsMethodsFactoryBase : IVfsMethodsFactory
{
    /// <summary>
    /// A thread-safe dictionary to cache database methods by connection type.
    /// </summary>
    private readonly ConcurrentDictionary<Type, IVfsMethods> _methodsCache = new();

    /// <summary>
    /// Gets the VFS methods for a given VFS connection.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <returns>The VFS methods.</returns>
    public virtual IVfsMethods GetMethods(IVfsConnection vfs)
    {
        if (!SupportsConnection(vfs))
        {
            throw new NotSupportedException(
                $"Connection type {vfs.GetType().FullName} is not supported by this factory."
            );
        }

        return _methodsCache.GetOrAdd(vfs.GetType(), _ => CreateMethodsCore());
    }

    /// <summary>
    /// Determines whether the specified VFS connection is supported by this factory.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <returns><c>true</c> if the connection is supported; otherwise, <c>false</c>.</returns>
    public abstract bool SupportsConnection(IVfsConnection vfs);

    /// <summary>
    /// Creates the core vfs methods.
    /// </summary>
    /// <returns>The created vfs methods.</returns>
    protected abstract IVfsMethods CreateMethodsCore();
}
