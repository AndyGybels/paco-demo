using Dapper;
using Npgsql;

namespace PacoDemo.Api.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS documents (
                id          UUID        PRIMARY KEY,
                file_name   TEXT        NOT NULL,
                status      TEXT        NOT NULL,
                uploaded_at TIMESTAMPTZ NOT NULL
            );

            CREATE TABLE IF NOT EXISTS qa_entries (
                id          UUID        PRIMARY KEY,
                document_id UUID        NOT NULL REFERENCES documents(id),
                question    TEXT        NOT NULL,
                answer      TEXT        NOT NULL,
                sources     TEXT        NOT NULL DEFAULT '[]',
                asked_at    TIMESTAMPTZ NOT NULL
            );
            """);
    }
}
