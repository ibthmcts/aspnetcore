// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching;

public class OutputCacheProfileAttribute : Attribute, IPoliciesMetadata
{
    private readonly List<IOutputCachingPolicy> _policies;

    public OutputCacheProfileAttribute(string profileName)
    {
        _policies = new List<IOutputCachingPolicy>() { new ProfilePolicy(profileName) };
    }

    List<IOutputCachingPolicy> IPoliciesMetadata.Policies => _policies;
}
