using SupportAgent.Api.Models;
using SupportAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<AgentService>();
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
    var reply = await agentService.SendAsync(request.Messages);
    return Results.Ok(new ChatResponse(reply));
});

// SPA fallback — serve index.html for all non-API routes
app.MapFallbackToFile("index.html");

app.Run();
