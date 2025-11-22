// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Auditing;

/// <summary>
/// Interface for logging MediaMatic operations for audit purposes.
/// </summary>
public interface IMediaMaticAuditLogger
{
    /// <summary>
    /// Logs a MediaMatic operation for audit purposes.
    /// </summary>
    /// <param name="auditEvent">The audit event details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogOperationAsync(MediaMaticAuditEvent auditEvent);
}
