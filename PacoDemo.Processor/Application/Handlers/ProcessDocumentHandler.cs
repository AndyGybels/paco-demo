using System.Text;
using PacoDemo.Processor.Domain.Aggregates;
using PacoDemo.Processor.Domain.Services;
using PacoDemo.Processor.Domain.ValueObjects;

namespace PacoDemo.Processor.Application.Handlers;

public class ProcessDocumentHandler
{
    private readonly ITextExtractor _textExtractor;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public ProcessDocumentHandler(
        ITextExtractor textExtractor,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore)
    {
        _textExtractor = textExtractor;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<Document> HandleAsync(
        Guid documentId, string fileName, byte[] fileContent, CancellationToken ct)
    {
        var id = DocumentId.From(documentId);
        var document = Document.Create(id, fileName);
        document.StartProcessing();

        try
        {
            var lines = _textExtractor.Extract(fileContent);
            var rawChunks = ChunkLines(lines).ToList();

            var chunksWithEmbeddings = new List<DocumentChunk>();
            foreach (var (chunkIndex, pageNumber, startLine, text) in rawChunks)
            {
                ct.ThrowIfCancellationRequested();
                var embedding = await _embeddingService.GetEmbeddingAsync(text, ct);
                chunksWithEmbeddings.Add(new DocumentChunk
                {
                    ChunkIndex = chunkIndex,
                    PageNumber = pageNumber,
                    StartLine  = startLine,
                    Text       = text,
                    Embedding  = embedding
                });
            }

            document.ApplyChunks(chunksWithEmbeddings);
            document.MarkReady();
            _vectorStore.Store(document);
        }
        catch
        {
            document.MarkFailed();
            throw;
        }

        return document;
    }

    private static IEnumerable<(int ChunkIndex, int PageNumber, int StartLine, string Text)> ChunkLines(
        IReadOnlyList<(int PageNumber, int LineNumber, string Text)> lines)
    {
        const int targetChunkChars = 500;
        const int overlapLines = 2;

        int globalIndex = 0;

        var pageGroups = lines
            .GroupBy(l => l.PageNumber)
            .OrderBy(g => g.Key);

        foreach (var pageGroup in pageGroups)
        {
            var pageLines = pageGroup.OrderBy(l => l.LineNumber).ToList();
            if (pageLines.Count == 0) continue;

            int start = 0;
            while (start < pageLines.Count)
            {
                var sb = new StringBuilder();
                int end = start;

                while (end < pageLines.Count)
                {
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(pageLines[end].Text);
                    end++;
                    if (sb.Length >= targetChunkChars) break;
                }

                var text = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    yield return (globalIndex++, pageGroup.Key, pageLines[start].LineNumber, text);

                if (end >= pageLines.Count) break;

                start = Math.Max(start + 1, end - overlapLines);
            }
        }
    }
}
