// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Local;

/// <summary>
/// Represents the Local specific VFS methods.
/// </summary>
public partial class LocalMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMethods"/> class.
    /// </summary>
    internal LocalMethods()
        : base(VfsProviderType.Local) { }

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;
}
