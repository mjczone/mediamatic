// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Maps file extensions to categories for filtering and organization.
/// </summary>
public static class FileCategoryMapper
{
    /// <summary>
    /// Image file category.
    /// </summary>
    public const string Image = "image";

    /// <summary>
    /// Video file category.
    /// </summary>
    public const string Video = "video";

    /// <summary>
    /// Audio file category.
    /// </summary>
    public const string Audio = "audio";

    /// <summary>
    /// Document file category.
    /// </summary>
    public const string Document = "document";

    /// <summary>
    /// Archive file category.
    /// </summary>
    public const string Archive = "archive";

    /// <summary>
    /// Code file category.
    /// </summary>
    public const string Code = "code";

    /// <summary>
    /// Other/unknown file category.
    /// </summary>
    public const string Other = "other";

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".avif",
        ".bmp",
        ".tiff",
        ".tif",
        ".svg",
        ".ico",
        ".heic",
        ".heif",
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".webm",
        ".mov",
        ".avi",
        ".mkv",
        ".wmv",
        ".flv",
        ".m4v",
        ".mpeg",
        ".mpg",
        ".3gp",
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".ogg",
        ".flac",
        ".aac",
        ".wma",
        ".m4a",
        ".opus",
        ".aiff",
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".odt",
        ".ods",
        ".odp",
        ".rtf",
        ".txt",
        ".csv",
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip",
        ".tar",
        ".gz",
        ".rar",
        ".7z",
        ".bz2",
        ".xz",
        ".tar.gz",
        ".tgz",
    };

    private static readonly HashSet<string> CodeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".js",
        ".ts",
        ".jsx",
        ".tsx",
        ".cs",
        ".py",
        ".java",
        ".html",
        ".css",
        ".scss",
        ".less",
        ".json",
        ".xml",
        ".yaml",
        ".yml",
        ".md",
        ".sql",
        ".sh",
        ".bash",
        ".ps1",
        ".rb",
        ".go",
        ".rs",
        ".php",
        ".swift",
        ".kt",
        ".cpp",
        ".c",
        ".h",
    };

    /// <summary>
    /// Gets the category for a file based on its extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without leading dot).</param>
    /// <returns>The file category.</returns>
    public static string GetCategory(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return Other;
        }

        // Ensure extension starts with a dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        if (ImageExtensions.Contains(extension))
        {
            return Image;
        }

        if (VideoExtensions.Contains(extension))
        {
            return Video;
        }

        if (AudioExtensions.Contains(extension))
        {
            return Audio;
        }

        if (DocumentExtensions.Contains(extension))
        {
            return Document;
        }

        if (ArchiveExtensions.Contains(extension))
        {
            return Archive;
        }

        if (CodeExtensions.Contains(extension))
        {
            return Code;
        }

        return Other;
    }

    /// <summary>
    /// Gets all supported categories.
    /// </summary>
    /// <returns>An array of all category names.</returns>
    public static string[] GetAllCategories()
    {
        return [Image, Video, Audio, Document, Archive, Code, Other];
    }
}
