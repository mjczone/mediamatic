// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.ZipFile;

/// <summary>
/// Represents the Zip specific VFS methods.
/// </summary>
public partial class ZipFileMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileMethods"/> class.
    /// </summary>
    internal ZipFileMethods()
        : base(VfsProviderType.ZipFile) { }

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;
}
