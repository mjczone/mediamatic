// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Tests.TestHelpers;

/// <summary>
/// A custom Fact attribute that skips the test if FFmpeg is not installed on the system.
/// </summary>
public sealed class SkipIfNoFfmpegFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkipIfNoFfmpegFactAttribute"/> class.
    /// </summary>
    public SkipIfNoFfmpegFactAttribute()
    {
        if (!TestDataHelper.IsFfmpegAvailable())
        {
            Skip = "FFmpeg is not installed on this system.";
        }
    }
}

/// <summary>
/// A custom Theory attribute that skips the test if FFmpeg is not installed on the system.
/// </summary>
public sealed class SkipIfNoFfmpegTheoryAttribute : TheoryAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkipIfNoFfmpegTheoryAttribute"/> class.
    /// </summary>
    public SkipIfNoFfmpegTheoryAttribute()
    {
        if (!TestDataHelper.IsFfmpegAvailable())
        {
            Skip = "FFmpeg is not installed on this system.";
        }
    }
}
