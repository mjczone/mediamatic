// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FFMpegCore.Arguments;

namespace MJCZone.MediaMatic.Providers;

/// <summary>
/// Utility class for virtual file system providers.
/// </summary>
public static class VfsProviderUtils
{
    /// <summary>
    /// Converts an arbitrary string into a URL-safe slug.
    /// - Lowercases using invariant culture
    /// - Removes diacritics (á -> a, ü -> u, etc.)
    /// - Replaces whitespace and common separators with '-'
    /// - Collapses multiple '-' and trims from both ends
    /// - Keeps only letters, digits, and allowed special characters
    /// - If maxLength is specified and exceeded, trims and appends "...{removedCount}"
    ///   (while keeping total length &lt;= maxLength).
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="allowedSpecialCharacters">
    /// Extra non-alphanumeric characters to allow in the slug.
    /// Defaults to the RFC 3986 unreserved specials: "-._~".
    /// Letters and digits are always allowed.
    /// </param>
    /// <param name="maxLength">
    /// Optional maximum length for the resulting slug. If the slug is longer,
    /// it is truncated and suffixed with "...{removedCount}".
    /// </param>
    /// <returns>URL-safe slug string.</returns>
    public static string GenerateSlug(string? input, string? allowedSpecialCharacters = "-._~", int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Default to RFC 3986 "unreserved" specials
        allowedSpecialCharacters ??= "-._~";

        // Build a set of allowed non-alphanumeric chars for quick lookup
        var allowedSet = new HashSet<char>();
        foreach (var ch in allowedSpecialCharacters)
        {
            if (!char.IsLetterOrDigit(ch))
            {
                allowedSet.Add(ch);
            }
        }

        // Normalize to decompose accents, then strip NonSpacingMark
        string normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        bool previousWasSeparator = false;

        foreach (char rawCh in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(rawCh) == UnicodeCategory.NonSpacingMark)
            {
                continue; // skip combining accents
            }

            char c = char.ToLowerInvariant(rawCh);

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                previousWasSeparator = false;
            }
            else if (char.IsWhiteSpace(c) || c is '-' or '_' or '+' or '.' or '/')
            {
                // Treat common separators as a single '-'
                if (!previousWasSeparator && sb.Length > 0)
                {
                    sb.Append('-');
                    previousWasSeparator = true;
                }
            }
            else if (allowedSet.Contains(c))
            {
                // Keep explicitly allowed special chars
                sb.Append(c);
                previousWasSeparator = false;
            }
            // else: drop anything not allowed
        }

        string slug = sb.ToString().Trim('-');

        // No max length specified → done
        if (maxLength is null || slug.Length <= maxLength.Value)
        {
            return slug;
        }

        int originalLength = slug.Length;
        int toRemove = originalLength - maxLength.Value;

        // Suffix that indicates truncation and how many chars were removed
        string suffix = "..." + toRemove.ToString(CultureInfo.InvariantCulture);
        int max = maxLength.Value;

        // Edge case: maxLength is too small to even hold the suffix fully
        if (suffix.Length >= max)
        {
            // Just return as much of the suffix as fits
            return suffix.Substring(0, max);
        }

        // Trim the slug so that slugPart + suffix fits within maxLength
        int slugPartLength = max - suffix.Length;
        string slugPart = slug.Substring(0, slugPartLength).TrimEnd('-');

        // If trimming dropped everything (e.g., slug was just hyphens in that region),
        // fall back to suffix only (or truncated suffix).
        if (slugPart.Length == 0)
        {
            return suffix.Length <= max ? suffix : suffix.Substring(0, max);
        }

        return slugPart + suffix;
    }

    /// <summary>
    /// Parses a connection string into its components.
    /// </summary>
    /// <param name="connectionString">The connection string to parse.</param>
    /// <param name="expectedPrefixes">Array of expected prefixes (e.g., "disk://").</param>
    /// <returns>Case insensitive dictionary of key-value pairs from the connection string.</returns>
    internal static Dictionary<string, string> ParseConnectionString(string connectionString, string[] expectedPrefixes)
    {
        return ParseConnectionString(connectionString, expectedPrefixes, out _);
    }

    /// <summary>
    /// Parses a connection string into its components. Also ensures that the connection string
    /// has one of the expected prefixes, adding the first expected prefix if missing.
    /// </summary>
    /// <remarks>
    /// If the connection string contains an unexpected prefix, an exception is thrown.
    /// This method is ensuring that the connection string is valid for the expected provider type.
    /// </remarks>
    /// <param name="connectionString">The connection string to parse.</param>
    /// <param name="expectedPrefixes">Array of expected prefixes (e.g., "disk://"). The first expected prefix is used as the primary prefix if none is present.</param>
    /// <param name="normalizedConnectionString">The normalized connection string with the primary prefix ensured.</param>
    /// <returns>Case insensitive dictionary of key-value pairs from the connection string.</returns>
    internal static Dictionary<string, string> ParseConnectionString(
        string connectionString,
        string[] expectedPrefixes,
        out string normalizedConnectionString
    )
    {
        if (expectedPrefixes == null || expectedPrefixes.Length == 0)
        {
            throw new ArgumentException("Expected prefixes cannot be null or empty.", nameof(expectedPrefixes));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        normalizedConnectionString = connectionString;

        // Add prefix if missing
        int prefixEndIndex = connectionString.IndexOf("://", StringComparison.OrdinalIgnoreCase);
        if (prefixEndIndex < 0)
        {
            // No prefix found, add the first expected prefix
            normalizedConnectionString = expectedPrefixes[0] + connectionString;
        }
        else
        {
            // Remove expected prefix if present
            var foundPrefix = false;
            foreach (var prefix in expectedPrefixes)
            {
                if (connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = connectionString[prefix.Length..];
                    normalizedConnectionString = expectedPrefixes[0] + connectionString;
                    foundPrefix = true;
                    break;
                }
            }

            // If a prefix remains, flag as invalid. A prefix is an alpha numeric string (with '.', '-', '_') followed by '://'
            if (!foundPrefix && prefixEndIndex >= 0)
            {
                string prefix = connectionString[..prefixEndIndex];
                if (prefix.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'))
                {
                    throw new ArgumentException(
                        $"Unexpected prefix '{prefix}://' found in connection string.",
                        nameof(connectionString)
                    );
                }
            }
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            // Split only on the first '=' to allow '=' in values
            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();
                result[key] = value;
            }
        }

        return result;
    }
}
