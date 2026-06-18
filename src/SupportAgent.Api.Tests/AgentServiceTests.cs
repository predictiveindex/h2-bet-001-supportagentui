using SupportAgent.Api.Services;

namespace SupportAgent.Api.Tests;

public class AgentServiceTests
{
    [Fact]
    public async Task SendAsync_ReturnsRepliesForBothAgents()
    {
        var service = new AgentService();

        var (replyA, responseIdA, replyB, responseIdB) =
            await service.SendAsync("Say hi in exactly one word.");

        Assert.NotEmpty(replyA);
        Assert.NotEmpty(responseIdA);
        Assert.NotEmpty(replyB);
        Assert.NotEmpty(responseIdB);
    }

    [Fact]
    public async Task SendAsync_WithPreviousResponseId_MaintainsContext()
    {
        var service = new AgentService();

        // First turn: ask a question that yields a memorable answer
        var (_, responseIdA, _, responseIdB) =
            await service.SendAsync("My favourite colour is ultraviolet. Please acknowledge that.");

        // Second turn: verify the context is retained via chained response IDs
        var (replyA2, _, replyB2, _) =
            await service.SendAsync("What colour did I tell you was my favourite?", responseIdA, responseIdB);

        Assert.Contains("ultraviolet", replyA2, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ultraviolet", replyB2, StringComparison.OrdinalIgnoreCase);
    }
}
