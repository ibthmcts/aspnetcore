// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching.Policies;

namespace Microsoft.AspNetCore.OutputCaching;

public class OutputCachePolicyBuilder
{
    private readonly List<IOutputCachingPolicy> _policies = new();
    private readonly List<Func<IOutputCachingContext, Task<bool>>> _requirements = new();

    public OutputCachePolicyBuilder(bool useDefaultPolicy = true)
    {
        if (useDefaultPolicy)
        {
            _policies.Add(new DefaultCacheHeaderPolicy());
        }
    }

    public OutputCachePolicyBuilder When(Func<IOutputCachingContext, Task<bool>> predicate)
    {
        _requirements.Add(predicate);
        return this;
    }

    public OutputCachePolicyBuilder Path(PathString pathBase)
    {
        ArgumentNullException.ThrowIfNull(pathBase, nameof(pathBase));

        _requirements.Add(context =>
        {
            var match = context.HttpContext.Request.Path.StartsWithSegments(pathBase);
            return Task.FromResult(match);
        });
        return this;

    }

    public OutputCachePolicyBuilder Path(params PathString[] pathBases)
    {
        ArgumentNullException.ThrowIfNull(pathBases, nameof(pathBases));

        _requirements.Add(context =>
        {
            var match = pathBases.Any(x => context.HttpContext.Request.Path.StartsWithSegments(x));
            return Task.FromResult(match);
        });
        return this;

    }

    public OutputCachePolicyBuilder Method(string method)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));

        _requirements.Add(context =>
        {
            var upperMethod = method.ToUpperInvariant();
            var match = context.HttpContext.Request.Method.ToUpperInvariant() == upperMethod;
            return Task.FromResult(match);
        });
        return this;
    }

    public OutputCachePolicyBuilder Method(params string[] methods)
    {
        ArgumentNullException.ThrowIfNull(methods, nameof(methods));

        _requirements.Add(context =>
        {
            var upperMethods = methods.Select(m => m.ToUpperInvariant()).ToArray();
            var match = methods.Any(m => context.HttpContext.Request.Method.ToUpperInvariant() == m);
            return Task.FromResult(match);
        });
        return this;
    }

    public OutputCachePolicyBuilder VaryByQuery(params string[] queryKeys)
    {
        ArgumentNullException.ThrowIfNull(queryKeys, nameof(queryKeys));

        _policies.Add(new VaryByQueryPolicy(queryKeys));
        return this;
    }

    public OutputCachePolicyBuilder Profile(string profileName)
    {
        ArgumentNullException.ThrowIfNull(profileName, nameof(profileName));

        _policies.Add(new ProfilePolicy(profileName));

        return this;
    }

    public OutputCachePolicyBuilder Tag(params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        _policies.Add(new TagsPolicy(tags));
        return this;
    }

    public OutputCachePolicyBuilder Expires(TimeSpan expiration)
    {
        _policies.Add(new ExpirationPolicy(expiration));
        return this;
    }

    public OutputCachePolicyBuilder Lock(bool lockResponse = true)
    {
        _policies.Add(new LockingPolicy(lockResponse));
        return this;
    }

    public OutputCachePolicyBuilder Clear()
    {
        _requirements.Clear();
        _policies.Clear();
        return this;
    }

    public OutputCachePolicyBuilder NotCacheable()
    {
        _policies.Add(new NoCachingPolicy());
        return this;
    }

    /// <summary>
    /// Builds a new <see cref="IOutputCachingPolicy"/> from the definitions
    /// in this instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="IOutputCachingPolicy"/> built from the definitions in this instance.
    /// </returns>
    public IOutputCachingPolicy Build()
    {
        var policies = new CompositePolicy(_policies.ToArray());

        if (_requirements.Any())
        {
            return new PredicatePolicy(async c =>
            {
                foreach (var r in _requirements)
                {
                    if (!await r(c))
                    {
                        return false;
                    }
                }

                return true;
            }, policies);
        }

        return policies;
    }
}
