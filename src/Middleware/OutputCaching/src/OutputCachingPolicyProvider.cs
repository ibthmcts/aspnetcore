// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OutputCaching;

internal class OutputCachingPolicyProvider : IOutputCachingPolicyProvider
{
    private readonly OutputCachingOptions options;

    public OutputCachingPolicyProvider(IOptions<OutputCachingOptions> options)
    {
        this.options = options.Value;
    }

    public async Task OnRequestAsync(IOutputCachingContext context)
    {
        foreach (var policy in options.RequestPolicies)
        {
            await policy.OnRequestAsync(context);
        }
    }

    public async Task OnServeFromCacheAsync(IOutputCachingContext context)
    {
        foreach (var policy in options.ResponsePolicies)
        {
            await policy.OnServeFromCacheAsync(context);
        }

        // Apply response policies defined on the feature, e.g. from action attributes

        var responsePolicies = context.HttpContext.Features.Get<IOutputCachingFeature>()?.ResponsePolicies;

        if (responsePolicies != null)
        {
            foreach (var policy in responsePolicies)
            {
                await policy.OnServeFromCacheAsync(context);
            }
        }
    }

    public async Task OnServeResponseAsync(IOutputCachingContext context)
    {
        foreach (var policy in options.ResponsePolicies)
        {
            await policy.OnServeResponseAsync(context);
        }

        // Apply response policies defined on the feature, e.g. from action attributes

        var responsePolicies = context.HttpContext.Features.Get<IOutputCachingFeature>()?.ResponsePolicies;

        if (responsePolicies != null)
        {
            foreach (var policy in responsePolicies)
            {
                await policy.OnServeResponseAsync(context);
            }
        }
    }
}
