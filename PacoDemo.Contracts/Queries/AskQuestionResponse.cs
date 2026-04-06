namespace PacoDemo.Contracts.Queries;

public record AskQuestionResponse
{
    public string Answer { get; init; } = string.Empty;
    public List<SourceChunk> Sources { get; init; } = new();
}

public record SourceChunk
{
    public int PageNumber { get; init; }
    public int LineNumber { get; init; }
    public string Excerpt { get; init; } = string.Empty;
    public int ChunkIndex { get; init; }
}
