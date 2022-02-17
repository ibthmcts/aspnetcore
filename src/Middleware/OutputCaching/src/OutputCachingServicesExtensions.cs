// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for the OutputCaching middleware.
/// </summary>
public static class OutputCachingServicesExtensions
{
    /// <summary>
    /// Add output caching services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
    /// <returns></returns>
    public static IServiceCollection AddOutputCaching(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        return services;
    }

    /// <summary>
    /// Add output caching services and configure the related options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="OutputCachingOptions"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddOutputCaching(this IServiceCollection services, Action<OutputCachingOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        services.Configure(configureOptions);
        services.AddOutputCaching();

        return services;
    }
}
