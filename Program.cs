using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

Console.WriteLine("Script start");

var host = new WebHostBuilder()
    .UseKestrel(options =>
    {
        options.ListenAnyIP(3000);
    })
    .ConfigureServices(services =>
    {
        services.AddRouting();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    })
    .Configure(app =>
    {
        app.UseCors();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", HandleRequest);
            endpoints.MapPost("/", HandleRequest);
            endpoints.Map("{**path}", HandleRequest);
        });
    })
    .Build();

Console.WriteLine("Server created");
Console.WriteLine("Server listening callback");
Console.WriteLine("Server running on 0.0.0.0:3000");

static async Task HandleRequest(HttpContext context)
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var method = context.Request.Method;
    var path = context.Request.Path.Value ?? "/";

    Console.WriteLine($"Request received: {method} {path} from {clientIp}");

    var dt = DateTime.UtcNow; // UTC plays nicer with "o"
    var version = Environment.GetEnvironmentVariable("SF_TAG") ?? "latest";

    var responseData = new
    {
        message = "Hello from .NET",
        dt = dt.ToString("o"), // ISO 8601
        version
    };

    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsync(JsonSerializer.Serialize(responseData));
}

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Server error: {ex.Message}");
}
