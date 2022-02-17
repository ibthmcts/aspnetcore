// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// Default policy. All requests are cached by default, using the host, scheme, path and query string.
/// </summary>
public class DefaultCacheHeaderPolicy : IOutputCachingRequestPolicy, IOutputCachingResponsePolicy
{
    private (string Host, string Value) _hostCache;
    private (string Scheme, string Value) _schemeCache;

    public Task OnRequestAsync(IOutputCachingContext context)
    {
        context.AttemptResponseCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.IsResponseCacheable = true;

        // Vary by any query by default
        context.CachedVaryByRules.QueryKeys = "*";

        // Vary by path by host:scheme:path default
        var request = context.HttpContext.Request;

        var host = _hostCache;
        if (host.Host != request.Host.Value)
        {
            host = (request.Host.Value, request.Host.Value.ToUpperInvariant());
            _hostCache = host;
        }

        var schemeCache = _schemeCache;
        if (schemeCache.Scheme != request.Scheme)
        {
            schemeCache = (request.Scheme, request.Scheme.ToUpperInvariant());
            _schemeCache = schemeCache;
        }

        context.CachedVaryByRules.VaryByPrefix = new [] { host.Value, schemeCache.Value, request.Path.Value?.ToUpperInvariant() };

        return Task.CompletedTask;
    }

    public Task OnServeFromCacheAsync(IOutputCachingContext context)
    {
        context.IsCacheEntryFresh = true;
        return Task.CompletedTask;
    }

    public Task OnServeResponseAsync(IOutputCachingContext context)
    {
        context.IsResponseCacheable = true;
        return Task.CompletedTask;
    }
}
