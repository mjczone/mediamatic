// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Options for browse operations.
/// </summary>
public sealed class BrowseOptions
{
    /// <summary>
    /// Gets or sets the type of content to list.
    /// </summary>
    public BrowseType Type { get; set; } = BrowseType.All;

    /// <summary>
    /// Gets or sets the wildcard filter pattern (e.g., "*.pdf", "*report*.xlsx").
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to list files recursively.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Gets or sets the fields to include in the response.
    /// If null or empty, all fields are included.
    /// </summary>
    public IEnumerable<string>? Fields { get; set; }

    /// <summary>
    /// Checks if a specific field should be included in the response.
    /// </summary>
    /// <param name="fieldName">The field name to check.</param>
    /// <returns>True if the field should be included.</returns>
    public bool IncludeField(string fieldName)
    {
        // If no fields specified, include all
        if (Fields == null || !Fields.Any())
        {
            return true;
        }

        return Fields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Type of content to list in browse operations.
/// </summary>
public enum BrowseType
{
    /// <summary>
    /// List both files and folders.
    /// </summary>
    All,

    /// <summary>
    /// List only files.
    /// </summary>
    Files,

    /// <summary>
    /// List only folders.
    /// </summary>
    Folders,
}
