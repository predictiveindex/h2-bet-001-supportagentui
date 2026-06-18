using Azure.Data.Tables;
using Azure.Identity;
using SupportAgent.Api.Models;
using System.Text.Json;

namespace SupportAgent.Api.Services;

public class VoteService
{
    // Azure Table Storage strings are UTF-16; limit is 32,767 characters (≈64KB at 2 bytes/char)
    private const int MaxPropertyChars = 32_767;

    private readonly TableClient _tableClient;

    public VoteService(IConfiguration config)
    {
        var endpoint = config["AZURE_STORAGE_ACCOUNT_URL"]
            ?? "https://sth2validatedev.table.core.windows.net";
        var serviceClient = new TableServiceClient(new Uri(endpoint), new DefaultAzureCredential());
        _tableClient = serviceClient.GetTableClient("bet001");
        _tableClient.CreateIfNotExists();
    }

    public async Task RecordVoteAsync(VoteRequest request, CancellationToken ct = default)
    {
        var votedAgent = request.VotedFor == "A" ? "SupportAgent" : "VanillaSupportAgent";
        var historyAJson = TruncateHistory(request.HistoryA);
        var historyBJson = TruncateHistory(request.HistoryB);

        var entity = new TableEntity("votes", Guid.NewGuid().ToString())
        {
            ["VotedAgent"] = votedAgent,
            ["HistoryA"] = historyAJson,
            ["HistoryB"] = historyBJson,
            ["CreatedAt"] = DateTimeOffset.UtcNow,
        };

        await _tableClient.AddEntityAsync(entity, ct);
    }

    // Truncates history from the beginning in complete prompt+response pairs
    // until the serialized JSON fits within the Azure Table Storage 64KB property limit.
    internal static string TruncateHistory(ChatHistoryEntry[] history)
    {
        var entries = history.ToList();

        while (entries.Count > 0)
        {
            var json = JsonSerializer.Serialize(entries);
            if (json.Length <= MaxPropertyChars)
                return json;

            // Remove the oldest complete pair (user + assistant); fall back to single entry
            entries.RemoveRange(0, entries.Count >= 2 ? 2 : 1);
        }

        return "[]";
    }
}
