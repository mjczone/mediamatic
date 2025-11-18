// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.B2;

/// <summary>
/// Represents the B2 specific VFS methods.
/// </summary>
public partial class B2Methods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="B2Methods"/> class.
    /// </summary>
    internal B2Methods()
        : base(VfsProviderType.B2) { }

    /// <inheritdoc/>
    public override bool SupportsBuckets => true;

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;
}
