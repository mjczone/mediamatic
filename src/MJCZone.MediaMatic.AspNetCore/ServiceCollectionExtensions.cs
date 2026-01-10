// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MJCZone.DapperMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Auditing;
using MJCZone.MediaMatic.AspNetCore.Factories;
using MJCZone.MediaMatic.AspNetCore.Repositories;
using MJCZone.MediaMatic.AspNetCore.Security;
using MJCZone.MediaMatic.AspNetCore.Services;
using MJCZone.MediaMatic.Processors;

namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Extension methods for registering MediaMatic services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MediaMatic ASP.NET Core services with configuration to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configure">Configuration action for the MediaMatic configuration builder.</param>
    /// <returns>The service collection for further configuration.</returns>
    public static IServiceCollection AddMediaMatic(
        this IServiceCollection services,
        Action<MediaMaticConfigurationBuilder>? configure = null
    )
    {
        services.AddScoped<IMediaMaticService, MediaMaticService>();

        // Register operation context services
        services.TryAddScoped<IOperationContext, OperationContext>();
        services.TryAddScoped<IOperationContextInitializer, OperationContextInitializer>();

        // Apply fluent configuration
        if (configure != null)
        {
            var builder = new MediaMaticConfigurationBuilder(services);
            configure.Invoke(builder);
        }

        // Register defaults (do this AFTER the fluent configuration to allow overrides by the user)

        // Register the connection factory for the database repository
        services.TryAddSingleton<IDbConnectionFactory, DbConnectionFactory>(); // Uses DapperMatic's DbConnectionProviderDetector internally
        services.TryAddSingleton<IVfsConnectionFactory, VfsConnectionFactory>();
        services.TryAddSingleton<IFilesourceIdFactory, GuidFilesourceIdFactory>();
        services.TryAddSingleton<IMediaMaticPermissions, DefaultMediaMaticPermissions>();
        services.TryAddSingleton<IMediaMaticAuditLogger, DefaultMediaMaticAuditLogger>();
        services.TryAddSingleton<IImageProcessor, ImageProcessor>();

        // Register default in-memory repository if no repository was explicitly configured
        // This ensures that filesources added via configuration or fluent API are captured
        RegisterFilesourceRepositoryIfNotRegistered(services);

        return services;
    }

    /// <summary>
    /// Registers the default in-memory filesource repository only if no repository was explicitly registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void RegisterFilesourceRepositoryIfNotRegistered(IServiceCollection services)
    {
        // Check if repository is already registered
        var hasRepository = services.Any(sd => sd.ServiceType == typeof(IMediaMaticFilesourceRepository));

        if (!hasRepository)
        {
            RegisterFilesourceRepository(services);
        }
    }

    /// <summary>
    /// Registers the filesource repository with deferred initialization to capture all configured filesources.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void RegisterFilesourceRepository(IServiceCollection services)
    {
        services.TryAddSingleton<IMediaMaticFilesourceRepository>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaMaticOptions>>();
            var filesourceIdFactory = serviceProvider.GetRequiredService<IFilesourceIdFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<InMemoryMediaMaticFilesourceRepository>>();
            var repository = new InMemoryMediaMaticFilesourceRepository(options, filesourceIdFactory, logger);

            // Get final options configuration (includes both direct config and fluent API additions)
            if (options?.Value?.Filesources?.Count > 0)
            {
                foreach (var filesource in options.Value.Filesources)
                {
                    _ = repository.AddFilesourceAsync(filesource).GetAwaiter().GetResult();
                }
            }

            return repository;
        });
    }
}
