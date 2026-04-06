using LanguageExt;

namespace PacoDemo.Api.Domain;

public interface IDocumentRepository
{
    Task AddAsync(Document document);
    Task<Option<Document>> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Document>> GetAllAsync();
    Task UpdateStatusAsync(Guid id, DocumentStatus status);
}
