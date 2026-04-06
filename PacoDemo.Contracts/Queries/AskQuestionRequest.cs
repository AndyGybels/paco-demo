namespace PacoDemo.Contracts.Queries;

public record AskQuestionRequest
{
    public Guid DocumentId { get; init; }
    public string Question { get; init; } = string.Empty;
}
