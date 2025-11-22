// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Options;

namespace MJCZone.MediaMatic.AspNetCore.Security;

/// <summary>
/// Default implementation of IMediaMaticPermissions that allows all users, all authenticated user, or users with a specific role.
/// </summary>
public class DefaultMediaMaticPermissions : IMediaMaticPermissions
{
    private readonly PermissionDefault _defaultBehavior;
    private readonly string? _requireRole;
    private readonly string? _readOnlyRole;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMediaMaticPermissions"/> class.
    /// </summary>
    /// <param name="options">The MediaMatic options.</param>
    /// <param name="defaultBehavior">The default permission behavior.</param>
    public DefaultMediaMaticPermissions(
        IOptions<MediaMaticOptions> options,
        PermissionDefault defaultBehavior = PermissionDefault.AllowAll
    )
    {
        _requireRole = options.Value.RequireRole;
        _readOnlyRole = options.Value.ReadOnlyRole;
        _defaultBehavior = string.IsNullOrWhiteSpace(_requireRole) ? defaultBehavior : PermissionDefault.RequireRole;
    }

    /// <inheritdoc />
    public Task<bool> IsAuthorizedAsync(IOperationContext context)
    {
        if (_defaultBehavior == PermissionDefault.AllowAll)
        {
            return Task.FromResult(true);
        }

        var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
        if (_defaultBehavior == PermissionDefault.RequireAuthentication)
        {
            return Task.FromResult(isAuthenticated);
        }

        if (_defaultBehavior == PermissionDefault.RequireRole)
        {
            // If not authenticated, no need to check roles
            if (!isAuthenticated)
            {
                return Task.FromResult(false);
            }

            var isGetRequest =
                (context.HttpMethod ?? string.Empty).Equals("GET", StringComparison.OrdinalIgnoreCase)
                || context.Operation?.EndsWith("/get", StringComparison.OrdinalIgnoreCase) == true;

            if (isGetRequest && !string.IsNullOrEmpty(_readOnlyRole) && context.User?.IsInRole(_readOnlyRole) == true)
            {
                return Task.FromResult(true);
            }

            if (!string.IsNullOrWhiteSpace(_requireRole) && context.User?.IsInRole(_requireRole) == true)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}

/// <summary>
/// Specifies the default permission behavior.
/// </summary>
public enum PermissionDefault
{
    /// <summary>
    /// Allow all operations.
    /// </summary>
    AllowAll,

    /// <summary>
    /// Require authentication for all operations.
    /// </summary>
    RequireAuthentication,

    /// <summary>
    /// Require a specific role for all operations.
    /// </summary>
    RequireRole,
}
