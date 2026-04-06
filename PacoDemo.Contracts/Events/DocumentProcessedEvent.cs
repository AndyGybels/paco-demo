namespace PacoDemo.Contracts.Events;

public record DocumentProcessedEvent
{
    public Guid DocumentId { get; init; }
    public int ChunkCount { get; init; }
}
