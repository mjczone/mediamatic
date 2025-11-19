// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Provides MIME type detection from file content.
/// </summary>
public interface IMimeTypeDetector
{
    /// <summary>
    /// Detects the MIME type from a stream by inspecting file content.
    /// </summary>
    /// <param name="stream">The stream to inspect. Position will be reset after detection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detected MIME type, or null if detection failed.</returns>
    Task<string?> DetectMimeTypeAsync(Stream stream, CancellationToken cancellationToken = default);
}
