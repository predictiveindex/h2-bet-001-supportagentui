using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using SupportAgent.Api.Models;
using SupportAgent.Api.Services;

namespace SupportAgent.Api.Tests;

public class VoteServiceTests
{
    private static VoteService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AZURE_STORAGE_ACCOUNT_URL"] = "https://sth2validatedev.table.core.windows.net"
            })
            .Build();
        return new VoteService(config);
    }

    [Fact]
    public async Task RecordVoteAsync_StoresVoteInTableStorage()
    {
        var service = CreateService();
        var historyA = new[]
        {
            new ChatHistoryEntry("user", "Hello"),
            new ChatHistoryEntry("assistant", "Hi there!")
        };
        var historyB = new[]
        {
            new ChatHistoryEntry("user", "Hello"),
            new ChatHistoryEntry("assistant", "Hey!")
        };

        var request = new VoteRequest("A", historyA, historyB);
        await service.RecordVoteAsync(request);

        // Verify by querying the table directly
        var tableClient = new TableClient(
            new Uri("https://sth2validatedev.table.core.windows.net"),
            "bet001",
            new DefaultAzureCredential());

        var found = false;
        await foreach (var entity in tableClient.QueryAsync<TableEntity>(
            filter: $"PartitionKey eq 'votes' and VotedAgent eq 'SupportAgent'"))
        {
            found = true;
            break;
        }

        Assert.True(found, "Expected to find a vote for 'SupportAgent' in the table.");
    }

    [Fact]
    public async Task RecordVoteAsync_TruncatesOversizedHistory()
    {
        var service = CreateService();

        // Generate history well over 64KB
        var largeHistory = Enumerable.Range(0, 600)
            .SelectMany(i => new[]
            {
                new ChatHistoryEntry("user", $"Question {i}: " + new string('x', 150)),
                new ChatHistoryEntry("assistant", $"Answer {i}: " + new string('y', 150))
            }).ToArray();

        var request = new VoteRequest("B", largeHistory, largeHistory);

        // Should not throw — history is truncated before writing
        await service.RecordVoteAsync(request);
    }

    [Fact]
    public void TruncateHistory_ProducesValidJsonUnder64KB()
    {
        var largeHistory = Enumerable.Range(0, 1000)
            .SelectMany(i => new[]
            {
                new ChatHistoryEntry("user", $"Q{i}: " + new string('x', 200)),
                new ChatHistoryEntry("assistant", $"A{i}: " + new string('y', 200))
            }).ToArray();

        var json = VoteService.TruncateHistory(largeHistory);

        Assert.True(System.Text.Encoding.UTF8.GetByteCount(json) <= 64 * 1024);
        // Verify it is valid JSON array
        var parsed = System.Text.Json.JsonSerializer.Deserialize<ChatHistoryEntry[]>(json);
        Assert.NotNull(parsed);
        // Verify pairs are intact (even number of entries if not empty)
        if (parsed!.Length > 0)
            Assert.Equal(0, parsed.Length % 2);
    }
}
