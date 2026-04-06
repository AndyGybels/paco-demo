using System.Collections.Concurrent;
using PacoDemo.Processor.Domain.Aggregates;
using PacoDemo.Processor.Domain.Services;
using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Infrastructure.VectorStore;

public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<Guid, Document> _store = new();

    public void Store(Document document) => _store[document.Id.Value] = document;

    public Document? Get(DocumentId id) =>
        _store.TryGetValue(id.Value, out var doc) ? doc : null;

    public IReadOnlyList<DocumentChunk> Search(DocumentId documentId, float[] queryEmbedding, int topK = 5)
    {
        if (!_store.TryGetValue(documentId.Value, out var document))
            return Array.Empty<DocumentChunk>();

        // Snapshot chunks to avoid any concurrent modification during iteration
        var chunks = document.Chunks.ToList();

        return chunks
            .Where(c => c.Embedding.Length > 0)
            .Select(c => (Chunk: c, Score: CosineSimilarity(queryEmbedding, c.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0f;

        float dot = 0f, magA = 0f, magB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot  += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        float denominator = MathF.Sqrt(magA) * MathF.Sqrt(magB);
        return denominator < 1e-10f ? 0f : dot / denominator;
    }
}
