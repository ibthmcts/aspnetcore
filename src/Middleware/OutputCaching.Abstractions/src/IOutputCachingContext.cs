// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.OutputCaching;

public interface IOutputCachingContext
{
    TimeSpan? CachedEntryAge { get; }
    HttpContext HttpContext { get; }
    DateTimeOffset? ResponseTime { get; }
    DateTimeOffset? ResponseDate { get; }
    DateTimeOffset? ResponseExpires { get; }
    TimeSpan? ResponseSharedMaxAge { get; }
    TimeSpan? ResponseMaxAge { get; }
    IHeaderDictionary CachedResponseHeaders { get; }
    CachedVaryByRules CachedVaryByRules { get; }
    ILogger Logger { get; }

    /// <summary>
    /// Determine whether the response caching logic should be attempted for the incoming HTTP request.
    /// </summary>
    bool AttemptResponseCaching { get; set; }

    /// <summary>
    /// Determine whether a cache lookup is allowed for the incoming HTTP request.
    /// </summary>
    bool AllowCacheLookup { get; set; }

    /// <summary>
    /// Determine whether storage of the response is allowed for the incoming HTTP request.
    /// </summary>
    bool AllowCacheStorage { get; set; }

    /// <summary>
    /// Determine whether the response received by the middleware can be cached for future requests.
    /// </summary>
    bool IsResponseCacheable { get; set; }

    /// <summary>
    /// Determine whether the response retrieved from the response cache is fresh and can be served.
    /// </summary>
    bool IsCacheEntryFresh { get; set; }
}
