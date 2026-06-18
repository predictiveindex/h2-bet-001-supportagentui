using SupportAgent.Api.Models;
using SupportAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
    if (!rateLimit.TryConsume())
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
