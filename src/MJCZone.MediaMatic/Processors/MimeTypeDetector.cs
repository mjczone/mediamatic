// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MimeDetective;

namespace MJCZone.MediaMatic.Processors;

/// <summary>
/// Detects MIME types from file content using Mime-Detective library.
/// </summary>
public class MimeTypeDetector : IMimeTypeDetector
{
    private static readonly Lazy<IContentInspector> Inspector = new(() =>
        new ContentInspectorBuilder() { Definitions = MimeDetective.Definitions.DefaultDefinitions.All() }.Build()
    );

    /// <inheritdoc/>
    public async Task<string?> DetectMimeTypeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must be readable", nameof(stream));
        }

        // Save original position to restore later
        var originalPosition = stream.CanSeek ? stream.Position : -1;

        try
        {
            // Reset stream to beginning if possible
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Position = 0;
            }

            // Read up to 560 bytes for MIME detection (default buffer size for Mime-Detective)
            const int bufferSize = 560;
            var buffer = new byte[bufferSize];
            var bytesRead = await stream
                .ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken)
                .ConfigureAwait(false);

            if (bytesRead == 0)
            {
                return null; // Empty stream
            }

            // Inspect the bytes using Mime-Detective
            var results = Inspector.Value.Inspect(buffer.AsSpan(0, bytesRead));

            // Get the first MIME type match
            var match = results.ByMimeType().FirstOrDefault();

            return match?.MimeType;
        }
        finally
        {
            // Restore original stream position if stream is seekable
            if (stream.CanSeek && originalPosition >= 0)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
