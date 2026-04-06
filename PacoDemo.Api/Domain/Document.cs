namespace PacoDemo.Api.Domain;

public enum DocumentStatus { Pending, Processing, Ready, Failed }

public class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private Document() { }

    public static Document Create(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return new Document
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };
    }

    /// <summary>Reconstitutes a Document from persistent storage. Not for domain use.</summary>
    public static Document Reconstitute(Guid id, string fileName, string status, DateTime uploadedAt) =>
        new()
        {
            Id = id,
            FileName = fileName,
            Status = Enum.Parse<DocumentStatus>(status),
            UploadedAt = uploadedAt
        };

    public void MarkProcessing() => Status = DocumentStatus.Processing;
}
