using PacoDemo.Processor.Domain.Services;
using UglyToad.PdfPig;

namespace PacoDemo.Processor.Infrastructure.Pdf;

public class PdfPigTextExtractor : ITextExtractor
{
    public IReadOnlyList<(int PageNumber, int LineNumber, string Text)> Extract(byte[] content)
    {
        var results = new List<(int, int, string)>();

        using var document = PdfDocument.Open(content);

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            if (words.Count == 0) continue;

            // Group words into visual lines by rounding their baseline Y to the nearest 2 points.
            // PDF Y-axis is bottom-up, so descending order gives top-to-bottom reading order.
            var lines = words
                .GroupBy(w => (int)Math.Round(w.BoundingBox.Bottom / 2.0) * 2)
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            for (int i = 0; i < lines.Count; i++)
                results.Add((page.Number, i + 1, lines[i]));
        }

        return results;
    }
}
