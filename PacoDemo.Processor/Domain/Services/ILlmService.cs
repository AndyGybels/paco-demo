namespace PacoDemo.Processor.Domain.Services;

public interface ILlmService
{
    Task<string> AskAsync(
        string question,
        IEnumerable<(int PageNumber, int StartLine, string Text)> contextChunks,
        CancellationToken ct = default);
}
