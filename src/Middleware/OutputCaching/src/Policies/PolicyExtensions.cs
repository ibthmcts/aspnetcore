// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.OutputCaching.Policies;
public static class PolicyExtensions
{
    public static TBuilder OutputCache<TBuilder>(this TBuilder builder, params IOutputCachingPolicy[] items) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        var policiesMetadata = new PoliciesMetadata();
        policiesMetadata.Policies.AddRange(items);

        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(policiesMetadata);
        });
        return builder;
    }

    public static TBuilder OutputCache<TBuilder>(this TBuilder builder, Action<OutputCachePolicyBuilder> policy) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var outputCachePolicyBuilder = new OutputCachePolicyBuilder();
        policy?.Invoke(outputCachePolicyBuilder);

        var policiesMetadata = new PoliciesMetadata();
        policiesMetadata.Policies.Add(outputCachePolicyBuilder.Build());

        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(policiesMetadata);
        });

        return builder;
    }
}
