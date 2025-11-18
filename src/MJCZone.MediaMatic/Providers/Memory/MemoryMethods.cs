// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Memory;

/// <summary>
/// Represents the Memory specific VFS methods.
/// </summary>
public partial class MemoryMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryMethods"/> class.
    /// </summary>
    internal MemoryMethods()
        : base(VfsProviderType.Memory) { }

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;
}
