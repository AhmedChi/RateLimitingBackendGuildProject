using RateLimitingBackendGuildProject.ApiService.Config;
using RateLimitingBackendGuildProject.ApiService.Data.Models;
using RateLimitingBackendGuildProject.ApiService.Endpoints;
using RateLimitingBackendGuildProject.ApiService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ClientStore>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

builder.Services.AddSingleton<TokenBucketStore>();

app.MapWeatherEndpoints();
app.MapDefaultEndpoints();

app.Run();