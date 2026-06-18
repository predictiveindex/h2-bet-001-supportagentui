using Azure.AI.Projects;
using Azure.AI.Extensions.OpenAI;
using Azure.Identity;
using OpenAI.Responses;
using SupportAgent.Api.Models;

#pragma warning disable OPENAI001

namespace SupportAgent.Api.Services;

public class AgentService
{
    private const string ProjectEndpoint = "https://h2-experiments.services.ai.azure.com/api/projects/H2-BET-001";

    private readonly ProjectResponsesClient _clientA;
    private readonly ProjectResponsesClient _clientB;

    public AgentService()
    {
        var projectClient = new AIProjectClient(new Uri(ProjectEndpoint), new DefaultAzureCredential());
        _clientA = projectClient.OpenAI.GetProjectResponsesClientForAgent(new AgentReference(name: "SupportAgent"));
        _clientB = projectClient.OpenAI.GetProjectResponsesClientForAgent(new AgentReference(name: "VanillaSupportAgent"));
    }

    public async Task<(string ReplyA, string ResponseIdA, string ReplyB, string ResponseIdB)> SendAsync(
        string userMessage,
        string? previousResponseIdA = null,
        string? previousResponseIdB = null,
        CancellationToken cancellationToken = default)
    {
        var taskA = CallAgentAsync(_clientA, userMessage, previousResponseIdA, cancellationToken);
        var taskB = CallAgentAsync(_clientB, userMessage, previousResponseIdB, cancellationToken);

        await Task.WhenAll(taskA, taskB);

        var (replyA, responseIdA) = taskA.Result;
        var (replyB, responseIdB) = taskB.Result;
        return (replyA, responseIdA, replyB, responseIdB);
    }

    private static async Task<(string Reply, string ResponseId)> CallAgentAsync(
        ProjectResponsesClient client,
        string userMessage,
        string? previousResponseId,
        CancellationToken ct)
    {
        CreateResponseOptions options = new()
        {
            InputItems = { ResponseItem.CreateUserMessageItem(userMessage) }
        };
        if (previousResponseId is not null)
            options.PreviousResponseId = previousResponseId;

        var result = await client.CreateResponseAsync(options, ct);
        return (result.Value.GetOutputText(), result.Value.Id);
    }
}
