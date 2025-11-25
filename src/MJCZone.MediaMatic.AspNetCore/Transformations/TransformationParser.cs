// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.Models;

namespace MJCZone.MediaMatic.AspNetCore.Transformations;

/// <summary>
/// Parses URL transformation parameters into processing options.
/// </summary>
public static class TransformationParser
{
    /// <summary>
    /// Parses a transformation string into image processing options.
    /// </summary>
    /// <param name="transformations">Comma-separated transformation parameters (e.g., "w_400,h_300,c_fill,q_80,f_webp").</param>
    /// <returns>Parsed transformation options.</returns>
    /// <exception cref="ArgumentException">Thrown when transformation syntax is invalid.</exception>
    public static TransformationOptions Parse(string transformations)
    {
        if (string.IsNullOrWhiteSpace(transformations))
        {
            throw new ArgumentException("Transformation string cannot be empty", nameof(transformations));
        }

        var options = new TransformationOptions();
        var parts = transformations.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var keyValue = part.Split('_', 2);
            if (keyValue.Length != 2)
            {
                throw new ArgumentException($"Invalid transformation parameter: {part}. Expected format: key_value");
            }

            var key = keyValue[0].Trim().ToLowerInvariant();
            var value = keyValue[1].Trim();

            try
            {
                switch (key)
                {
                    case "w":
                        options.Width = ParseInt(value, "width");
                        break;

                    case "h":
                        options.Height = ParseInt(value, "height");
                        break;

                    case "ar":
                        options.AspectRatio = ParseAspectRatio(value);
                        break;

                    case "dpr":
                        options.DevicePixelRatio = ParseFloat(value, "device pixel ratio");
                        break;

                    case "c":
                        options.CropMode = ParseCropMode(value);
                        break;

                    case "g":
                        options.Gravity = ParseGravity(value);
                        break;

                    case "x":
                        options.FocalPointX = ParseFloat(value, "focal point X", 0f, 1f);
                        break;

                    case "y":
                        options.FocalPointY = ParseFloat(value, "focal point Y", 0f, 1f);
                        break;

                    case "q":
                        options.Quality = ParseQuality(value);
                        break;

                    case "f":
                        options.Format = ParseFormat(value);
                        break;

                    case "bg":
                        options.BackgroundColor = ParseBackgroundColor(value);
                        break;

                    case "m":
                        options.PreserveMetadata = ParseMetadataOption(value);
                        break;

                    case "v":
                        options.Version = value;
                        break;

                    case "e":
                        // Effects - future implementation
                        // options.Effects.Add(ParseEffect(value));
                        break;

                    default:
                        throw new ArgumentException($"Unknown transformation parameter: {key}");
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException($"Error parsing transformation parameter '{part}': {ex.Message}", ex);
            }
        }

        // Post-processing: Apply DPR to dimensions
        if (options.DevicePixelRatio.HasValue && options.DevicePixelRatio.Value != 1f)
        {
            if (options.Width.HasValue)
            {
                options.Width = (int)(options.Width.Value * options.DevicePixelRatio.Value);
            }

            if (options.Height.HasValue)
            {
                options.Height = (int)(options.Height.Value * options.DevicePixelRatio.Value);
            }
        }

        // Post-processing: Calculate missing dimension from aspect ratio
        if (options.AspectRatio.HasValue)
        {
            if (options.Width.HasValue && !options.Height.HasValue)
            {
                options.Height = (int)(options.Width.Value / options.AspectRatio.Value);
            }
            else if (options.Height.HasValue && !options.Width.HasValue)
            {
                options.Width = (int)(options.Height.Value * options.AspectRatio.Value);
            }
        }

        // Post-processing: Build FocalPoint if custom gravity
        if (options.Gravity == GravityMode.Custom && (options.FocalPointX.HasValue || options.FocalPointY.HasValue))
        {
            options.FocalPoint = new FocalPoint { X = options.FocalPointX ?? 0.5f, Y = options.FocalPointY ?? 0.5f };
        }

        return options;
    }

    private static int ParseInt(string value, string paramName)
    {
        if (!int.TryParse(value, out var result) || result <= 0)
        {
            throw new ArgumentException($"Invalid {paramName}: {value}. Must be a positive integer.");
        }

        return result;
    }

