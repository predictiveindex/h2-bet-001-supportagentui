namespace SupportAgent.Api.Models;

public record ChatRequest(
    string Message,
    string? PreviousResponseIdA = null,
    string? PreviousResponseIdB = null);

public record ChatResponse(
    string ReplyA, string ResponseIdA,
    string ReplyB, string ResponseIdB);

public record ChatHistoryEntry(string Role, string Content);

public record VoteRequest(
    string VotedFor,                // "A" or "B"
    ChatHistoryEntry[] HistoryA,
    ChatHistoryEntry[] HistoryB);
