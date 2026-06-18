using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
using SupportAgent.Api.Models;
using SupportAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Redis connection (optional — RateLimitService falls back to in-memory if absent) ──
var redisHost = builder.Configuration["Redis:Host"];
if (!string.IsNullOrWhiteSpace(redisHost))
{
    var redisOptions = new ConfigurationOptions { AbortOnConnectFail = false };
    redisOptions.EndPoints.Add(redisHost, 6380);
    redisOptions.Ssl = true;
    await redisOptions.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
    var muxer = await ConnectionMultiplexer.ConnectAsync(redisOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);
}

builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<VoteService>();
builder.Services.AddSingleton<RateLimitService>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/chat", async (ChatRequest request, AgentService agentService, RateLimitService rateLimit) =>
{
    if (!await rateLimit.TryConsumeAsync())
        return Results.Json(
            new { error = "Daily request limit reached. Please try again tomorrow." },
            statusCode: 429);

    var (replyA, responseIdA, replyB, responseIdB) = await agentService.SendAsync(
        request.Message, request.PreviousResponseIdA, request.PreviousResponseIdB);

    return Results.Ok(new ChatResponse(replyA, responseIdA, replyB, responseIdB));
});

app.MapPost("/api/vote", async (VoteRequest request, VoteService voteService) =>
{
    await voteService.RecordVoteAsync(request);
    return Results.Ok();
});

// SPA fallback — serve index.html for all non-API routes
app.MapFallbackToFile("index.html");

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }
