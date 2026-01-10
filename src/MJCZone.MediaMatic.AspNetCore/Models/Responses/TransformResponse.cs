// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.AspNetCore.Models.Dtos;

/// <summary>
/// Response containing results from a transform operation.
/// </summary>
public sealed class TransformResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformResponse"/> class.
    /// </summary>
    public TransformResponse()
    {
        Result = new TransformResultDto();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformResponse"/> class.
    /// </summary>
    /// <param name="result">The transformation result data.</param>
    public TransformResponse(TransformResultDto result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets or sets the transformation result data.
    /// </summary>
    public TransformResultDto? Result { get; set; }
}
