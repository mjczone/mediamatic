// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace MJCZone.MediaMatic.AspNetCore.Auditing;

/// <summary>
/// Default implementation of IMediaMaticAuditLogger that uses ILogger.
/// </summary>
public partial class DefaultMediaMaticAuditLogger : IMediaMaticAuditLogger
{
    private readonly ILogger<DefaultMediaMaticAuditLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMediaMaticAuditLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DefaultMediaMaticAuditLogger(ILogger<DefaultMediaMaticAuditLogger> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task LogOperationAsync(MediaMaticAuditEvent auditEvent)
    {
        if (auditEvent.Success)
        {
            LogOperationSuccess(
                _logger,
                auditEvent.Operation,
                auditEvent.UserIdentifier,
                auditEvent.FilesourceId ?? "N/A"
            );
        }
        else
        {
            LogOperationFailure(
                _logger,
                auditEvent.Operation,
                auditEvent.UserIdentifier,
                auditEvent.FilesourceId ?? "N/A",
                auditEvent.Message ?? "Unknown error"
            );
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "MediaMatic Operation: {Operation} by {User} on {Filesource} - Success"
    )]
    private static partial void LogOperationSuccess(ILogger logger, string operation, string user, string filesource);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "MediaMatic Operation: {Operation} by {User} on {Filesource} - Failed: {Error}"
    )]
    private static partial void LogOperationFailure(
        ILogger logger,
        string operation,
        string user,
        string filesource,
        string error
    );
}
