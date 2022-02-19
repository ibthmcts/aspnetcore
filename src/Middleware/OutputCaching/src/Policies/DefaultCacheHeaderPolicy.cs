// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// Default policy.
/// </summary>
public class DefaultCacheHeaderPolicy : IOutputCachingRequestPolicy, IOutputCachingResponsePolicy
{
    public Task OnRequestAsync(IOutputCachingContext context)
    {
        context.AttemptResponseCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.IsResponseCacheable = true;

        // Vary by any query by default
        context.CachedVaryByRules.QueryKeys = "*";

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
