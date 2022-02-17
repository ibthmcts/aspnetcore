// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OutputCaching.Policies;
public static class PolicyExtensions
{
    public static PredicatePolicy When(this IOutputCachingRequestPolicy policy, Func<IOutputCachingContext, Task<bool>> predicate)
    {
        return new PredicatePolicy(predicate, policy);
    }

    public static PredicatePolicy Map(this IOutputCachingRequestPolicy policy, PathString pathBase)
    {
        return new PredicatePolicy(context =>
        {
            var match = context.HttpContext.Request.Path.StartsWithSegments(pathBase);
            return Task.FromResult(match);
        }, policy);
    }

    public static PredicatePolicy Map(this IOutputCachingRequestPolicy policy, params PathString[] pathBases)
    {
        return new PredicatePolicy(context =>
        {
            var match = pathBases.Any(x => context.HttpContext.Request.Path.StartsWithSegments(x));
            return Task.FromResult(match);
        }, policy);
    }

    public static PredicatePolicy Methods(this IOutputCachingRequestPolicy policy, string method)
    {
        return new PredicatePolicy(context =>
        {
            var upperMethod = method.ToUpperInvariant();
            var match = context.HttpContext.Request.Method.ToUpperInvariant() == upperMethod;
            return Task.FromResult(match);
        }, policy);
    }

    public static PredicatePolicy Methods(this IOutputCachingRequestPolicy policy, params string[] methods)
    {
        return new PredicatePolicy(context =>
        {
            var upperMethods = methods.Select(m => m.ToUpperInvariant()).ToArray();
            var match = methods.Any(m => context.HttpContext.Request.Method.ToUpperInvariant() == m);
            return Task.FromResult(match);
        }, policy);
    }
}
