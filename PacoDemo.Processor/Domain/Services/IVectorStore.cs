using PacoDemo.Processor.Domain.Aggregates;
using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Domain.Services;

public interface IVectorStore
{
    void Store(Document document);
    Document? Get(DocumentId id);
    IReadOnlyList<DocumentChunk> Search(DocumentId documentId, float[] queryEmbedding, int topK = 5);
}
