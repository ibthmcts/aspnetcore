// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.OutputCaching;

internal class MemoryOutputCache : IOutputCache
{
    private readonly IMemoryCache _cache;

    internal MemoryOutputCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public OutputCacheEntry? Get(string key)
    {
        var entry = _cache.Get(key);

        if (entry is MemoryCachedResponse memoryCachedResponse)
        {
            return new OutputCacheEntry
            {
                Created = memoryCachedResponse.Created,
                StatusCode = memoryCachedResponse.StatusCode,
                Headers = memoryCachedResponse.Headers,
                Body = memoryCachedResponse.Body
            };
        }

        return null;
    }

    public void Set(string key, OutputCacheEntry cachedResponse, TimeSpan validFor)
    {
        _cache.Set(
            key,
            new MemoryCachedResponse
            {
                Created = cachedResponse.Created,
                StatusCode = cachedResponse.StatusCode,
                Headers = cachedResponse.Headers,
                Body = cachedResponse.Body
            },
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = validFor,
                Size = CacheEntryHelpers.EstimateCachedResponseSize(cachedResponse)
            });
    }
}
