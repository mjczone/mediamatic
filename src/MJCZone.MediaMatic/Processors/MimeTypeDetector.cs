// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Detects MIME types from file content using Mime-Detective library.
/// </summary>
public class MimeTypeDetector : IMimeTypeDetector
{
    /// <inheritdoc/>
    public Task<string?> DetectMimeTypeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // TODO: Implement using Mime-Detective 25.8.1 API
        // Note: API uses ReadOnlySpan<byte> instead of Stream
        throw new NotImplementedException("MimeTypeDetector not yet implemented");
    }
}
