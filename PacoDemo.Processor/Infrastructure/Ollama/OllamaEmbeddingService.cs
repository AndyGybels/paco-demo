using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PacoDemo.Processor.Domain.Services;

namespace PacoDemo.Processor.Infrastructure.Ollama;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;

    public OllamaEmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = "nomic-embed-text", prompt = text };

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
            cancellationToken: ct);

        return result?.Embedding ?? Array.Empty<float>();
    }

    private record OllamaEmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[] Embedding);
}
