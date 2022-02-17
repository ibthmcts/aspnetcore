// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCaching(options =>
{
    // We might need a way to apply request policies by pattern, for instance changing VaryByQuery for a specific route 
    options.RequestPolicies.Add(new VaryByQueryPolicy("culture"));
});

var app = builder.Build();

app.UseOutputCaching();

// Cached because default policy
app.MapGet("/", () => "Hello " + DateTime.UtcNow.ToString("o"));

// Cached because default policy
app.MapGet("/nocache", async context =>
{
    context.Features.Get<IOutputCachingFeature>().ResponsePolicies.Add(new NoCachingPolicy());
    await context.Response.WriteAsync("Not cached " + DateTime.UtcNow.ToString("o"));
});

// Cached because Response Caching policy and contains "Cache-Control: public"
app.MapGet("/headers", async context =>
{
    context.Features.Get<IOutputCachingFeature>().ResponsePolicies.Add(new ResponseCachingPolicy());
    context.Response.Headers.CacheControl = CacheControlHeaderValue.PublicString;
    await context.Response.WriteAsync("Headers " + DateTime.UtcNow.ToString("o"));
});

// Cached because Response Caching policy and contains "Cache-Control: public"
app.MapGet("/query", async context =>
{
    await context.Response.WriteAsync($"Culture: {context.Request.Query["culture"]} {DateTime.UtcNow.ToString("o")}");
});

await app.RunAsync();
