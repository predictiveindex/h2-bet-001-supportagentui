using SupportAgent.Api.Models;
using SupportAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AgentService>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/chat", async (ChatRequest request, AgentService agentService) =>
{
    var (reply, responseId) = await agentService.SendAsync(request.Message, request.PreviousResponseId);
    return Results.Ok(new ChatResponse(reply, responseId));
});

// SPA fallback — serve index.html for all non-API routes
app.MapFallbackToFile("index.html");

app.Run();
