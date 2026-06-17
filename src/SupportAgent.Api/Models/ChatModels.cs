namespace SupportAgent.Api.Models;

public record ChatMessage(string Role, string Content);
public record ChatRequest(ChatMessage[] Messages);
public record ChatResponse(string Reply);
