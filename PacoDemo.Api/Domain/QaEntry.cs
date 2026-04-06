namespace PacoDemo.Api.Domain;

public record QaSource(int PageNumber, int LineNumber, string Excerpt, int ChunkIndex);

public class QaEntry
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public IReadOnlyList<QaSource> Sources { get; private set; } = [];
    public DateTime AskedAt { get; private set; }

    private QaEntry() { }

    public static QaEntry Create(Guid documentId, string question, string answer, IReadOnlyList<QaSource> sources) =>
        new()
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Question = question,
            Answer = answer,
            Sources = sources,
            AskedAt = DateTime.UtcNow
        };

    public static QaEntry Reconstitute(Guid id, Guid documentId, string question, string answer, IReadOnlyList<QaSource> sources, DateTime askedAt) =>
        new()
        {
            Id = id,
            DocumentId = documentId,
            Question = question,
            Answer = answer,
            Sources = sources,
            AskedAt = askedAt
        };
}
