// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.FTP;

/// <summary>
/// Represents the FTP specific VFS methods.
/// </summary>
public partial class FTPMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FTPMethods"/> class.
    /// </summary>
    internal FTPMethods()
        : base(VfsProviderType.FTP) { }

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;
}
