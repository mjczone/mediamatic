// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Factories;

/// <summary>
/// Factory for generating filesource IDs using GUIDs.
/// </summary>
public class GuidFilesourceIdFactory : IFilesourceIdFactory
{
    /// <inheritdoc />
    public string GenerateId(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        if (!string.IsNullOrWhiteSpace(filesource.Id) && Guid.TryParse(filesource.Id, out var uid) && uid != Guid.Empty)
        {
            return uid.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
