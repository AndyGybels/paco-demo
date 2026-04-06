namespace PacoDemo.Processor.Domain.Services;

public interface ITextExtractor
{
    IReadOnlyList<(int PageNumber, int LineNumber, string Text)> Extract(byte[] content);
}
