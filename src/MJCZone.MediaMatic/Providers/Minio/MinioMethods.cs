// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.Minio;

/// <summary>
/// Represents the Minio specific VFS methods.
/// </summary>
public partial class MinioMethods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MinioMethods"/> class.
    /// </summary>
    internal MinioMethods()
        : base(VfsProviderType.Minio) { }

    /// <inheritdoc/>
    public override bool SupportsBuckets => true;

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;
}