    private static float ParseFloat(string value, string paramName, float? min = null, float? max = null)
    {
        if (!float.TryParse(value, out var result))
        {
            throw new ArgumentException($"Invalid {paramName}: {value}. Must be a number.");
        }

        if (min.HasValue && result < min.Value)
        {
            throw new ArgumentException($"Invalid {paramName}: {value}. Must be >= {min.Value}.");
        }

        if (max.HasValue && result > max.Value)
        {
            throw new ArgumentException($"Invalid {paramName}: {value}. Must be <= {max.Value}.");
        }

        return result;
    }

    private static float ParseAspectRatio(string value)
    {
        // Support both "16:9" and "1.77" formats
        if (value.Contains(':', StringComparison.Ordinal))
        {
            var parts = value.Split(':', 2);
            if (
                parts.Length != 2
                || !float.TryParse(parts[0], out var width)
                || !float.TryParse(parts[1], out var height)
                || height == 0
            )
            {
                throw new ArgumentException($"Invalid aspect ratio: {value}. Expected format: '16:9' or '1.77'.");
            }

            return width / height;
        }
        else
        {
            if (!float.TryParse(value, out var ratio) || ratio <= 0)
            {
                throw new ArgumentException($"Invalid aspect ratio: {value}. Must be a positive number.");
            }

            return ratio;
        }
    }

    private static ResizeMode ParseCropMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "fit" or "scale" => ResizeMode.Fit,
            "fill" or "cover" or "thumb" => ResizeMode.Cover,
            "pad" => ResizeMode.Pad,
            "stretch" => ResizeMode.Stretch,
            _ => throw new ArgumentException(
                $"Invalid crop mode: {value}. Valid values: fit, fill, cover, pad, stretch, scale, thumb."
            ),
        };
    }

    private static GravityMode ParseGravity(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "center" => GravityMode.Center,
            "north" => GravityMode.North,
            "south" => GravityMode.South,
            "east" => GravityMode.East,
            "west" => GravityMode.West,
            "northeast" => GravityMode.NorthEast,
            "northwest" => GravityMode.NorthWest,
            "southeast" => GravityMode.SouthEast,
            "southwest" => GravityMode.SouthWest,
            "xy" => GravityMode.Custom,
            _ => throw new ArgumentException(
                $"Invalid gravity: {value}. Valid values: center, north, south, east, west, northeast, northwest, southeast, southwest, xy."
            ),
        };
    }

    private static int? ParseQuality(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "auto" => null, // Auto quality - let service decide based on format
            "best" => 100,
            "good" => 85,
            "eco" => 70,
            _ => ParseInt(value, "quality") is var q && q >= 0 && q <= 100
                ? q
                : throw new ArgumentException(
                    $"Invalid quality: {value}. Must be 0-100 or one of: auto, best, good, eco."
                ),
        };
    }

    private static ImageFormatOption ParseFormat(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "auto" => ImageFormatOption.Auto,
            "jpeg" or "jpg" => ImageFormatOption.Jpeg,
            "png" => ImageFormatOption.Png,
            "webp" => ImageFormatOption.WebP,
            "avif" => ImageFormatOption.Avif,
            "bmp" => ImageFormatOption.Bmp,
            "gif" => ImageFormatOption.Gif,
            _ => throw new ArgumentException(
                $"Invalid format: {value}. Valid values: auto, jpeg, jpg, png, webp, avif, bmp, gif."
            ),
        };
    }

    private static string ParseBackgroundColor(string value)
    {
        if (value.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            return "transparent";
        }

        // Validate hex color format (6 characters, no # prefix)
        if (value.Length != 6 || !value.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
        {
            throw new ArgumentException(
                $"Invalid background color: {value}. Expected 6-character hex RGB (e.g., 'ffffff') or 'transparent'."
            );
        }

        return $"#{value}";
    }

    private static bool ParseMetadataOption(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "keep" => true,
            "strip" => false,
            _ => throw new ArgumentException($"Invalid metadata option: {value}. Valid values: keep, strip."),
        };
    }
}
