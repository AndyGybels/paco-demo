namespace PacoDemo.Api.Infrastructure;

public class LocalFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:BasePath"]
            ?? Path.Combine(Path.GetTempPath(), "pacodemo-pdfs");
        Directory.CreateDirectory(_basePath);
    }

    public async Task SaveAsync(Guid documentId, byte[] content, CancellationToken ct = default) =>
        await File.WriteAllBytesAsync(GetPath(documentId), content, ct);

    public string GetPath(Guid documentId) =>
        Path.Combine(_basePath, $"{documentId}.pdf");

    public bool Exists(Guid documentId) =>
        File.Exists(GetPath(documentId));
}
