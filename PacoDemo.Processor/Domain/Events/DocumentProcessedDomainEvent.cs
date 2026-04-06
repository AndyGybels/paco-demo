using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Domain.Events;

public record DocumentProcessedDomainEvent(DocumentId DocumentId, int ChunkCount, DateTime OccurredAt);
