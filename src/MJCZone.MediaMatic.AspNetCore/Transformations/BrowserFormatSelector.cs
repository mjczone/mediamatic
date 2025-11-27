// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using DeviceDetectorNET;
using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.AspNetCore.Transformations;

/// <summary>
/// Selects optimal image format based on browser capabilities.
/// </summary>
public static class BrowserFormatSelector
{
    /// <summary>
    /// Maximum number of User-Agent strings to cache to prevent unbounded memory growth.
    /// </summary>
    private const int MaxCacheSize = 1000;

    /// <summary>
    /// Cache of User-Agent to AVIF support result.
    /// Since browser capabilities are immutable, results can be cached indefinitely.
    /// </summary>
    private static readonly ConcurrentDictionary<string, bool> AvifSupportCache = new();

    /// <summary>
    /// Selects the optimal image format based on browser capabilities.
    /// </summary>
    /// <param name="userAgent">User-Agent header from the HTTP request.</param>
    /// <param name="acceptHeader">Accept header from the HTTP request.</param>
    /// <param name="originalFormat">The original image format (fallback).</param>
    /// <returns>The optimal image format to serve.</returns>
    public static ImageFormat SelectOptimalFormat(
        string? userAgent,
        string? acceptHeader,
        ImageFormat originalFormat = ImageFormat.Jpeg
    )
    {
        // Priority: AVIF > WebP > original format

        // Check Accept header first (most reliable)
        var accept = acceptHeader?.ToLowerInvariant() ?? string.Empty;

        // Check for AVIF support
        if (accept.Contains("image/avif", StringComparison.OrdinalIgnoreCase))
        {
            // Verify browser version supports AVIF properly
            if (BrowserSupportsAvif(userAgent))
            {
                return ImageFormat.Avif;
            }
        }

        // Check for WebP support
        if (accept.Contains("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            return ImageFormat.WebP;
        }

        // Fallback to original format or JPEG
        return originalFormat;
    }

    /// <summary>
    /// Determines if the browser properly supports AVIF.
    /// Results are cached by User-Agent string for performance.
    /// </summary>
    /// <param name="userAgent">User-Agent header.</param>
    /// <returns>True if the browser supports AVIF.</returns>
    private static bool BrowserSupportsAvif(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return false;
        }

        // Check cache first
        if (AvifSupportCache.TryGetValue(userAgent, out var cached))
        {
            return cached;
        }

        // Parse User-Agent and determine AVIF support
        var result = ParseBrowserSupportsAvif(userAgent);

        // Cache result if under size limit (simple eviction: stop caching when full)
        if (AvifSupportCache.Count < MaxCacheSize)
        {
            AvifSupportCache.TryAdd(userAgent, result);
        }

        return result;
    }

    /// <summary>
    /// Parses the User-Agent to determine AVIF support.
    /// </summary>
    private static bool ParseBrowserSupportsAvif(string userAgent)
    {
        try
        {
            var detector = new DeviceDetector(userAgent);
            detector.Parse();

            var client = detector.GetClient();
            if (client.Success && client.Match != null)
            {
                var browserName = client.Match.Name?.ToLowerInvariant() ?? string.Empty;
                var version = client.Match.Version;

                // AVIF browser support:
                // Chrome/Edge: 85+
                // Firefox: 93+
                // Safari: 16+
                // Opera: 71+

                if (!string.IsNullOrEmpty(version) && TryParseVersion(version, out var majorVersion))
                {
                    return browserName switch
                    {
                        "chrome" or "chromium" or "edge" or "chrome mobile" or "edge mobile" => majorVersion >= 85,
                        "firefox" or "firefox mobile" => majorVersion >= 93,
                        "safari" or "mobile safari" => majorVersion >= 16,
                        "opera" or "opera mobile" => majorVersion >= 71,
                        _ => false,
                    };
                }
            }
        }
        catch
        {
            // If detection fails, fall back to WebP or original
            return false;
        }

        return false;
    }

    private static bool TryParseVersion(string version, out int majorVersion)
    {
        majorVersion = 0;

        // Extract major version from strings like "85.0.4183.102" or "85"
        var parts = version.Split('.', '-', '_');
        if (parts.Length > 0 && int.TryParse(parts[0], out var parsed))
        {
            majorVersion = parsed;
            return true;
        }

        return false;
    }
}
