namespace PacoDemo.Contracts.Commands;

public record ProcessDocumentCommand
{
    public Guid DocumentId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] FileContent { get; init; } = [];
}
