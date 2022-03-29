// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Interop.FunctionalTests.Http2;

public class Http2RequestTests : LoggedTest
{
    [Fact]
    public async Task GET_NoTLS_Http11RequestToHttp2Endpoint_400Result()
    {
        // Arrange
        var builder = CreateHostBuilder(c => Task.CompletedTask, protocol: HttpProtocols.Http2, plaintext: true);

        using (var host = builder.Build())
        using (var client = HttpHelpers.CreateClient())
        {
            await host.StartAsync().DefaultTimeout();

            var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{host.GetPort()}/");
            request.Version = HttpVersion.Version11;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

            // Act
            var responseMessage = await client.SendAsync(request, CancellationToken.None).DefaultTimeout();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
            Assert.Equal("An HTTP/1.x request was sent to an HTTP/2 only endpoint.", await responseMessage.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task GET_RequestReturnsLargeData_GracefulShutdownDuringRequest_RequestGracefullyCompletes()
    {
        // Arrange
        const int DataLength = 500_000;
        var randomBytes = Enumerable.Range(1, DataLength).Select(i => (byte)((i % 10) + 48)).ToArray();

        var syncPoint = new SyncPoint();

        ILogger logger = null;
        IHostApplicationLifetime hostApplicationLifetime = null;
        var builder = CreateHostBuilder(
            async c =>
            {
                await syncPoint.WaitToContinue();

                var memory = c.Response.BodyWriter.GetMemory(randomBytes.Length);

                logger.LogInformation($"Server writing {randomBytes.Length} bytes response");
                randomBytes.CopyTo(memory);

                logger.LogInformation($"Server advancing {randomBytes.Length} bytes response");
                c.Response.BodyWriter.Advance(randomBytes.Length);
            },
            protocol: HttpProtocols.Http2);

        using var host = builder.Build();
        logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test");
        hostApplicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

        var client = HttpHelpers.CreateClient();

        // Act
        await host.StartAsync().DefaultTimeout();

        var longRunningTask = StartLongRunningRequestAsync(logger, host, client);

        logger.LogInformation("Waiting for request on server");
        await syncPoint.WaitForSyncPoint().DefaultTimeout();

        logger.LogInformation("Stopping server");
        var stopTask = host.StopAsync();

        syncPoint.Continue();

        var readData = await longRunningTask.DefaultTimeout();
        await stopTask.DefaultTimeout();

        // Assert
        Assert.Equal(randomBytes, readData);
    }

    private static async Task<byte[]> StartLongRunningRequestAsync(ILogger logger, IHost host, HttpMessageInvoker client)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://127.0.0.1:{host.GetPort()}/");
        request.Version = HttpVersion.Version20;

        var responseMessage = await client.SendAsync(request, CancellationToken.None).DefaultTimeout();
        responseMessage.EnsureSuccessStatusCode();

        var responseStream = await responseMessage.Content.ReadAsStreamAsync();

        var data = new List<byte>();
        var buffer = new byte[1024 * 128];
        int readCount;
        while ((readCount = await responseStream.ReadAsync(buffer)) != 0)
        {
            data.AddRange(buffer.AsMemory(0, readCount).ToArray());
            logger.LogInformation($"Received {readCount} bytes. Total {data.Count} bytes.");
        }
        logger.LogInformation($"Finished reading response content");

        return data.ToArray();
    }

    private IHostBuilder CreateHostBuilder(RequestDelegate requestDelegate, HttpProtocols? protocol = null, Action<KestrelServerOptions> configureKestrel = null, bool? plaintext = null)
    {
        return HttpHelpers.CreateHostBuilder(AddTestLogging, requestDelegate, protocol, configureKestrel, plaintext);
    }
}
