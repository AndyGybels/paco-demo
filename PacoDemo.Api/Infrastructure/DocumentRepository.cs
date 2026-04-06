using Dapper;
using LanguageExt;
using Npgsql;
using PacoDemo.Api.Domain;

namespace PacoDemo.Api.Infrastructure;

public class DocumentRepository : IDocumentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DocumentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddAsync(Document document)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            """
            INSERT INTO documents (id, file_name, status, uploaded_at)
            VALUES (@Id, @FileName, @Status, @UploadedAt)
            """,
            new
            {
                document.Id,
                document.FileName,
                Status = document.Status.ToString(),
                document.UploadedAt
            });
    }

    public async Task<Option<Document>> GetByIdAsync(Guid id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var row = await conn.QuerySingleOrDefaultAsync(
            "SELECT id, file_name, status, uploaded_at FROM documents WHERE id = @Id",
            new { Id = id });

        if (row is null) return Option<Document>.None;

        return Option<Document>.Some(Document.Reconstitute(
            (Guid)row.id,
            (string)row.file_name,
            (string)row.status,
            (DateTime)row.uploaded_at));
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync()
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var rows = await conn.QueryAsync(
            "SELECT id, file_name, status, uploaded_at FROM documents ORDER BY uploaded_at DESC");

        return rows.Select(row => Document.Reconstitute(
                (Guid)row.id,
                (string)row.file_name,
                (string)row.status,
                (DateTime)row.uploaded_at))
            .ToList();
    }

    public async Task UpdateStatusAsync(Guid id, DocumentStatus status)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "UPDATE documents SET status = @Status WHERE id = @Id",
            new { Id = id, Status = status.ToString() });
    }
}
