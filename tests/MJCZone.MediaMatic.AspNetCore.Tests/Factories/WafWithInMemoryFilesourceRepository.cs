// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Security;

namespace MJCZone.MediaMatic.AspNetCore.Tests.Factories;

/// <summary>
/// WebApplicationFactory for MediaMatic tests with in-memory filesource repository.
/// </summary>
public class WafWithInMemoryFilesourceRepository(IReadOnlyList<FilesourceDto> filesources)
    : WebApplicationFactory<Program>
{
    private static readonly string EncryptionKey = CryptoUtils.GenerateEncryptionKey();

    private readonly List<FilesourceDto>? _testFilesources = [.. filesources];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure options for test-specific filesources
            services.Configure<MediaMaticOptions>(options =>
            {
                options.ConnectionStringEncryptionKey = EncryptionKey;

                if (_testFilesources != null && _testFilesources.Count != 0)
                {
                    options.Filesources.AddRange(_testFilesources);
                }
            });
        });

        builder.UseEnvironment("Testing");

        // Suppress logging noise during tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }
}
