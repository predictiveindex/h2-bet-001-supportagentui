using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SupportAgent.Api.Tests;

public class ApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ChatEndpoint_ReturnsDualResponses()
    {
        var body = JsonSerializer.Serialize(new { message = "Say hi in exactly one word." });
        var response = await _client.PostAsync(
            "/api/chat",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("replyA").GetString()!.Length > 0, "replyA should not be empty");
        Assert.True(json.GetProperty("replyB").GetString()!.Length > 0, "replyB should not be empty");
        Assert.False(string.IsNullOrEmpty(json.GetProperty("responseIdA").GetString()), "responseIdA should not be empty");
        Assert.False(string.IsNullOrEmpty(json.GetProperty("responseIdB").GetString()), "responseIdB should not be empty");
    }

    [Fact]
    public async Task VoteEndpoint_Returns200()
    {
        var body = JsonSerializer.Serialize(new
        {
            votedFor = "A",
            historyA = new[]
            {
                new { role = "user", content = "Hello" },
                new { role = "assistant", content = "Hi!" }
            },
            historyB = new[]
            {
                new { role = "user", content = "Hello" },
                new { role = "assistant", content = "Hey!" }
            }
        });

        var response = await _client.PostAsync(
            "/api/vote",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
