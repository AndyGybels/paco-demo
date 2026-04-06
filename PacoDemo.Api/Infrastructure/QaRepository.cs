using System.Text.Json;
using Dapper;
using Npgsql;
using PacoDemo.Api.Domain;

namespace PacoDemo.Api.Infrastructure;

public class QaRepository : IQaRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public QaRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task AddAsync(QaEntry entry)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            """
            INSERT INTO qa_entries (id, document_id, question, answer, sources, asked_at)
            VALUES (@Id, @DocumentId, @Question, @Answer, @Sources, @AskedAt)
            """,
            new
            {
                entry.Id,
                entry.DocumentId,
                entry.Question,
                entry.Answer,
                Sources = JsonSerializer.Serialize(entry.Sources),
                entry.AskedAt
            });
    }

    public async Task<IReadOnlyList<QaEntry>> GetByDocumentIdAsync(Guid documentId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var rows = await conn.QueryAsync(
            """
            SELECT id, document_id, question, answer, sources, asked_at
            FROM qa_entries
            WHERE document_id = @DocumentId
            ORDER BY asked_at
            """,
            new { DocumentId = documentId });

        return rows.Select(row =>
        {
            var sources = JsonSerializer.Deserialize<List<QaSource>>((string)row.sources) ?? [];
            return QaEntry.Reconstitute(
                (Guid)row.id,
                (Guid)row.document_id,
                (string)row.question,
                (string)row.answer,
                sources,
                (DateTime)row.asked_at);
        }).ToList();
    }
}
