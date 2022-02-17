// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// Default policy
/// </summary>
public class VaryByQueryPolicy : IOutputCachingRequestPolicy
{
    private StringValues _queryKeys { get; set; }

    public VaryByQueryPolicy()
    {
    }

    public VaryByQueryPolicy(string queryKey)
    {
        _queryKeys = queryKey;
    }

    public VaryByQueryPolicy(params string[] queryKeys)
    {
        _queryKeys = queryKeys;
    }

    public Task OnRequestAsync(IOutputCachingContext context)
    {
        // No vary by query?
        if (_queryKeys.Count == 0)
        {
            context.CachedVaryByRules.QueryKeys = _queryKeys;
            return Task.CompletedTask;
        }

        // If the current key is "*" (default) replace it
        if (context.CachedVaryByRules.QueryKeys.Count == 1 && string.Equals(context.CachedVaryByRules.QueryKeys[0], "*", StringComparison.Ordinal))
        {
            context.CachedVaryByRules.QueryKeys = _queryKeys;
            return Task.CompletedTask;
        }

        context.CachedVaryByRules.QueryKeys = StringValues.Concat(context.CachedVaryByRules.QueryKeys, _queryKeys);

        return Task.CompletedTask;
    }
}
