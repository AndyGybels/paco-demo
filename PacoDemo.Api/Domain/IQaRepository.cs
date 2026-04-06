namespace PacoDemo.Api.Domain;

public interface IQaRepository
{
    Task AddAsync(QaEntry entry);
    Task<IReadOnlyList<QaEntry>> GetByDocumentIdAsync(Guid documentId);
}
