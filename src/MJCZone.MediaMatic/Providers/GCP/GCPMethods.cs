// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.GCP;

/// <summary>
/// Represents the GCP specific VFS methods.
/// </summary>
public partial class GCPMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GCPMethods"/> class.
    /// </summary>
    internal GCPMethods()
        : base(VfsProviderType.GCP) { }

    /// <inheritdoc/>
    public override bool SupportsBuckets => true;

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;
}
