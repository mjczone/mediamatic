// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Models;

/// <summary>
/// Represents a virtual file in the MediaMatic system.
/// </summary>
[Serializable]
public class VFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VFile"/> class.
    /// Used for deserialization.
    /// </summary>
    public VFile() { }

    /// <summary>
    /// Gets or sets the name of the virtual file.
    /// </summary>
    public string Name { get; set; } = default!;
}
