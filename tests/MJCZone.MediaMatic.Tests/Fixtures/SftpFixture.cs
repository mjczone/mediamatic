// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MJCZone.MediaMatic.Tests.Fixtures;

/// <summary>
/// Fixture for SFTP testcontainer.
/// Shared across all SFTP provider tests using xUnit Collection Fixture.
/// Uses atmoz/sftp Docker image via generic Testcontainers.
/// </summary>
public class SftpFixture : IAsyncLifetime
{
    private IContainer? _container;

    /// <summary>
    /// Gets the SFTP container.
    /// </summary>
    public IContainer Container => _container ?? throw new InvalidOperationException("Container not initialized");

    /// <summary>
    /// Gets the SFTP connection string for this instance.
    /// </summary>
    public string SftpConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the default username.
    /// </summary>
    public const string Username = "basic";

    /// <summary>
    /// Gets the default password.
    /// </summary>
    public const string Password = "password";

    /// <summary>
    /// Gets the SFTP port.
    /// </summary>
    public int SftpPort { get; private set; }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // atmoz/sftp format: "user:password:e:uid:gid:directory"
        // The ':e' flag allows writing to any directory in the user's home
        // uid=1000, gid=1000 are default
        _container = new ContainerBuilder()
            .WithImage("atmoz/sftp:alpine")
            .WithPortBinding(22, true)
            // .WithTmpfsMount("/home/basic/upload") // Use tmpfs for upload directory
            //.WithCommand($"{Username}:{Password}:e:1000:1000")
            .WithCommand($"{Username}:{Password}:::upload")
            .Build();

        await _container.StartAsync();

        // Give SFTP server time to initialize
        await Task.Delay(2000);

        // Get mapped port
        SftpPort = _container.GetMappedPublicPort(22);

        // Build connection string for MediaMatic SFTP provider
        // path=upload changes to the upload directory (relative to user's home)
        // SftpConnectionString = $"sftp://host=127.0.0.1;port={SftpPort};username={Username};password={Password};path=upload";
        SftpConnectionString = $"sftp://host=127.0.0.1;port={SftpPort};username={Username};password={Password};";
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
