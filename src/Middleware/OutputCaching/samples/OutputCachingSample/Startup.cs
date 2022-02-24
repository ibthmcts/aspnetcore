// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// TODO:
// buidler pattern for endpoint options
// attribute
// https://github.com/dotnet/aspnetcore/issues/39840


using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.OutputCaching.Policies;

long requests = 0;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCaching(options =>
{
    // options.Policies.Clear();

    options.Profiles["NoCache"] = new OutputCachePolicyBuilder().NotCacheable().Build();
});

var app = builder.Build();

app.UseOutputCaching();

// Cached because default policy
app.MapGet("/", () => "Hello " + DateTime.UtcNow.ToString("o")).OutputCache(p => p.Tag("home"));

app.MapPost("/purge/{tag}", async (IOutputCacheStore cache, string tag) =>
{
    // POST such that the endpoint is not cached itself

    if (!String.IsNullOrEmpty(tag))
    {
        await cache.EvictByTagAsync(tag);
    }
});

// Cached because default policy
app.MapGet("/slownolock", async (context) =>
{
    var logger = context.RequestServices.GetService<ILogger<OutputCachingMiddleware>>();
    logger.LogWarning("Slowing ... {requests}", requests++);
    await Task.Delay(1000);
    await context.Response.WriteAsync("Slow " + DateTime.UtcNow.ToString("o"));
}).OutputCache(p => p.Expires(TimeSpan.FromSeconds(1)).Lock(false));

// Cached because default policy
app.MapGet("/slow", async (context) =>
{
    var logger = context.RequestServices.GetService<ILogger<OutputCachingMiddleware>>();
    logger.LogWarning("Slowing ... {requests}", requests++);
    await Task.Delay(1000);
    await context.Response.WriteAsync("Slow " + DateTime.UtcNow.ToString("o"));
}).OutputCache(p => p.Expires(TimeSpan.FromSeconds(1)).Lock(true));

// Cached because default policy
app.MapGet("/nocache", async context =>
{
    await context.Response.WriteAsync("Not cached " + DateTime.UtcNow.ToString("o"));
}).OutputCache(p => p.Profile("NoCache"));

// Cached because Response Caching policy and contains "Cache-Control: public"
app.MapGet("/headers", async context =>
{
    // From a browser this endpoint won't be cached because of max-age: 0
    context.Response.Headers.CacheControl = CacheControlHeaderValue.PublicString;
    await context.Response.WriteAsync("Headers " + DateTime.UtcNow.ToString("o"));
}).OutputCache(new ResponseCachingPolicy());

app.MapGet("/query", async context =>
{
    // Cached entries will vary by culture, but any other additional query is ignored and returned the same cached content

    await context.Response.WriteAsync($"Culture: {context.Request.Query["culture"]} {DateTime.UtcNow.ToString("o")}");
}).OutputCache(p => p.VaryByQuery("culture"));

await app.RunAsync();
