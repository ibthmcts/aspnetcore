// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching.Policies;

internal class PoliciesMetadata : IPoliciesMetadata
{
    public List<IOutputCachingRequestPolicy> RequestPolicies { get; } = new();

    public List<IOutputCachingResponsePolicy> ResponsePolicies { get; } = new();
}
