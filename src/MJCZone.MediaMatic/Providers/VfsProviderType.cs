// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic;

/// <summary>
/// Represents the type of a virtual file system provider.
/// </summary>
public enum VfsProviderType
{
    /// <summary>
    /// In memory virtual file system.
    /// </summary>
    Memory,

    /// <summary>
    /// Local file system.
    /// </summary>
    Local,

    /// <summary>
    /// ZipFile archive file system.
    /// </summary>
    ZipFile,

    /// <summary>
    /// Amazon S3 file system.
    /// </summary>
    S3,

    /// <summary>
    /// SFTP file system.
    /// </summary>
    SFTP,

    /// <summary>
    /// Google Cloud Platform file system.
    /// </summary>
    GCP,

    /// <summary>
    /// B2 Cloud file system.
    /// </summary>
    B2,

    /// <summary>
    /// Minio Cloud file system.
    /// </summary>
    Minio,

    /// <summary>
    /// Other or custom file system.
    /// </summary>
    Other,
}
