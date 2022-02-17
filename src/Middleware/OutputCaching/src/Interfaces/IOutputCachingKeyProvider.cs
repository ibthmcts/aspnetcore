// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching;

internal interface IOutputCachingKeyProvider
{
    /// <summary>
    /// Create a base key for a response cache entry.
    /// </summary>
    /// <param name="context">The <see cref="OutputCachingContext"/>.</param>
    /// <returns>The created base key.</returns>
    string CreateBaseKey(OutputCachingContext context);

    /// <summary>
    /// Create a vary key for storing cached responses.
    /// </summary>
    /// <param name="context">The <see cref="OutputCachingContext"/>.</param>
    /// <returns>The created vary key.</returns>
    string CreateStorageVaryByKey(OutputCachingContext context);
}
