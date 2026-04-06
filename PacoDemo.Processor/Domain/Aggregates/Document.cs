using PacoDemo.Processor.Domain.Events;
using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Domain.Aggregates;

public enum DocumentStatus { Pending, Processing, Ready, Failed }

public class Document
{
    public DocumentId Id { get; private set; } = null!;
    public string FileName { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public IReadOnlyList<DocumentChunk> Chunks => _chunks.AsReadOnly();

    private readonly List<DocumentChunk> _chunks = new();
    private readonly List<object> _domainEvents = new();

    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private Document() { }

    public static Document Create(DocumentId id, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return new Document
        {
            Id = id,
            FileName = fileName,
            Status = DocumentStatus.Pending
        };
    }

    public void StartProcessing()
    {
        if (Status != DocumentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing a document in state {Status}.");
        Status = DocumentStatus.Processing;
    }

    public void ApplyChunks(IEnumerable<DocumentChunk> chunks)
    {
        if (Status != DocumentStatus.Processing)
            throw new InvalidOperationException("Chunks can only be applied while document is Processing.");
        _chunks.Clear();
        _chunks.AddRange(chunks);
    }

    public void MarkReady()
    {
        if (Status != DocumentStatus.Processing)
            throw new InvalidOperationException("Cannot mark ready unless Processing.");
        if (_chunks.Count == 0)
            throw new InvalidOperationException("Cannot mark ready with no chunks.");

        Status = DocumentStatus.Ready;
        _domainEvents.Add(new DocumentProcessedDomainEvent(Id, _chunks.Count, DateTime.UtcNow));
    }

    public void MarkFailed()
    {
        Status = DocumentStatus.Failed;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
