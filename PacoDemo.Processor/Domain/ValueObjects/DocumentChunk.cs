namespace PacoDemo.Processor.Domain.ValueObjects;

public record DocumentChunk
{
    public int ChunkIndex { get; init; }
    public int PageNumber { get; init; }
    public int StartLine { get; init; }
    public string Text { get; init; } = string.Empty;
    public float[] Embedding { get; init; } = Array.Empty<float>();

    public string Excerpt => Text.Length > 200 ? Text[..200] + "\u2026" : Text;
}
