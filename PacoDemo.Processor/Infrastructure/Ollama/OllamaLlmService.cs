using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using PacoDemo.Processor.Domain.Services;

namespace PacoDemo.Processor.Infrastructure.Ollama;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OllamaLlmService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> AskAsync(
        string question,
        IEnumerable<(int PageNumber, int StartLine, string Text)> contextChunks,
        CancellationToken ct = default)
    {
        var modelName = _config["Ollama:LlmModel"] ?? "ministral-3-8b-instruct-2512";
        var systemPrompt = BuildSystemPrompt(contextChunks);

        var requestBody = new
        {
            model = modelName,
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = question }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/chat", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: ct);

        return result?.Message?.Content ?? "No answer generated.";
    }

    private static string BuildSystemPrompt(IEnumerable<(int PageNumber, int StartLine, string Text)> contextChunks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant that answers questions strictly based on the provided document excerpts.");
        sb.AppendLine("Rules:");
        sb.AppendLine("- Answer only using the context below.");
        sb.AppendLine("- Cite sources inline using [Page N, Line M] ONLY for the specific chunks whose text you directly used to form your answer.");
        sb.AppendLine("- Do NOT cite a chunk unless its excerpt actually contains information relevant to the answer.");
        sb.AppendLine("- Do NOT cite every chunk in the context — only the ones you drew from.");
        sb.AppendLine();
        sb.AppendLine("Context:");

        int i = 1;
        foreach (var (pageNumber, startLine, text) in contextChunks)
        {
            sb.AppendLine($"[Chunk {i} \u2014 Page {pageNumber}, Line {startLine}]");
            sb.AppendLine(text);
            sb.AppendLine();
            i++;
        }

        return sb.ToString();
    }

    private record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaMessage? Message);

    private record OllamaMessage(
        [property: JsonPropertyName("role")]    string Role,
        [property: JsonPropertyName("content")] string Content);
}
