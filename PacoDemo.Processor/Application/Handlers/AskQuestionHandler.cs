using System.Text.RegularExpressions;
using PacoDemo.Processor.Domain.Services;
using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Application.Handlers;

public class AskQuestionHandler
{
    private static readonly Regex CitationRegex =
        new(@"\[Page\s+(\d+),\s*Line\s+(\d+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmService _llmService;

    public AskQuestionHandler(
        IVectorStore vectorStore,
        IEmbeddingService embeddingService,
        ILlmService llmService)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _llmService = llmService;
    }

    public async Task<(string Answer, IReadOnlyList<DocumentChunk> Sources)> HandleAsync(
        Guid documentId, string question, CancellationToken ct)
    {
        var id = DocumentId.From(documentId);

        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(question, ct);
        var topChunks = _vectorStore.Search(id, queryEmbedding, topK: 5);

        if (topChunks.Count == 0)
            return ("No relevant content found for this document.", Array.Empty<DocumentChunk>());

        var context = topChunks.Select(c => (c.PageNumber, c.StartLine, c.Text));
        var answer = await _llmService.AskAsync(question, context, ct);

        // Filter sources to only chunks the LLM actually cited.
        var cited = CitationRegex.Matches(answer)
            .Select(m => (Page: int.Parse(m.Groups[1].Value), Line: int.Parse(m.Groups[2].Value)))
            .ToHashSet();

        var sources = topChunks
            .Where(c => cited.Contains((c.PageNumber, c.StartLine)))
            .GroupBy(c => (c.PageNumber, c.StartLine))
            .Select(g => g.First())
            .ToList();

        if (sources.Count == 0 && !answer.Contains("I don't know", StringComparison.OrdinalIgnoreCase))
            sources = topChunks.Take(1).ToList();

        var cleanAnswer = CitationRegex.Replace(answer, "").Trim();

        return (cleanAnswer, sources);
    }
}
