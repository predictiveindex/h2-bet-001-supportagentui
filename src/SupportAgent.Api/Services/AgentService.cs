using Azure.Core;
using Azure.Identity;
using SupportAgent.Api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupportAgent.Api.Services;

public class AgentService
{
    private const string FoundryEndpoint =
        "https://H2-Experiments.services.ai.azure.com/api/projects/H2-BET-001/agents/SupportAgent/endpoint/protocols/openai/responses";

    private const string TokenScope = "https://cognitiveservices.azure.com/.default";

    private readonly HttpClient _httpClient;
    private readonly TokenCredential _credential;

    public AgentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _credential = new DefaultAzureCredential();
    }

    public async Task<string> SendAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var token = await _credential.GetTokenAsync(
            new TokenRequestContext([TokenScope]),
            cancellationToken);

        var payload = new FoundryRequest(
            Input: messages.Select(m => new FoundryMessage(m.Role, m.Content)).ToArray());

        var json = JsonSerializer.Serialize(payload, FoundryJsonContext.Default.FoundryRequest);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, FoundryEndpoint) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize(responseJson, FoundryJsonContext.Default.FoundryResponse);

        return result?.Output?.FirstOrDefault(o => o.Type == "message")
                      ?.Content?.FirstOrDefault(c => c.Type == "output_text")
                      ?.Text
               ?? string.Empty;
    }
}

// Internal DTOs for the OpenAI Responses API wire format
internal record FoundryRequest(
    [property: JsonPropertyName("input")] FoundryMessage[] Input);

internal record FoundryMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal record FoundryResponse(
    [property: JsonPropertyName("output")] FoundryOutputItem[]? Output);

internal record FoundryOutputItem(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("content")] FoundryContentItem[]? Content);

internal record FoundryContentItem(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string? Text);

[JsonSerializable(typeof(FoundryRequest))]
[JsonSerializable(typeof(FoundryResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FoundryJsonContext : JsonSerializerContext { }

