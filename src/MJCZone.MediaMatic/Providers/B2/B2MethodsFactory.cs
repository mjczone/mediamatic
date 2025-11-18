// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.B2;

/// <summary>
///  Provides B2 specific vfs methods.
/// </summary>
public class B2MethodsFactory : VfsMethodsFactoryBase
{
    /// <summary>
    ///  Determines if the factory supports the given connection.
    /// </summary>
    /// <param name="vfs">The VFS connection.</param>
    /// <returns>True if supported; otherwise false.</returns>
    public override bool SupportsConnection(IVfsConnection vfs)
    {
        return vfs is B2VfsConnection;
    }

    /// <summary>
    ///  Creates the VFS methods.
    /// </summary>
    /// <returns>The VFS methods.</returns>
    protected override IVfsMethods CreateMethodsCore()
    {
        return new B2Methods();
    }
}
