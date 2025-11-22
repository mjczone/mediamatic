// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using MJCZone.MediaMatic.AspNetCore.Models.Dtos;

namespace MJCZone.MediaMatic.AspNetCore.Factories;

/// <summary>
/// Factory for generating filesource IDs based on request data.
/// </summary>
public interface IFilesourceIdFactory
{
    /// <summary>
    /// Generates a filesource ID based on the provided request.
    /// </summary>
    /// <param name="filesource">The request containing filesource details.</param>
    /// <returns>A unique filesource ID.</returns>
    string GenerateId(FilesourceDto filesource);
}
