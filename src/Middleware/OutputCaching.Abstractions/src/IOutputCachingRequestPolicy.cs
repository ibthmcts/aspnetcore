// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// An implementation of this interface can update how the current request is cached.
/// </summary>
public interface IOutputCachingRequestPolicy
{
    Task OnRequestAsync(IOutputCachingContext context);

    // public virtual bool AttemptResponseCaching(ResponseCachingContext context)
    // {
    //     var request = context.HttpContext.Request;

    //     // Verify the method
    //     if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
    //     {
    //         context.Logger.RequestMethodNotCacheable(request.Method);
    //         return false;
    //     }

    //     // Verify existence of authorization headers
    //     if (!StringValues.IsNullOrEmpty(request.Headers.Authorization))
    //     {
    //         context.Logger.RequestWithAuthorizationNotCacheable();
    //         return false;
    //     }

    //     return true;
    // }

    // public virtual bool AllowCacheLookup(ResponseCachingContext context)
    // {
    //     var requestHeaders = context.HttpContext.Request.Headers;
    //     var cacheControl = requestHeaders.CacheControl;

    //     // Verify request cache-control parameters
    //     if (!StringValues.IsNullOrEmpty(cacheControl))
    //     {
    //         if (HeaderUtilities.ContainsCacheDirective(cacheControl, CacheControlHeaderValue.NoCacheString))
    //         {
    //             context.Logger.RequestWithNoCacheNotCacheable();
    //             return false;
    //         }
    //     }
    //     else
    //     {
    //         // Support for legacy HTTP 1.0 cache directive
    //         if (HeaderUtilities.ContainsCacheDirective(requestHeaders.Pragma, CacheControlHeaderValue.NoCacheString))
    //         {
    //             context.Logger.RequestWithPragmaNoCacheNotCacheable();
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    // public virtual bool AllowCacheStorage(ResponseCachingContext context)
    // {
    //     // Check request no-store
    //     return !HeaderUtilities.ContainsCacheDirective(context.HttpContext.Request.Headers.CacheControl, CacheControlHeaderValue.NoStoreString);
    // }

}
