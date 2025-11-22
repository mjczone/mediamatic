// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Security;

/// <summary>
/// Interface for implementing custom authorization logic for MediaMatic operations.
/// </summary>
public interface IMediaMaticPermissions
{
    /// <summary>
    /// Determines whether a user is authorized to perform a specific operation.
    /// </summary>
    /// <param name="context">The authorization context containing operation and resource details.</param>
    /// <returns>True if the user is authorized; otherwise, false.</returns>
    Task<bool> IsAuthorizedAsync(IOperationContext context);
}
