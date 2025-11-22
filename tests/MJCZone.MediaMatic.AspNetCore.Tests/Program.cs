// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory }
);

// Add MediaMatic services
builder.Services.AddMediaMatic();

var app = builder.Build();

app.UseRouting();

// Map MediaMatic endpoints
app.UseMediaMatic();

app.Run();

/// <summary>
/// Test application program for WebApplicationFactory.
/// </summary>
public partial class Program
{
    // This class is used by WebApplicationFactory for testing
}
