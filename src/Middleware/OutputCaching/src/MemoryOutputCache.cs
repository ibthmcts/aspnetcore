// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.OutputCaching;

internal class MemoryOutputCache : IOutputCache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, HashSet<string>> _taggedEntries = new();

    internal MemoryOutputCache(IMemoryCache cache)
    {
        ArgumentNullException.ThrowIfNull(cache, nameof(cache));

        _cache = cache;
    }

    public Task EvictByTagAsync(string tag)
    {
        if (_taggedEntries.TryGetValue(tag, out var keys))
        {
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    public Task<OutputCacheEntry?> GetAsync(string key)
    {
        var entry = _cache.Get(key);

        if (entry is MemoryCachedResponse memoryCachedResponse)
        {
            var outputCacheEntry = new OutputCacheEntry
            {
                Created = memoryCachedResponse.Created,
                StatusCode = memoryCachedResponse.StatusCode,
                Headers = memoryCachedResponse.Headers,
                Body = memoryCachedResponse.Body,
            };

            outputCacheEntry.Tags = memoryCachedResponse.Tags.ToArray();

            return Task.FromResult(outputCacheEntry);
        }

        return Task.FromResult(default(OutputCacheEntry));
    }

    public Task SetAsync(string key, OutputCacheEntry cachedResponse, TimeSpan validFor)
    {
        foreach (var tag in cachedResponse.Tags)
        {
            var keys = _taggedEntries.GetOrAdd(tag, _ => new HashSet<string>());

            // Copy the list of tags to prevent locking

            var local = new HashSet<string>(keys);
            local.Add(key);

            _taggedEntries[tag] = local;
        }

        _cache.Set(
            key,
            new MemoryCachedResponse
            {
                Created = cachedResponse.Created,   
                StatusCode = cachedResponse.StatusCode,
                Headers = cachedResponse.Headers,
                Body = cachedResponse.Body,
                Tags = cachedResponse.Tags
            },
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = validFor,
                Size = CacheEntryHelpers.EstimateCachedResponseSize(cachedResponse)               
            });

        return Task.CompletedTask;
    }
}
