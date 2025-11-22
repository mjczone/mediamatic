// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MJCZone.MediaMatic.AspNetCore.Auditing;
using MJCZone.MediaMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Models.Dtos;
using MJCZone.MediaMatic.AspNetCore.Repositories;
using MJCZone.MediaMatic.AspNetCore.Security;

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Fluent configuration builder for MediaMatic services.
/// </summary>
public sealed class MediaMaticConfigurationBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMaticConfigurationBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    internal MediaMaticConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Adds a single filesource to the configuration.
    /// </summary>
    /// <param name="filesource">The filesource to add.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder WithFilesource(FilesourceDto filesource)
    {
        ArgumentNullException.ThrowIfNull(filesource);

        _services.Configure<MediaMaticOptions>(options => options.Filesources.Add(filesource));

        return this;
    }

    /// <summary>
    /// Adds multiple filesources to the configuration.
    /// </summary>
    /// <param name="filesources">The filesources to add.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder WithFilesources(params FilesourceDto[] filesources)
    {
        ArgumentNullException.ThrowIfNull(filesources);

        _services.Configure<MediaMaticOptions>(options => options.Filesources.AddRange(filesources));

        return this;
    }

    /// <summary>
    /// Adds multiple filesources to the configuration.
    /// </summary>
    /// <param name="filesources">The filesources to add.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder WithFilesources(IEnumerable<FilesourceDto> filesources)
    {
        ArgumentNullException.ThrowIfNull(filesources);

        _services.Configure<MediaMaticOptions>(options => options.Filesources.AddRange(filesources));

        return this;
    }

    #region FilesourceId Factory Configuration

    /// <summary>
    /// Configures MediaMatic to use a custom filesource ID factory implementation.
    /// </summary>
    /// <typeparam name="TFactory">The type of the custom factory implementation.</typeparam>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseCustomFilesourceIdFactory<TFactory>()
        where TFactory : class, IFilesourceIdFactory
    {
        _services.AddSingleton<IFilesourceIdFactory, TFactory>();
        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a custom filesource ID factory implementation.
    /// </summary>
    /// <param name="implementationFactory">A factory function to create the factory instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if implementationFactory is null.</exception>
    public MediaMaticConfigurationBuilder UseCustomFilesourceIdFactory(
        Func<IServiceProvider, IFilesourceIdFactory> implementationFactory
    )
    {
        ArgumentNullException.ThrowIfNull(implementationFactory);
        _services.AddSingleton(implementationFactory);
        return this;
    }
    #endregion

    #region Permissions Configuration

    /// <summary>
    /// Configures MediaMatic to use a custom permissions implementation.
    /// </summary>
    /// <typeparam name="TPermissions">The type of the custom permissions implementation.</typeparam>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseCustomPermissions<TPermissions>()
        where TPermissions : class, IMediaMaticPermissions
    {
        _services.AddSingleton<IMediaMaticPermissions, TPermissions>();
        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a custom permissions implementation.
    /// </summary>
    /// <param name="implementationFactory">A factory function to create the permissions instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if implementationFactory is null.</exception>
    public MediaMaticConfigurationBuilder UseCustomPermissions(
        Func<IServiceProvider, IMediaMaticPermissions> implementationFactory
    )
    {
        ArgumentNullException.ThrowIfNull(implementationFactory);
        _services.AddSingleton(implementationFactory);
        return this;
    }
    #endregion // Permissions Configuration

    #region Audit Logger Configuration

    /// <summary>
    /// Configures MediaMatic to use a custom audit logger implementation.
    /// </summary>
    /// <typeparam name="TAuditLogger">The type of the custom audit logger implementation.</typeparam>
    /// <returns>The configuration builder for method chaining.</returns>
    /// <remarks>
    /// The custom audit logger must implement <see cref="IMediaMaticAuditLogger"/>.
    /// </remarks>
    public MediaMaticConfigurationBuilder UseCustomAuditLogger<TAuditLogger>()
        where TAuditLogger : class, IMediaMaticAuditLogger
    {
        _services.AddSingleton<IMediaMaticAuditLogger, TAuditLogger>();
        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a custom audit logger implementation.
    /// </summary>
    /// <param name="implementationFactory">A factory function to create the audit logger instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if implementationFactory is null.</exception>
    /// <remarks>
    /// The custom audit logger must implement <see cref="IMediaMaticAuditLogger"/>.
    /// </remarks>
    public MediaMaticConfigurationBuilder UseCustomAuditLogger(
        Func<IServiceProvider, IMediaMaticAuditLogger> implementationFactory
    )
    {
        ArgumentNullException.ThrowIfNull(implementationFactory);
        _services.AddSingleton(implementationFactory);
        return this;
    }
    #endregion // Audit Logger Configuration

    #region Filesource Repository Configuration

    /// <summary>
    /// Configures MediaMatic to use a file-based filesource repository.
    /// </summary>
    /// <param name="filePath">The path to the JSON file where filesources will be stored.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseFileFilesourceRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        _services.AddSingleton<IMediaMaticFilesourceRepository>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MediaMaticOptions>>();
            var datasourceIdFactory = provider.GetRequiredService<IFilesourceIdFactory>();
            var logger = provider.GetRequiredService<ILogger<FileMediaMaticFilesourceRepository>>();
            var repository = new FileMediaMaticFilesourceRepository(filePath, datasourceIdFactory, options, logger);

            // Initialize with configured filesources from options
            if (options.Value.Filesources?.Count > 0)
            {
                foreach (var filesource in options.Value.Filesources)
                {
                    _ = repository.AddFilesourceAsync(filesource).GetAwaiter().GetResult();
                }
            }

            return repository;
        });

        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a database-based filesource repository.
    /// </summary>
    /// <param name="provider">The database provider for the repository storage.</param>
    /// <param name="connectionString">The connection string for the repository database.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseDatabaseFilesourceRepository(string provider, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _services.AddSingleton<IMediaMaticFilesourceRepository>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaMaticOptions>>();
            var filesourceDbConnectionFactory = serviceProvider.GetRequiredService<IFilesourceDbConnectionFactory>();
            var datasourceIdFactory = serviceProvider.GetRequiredService<IFilesourceIdFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<DatabaseMediaMaticFilesourceRepository>>();
            var repository = new DatabaseMediaMaticFilesourceRepository(
                filesourceDbConnectionFactory,
                datasourceIdFactory,
                options,
                logger
            );
            repository.Initialize();

            // Initialize with configured filesources from options
            if (options.Value.Filesources?.Count > 0)
            {
                foreach (var filesource in options.Value.Filesources)
                {
                    _ = repository.AddFilesourceAsync(filesource).GetAwaiter().GetResult();
                }
            }

            return repository;
        });

        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a custom filesource repository implementation.
    /// </summary>
    /// <typeparam name="TRepository">The type of the custom repository implementation.</typeparam>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseCustomFilesourceRepository<TRepository>()
        where TRepository : class, IMediaMaticFilesourceRepository
    {
        _services.AddSingleton<IMediaMaticFilesourceRepository, TRepository>();
        return this;
    }

    /// <summary>
    /// Configures MediaMatic to use a custom filesource repository implementation.
    /// </summary>
    /// <param name="implementationFactory">A factory function to create the repository instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public MediaMaticConfigurationBuilder UseCustomFilesourceRepository(
        Func<IServiceProvider, IMediaMaticFilesourceRepository> implementationFactory
    )
    {
        ArgumentNullException.ThrowIfNull(implementationFactory);
        _services.AddSingleton(implementationFactory);
        return this;
    }

    #endregion // Filesource Repository Configuration
}
