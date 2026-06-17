namespace SupportAgent.Api.Models;

public record ChatRequest(string Message, string? PreviousResponseId = null);
public record ChatResponse(string Reply, string ResponseId);
