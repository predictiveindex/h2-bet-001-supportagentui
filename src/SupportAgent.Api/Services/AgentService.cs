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
    private const string AgentName = "SupportAgent";

    private readonly ProjectResponsesClient _client;

    public AgentService()
    {
        var projectClient = new AIProjectClient(new Uri(ProjectEndpoint), new DefaultAzureCredential());
        var agentRef = new AgentReference(name: AgentName);
        _client = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentRef);
    }

    public async Task<(string Reply, string ResponseId)> SendAsync(
        string userMessage,
        string? previousResponseId = null,
        CancellationToken cancellationToken = default)
    {
        CreateResponseOptions options = new()
        {
            InputItems = { ResponseItem.CreateUserMessageItem(userMessage) }
        };
        if (previousResponseId is not null)
            options.PreviousResponseId = previousResponseId;

        var result = await _client.CreateResponseAsync(options, cancellationToken);
        return (result.Value.GetOutputText(), result.Value.Id);
    }
}
