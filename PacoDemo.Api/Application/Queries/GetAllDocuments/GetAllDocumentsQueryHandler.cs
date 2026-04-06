using LanguageExt.Common;
using MediatR;
using PacoDemo.Api.Application.Queries.GetDocument;
using PacoDemo.Api.Domain;

namespace PacoDemo.Api.Application.Queries.GetAllDocuments;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, Result<IReadOnlyList<DocumentResponse>>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<DocumentResponse>>> Handle(
        GetAllDocumentsQuery query, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetAllAsync();

        IReadOnlyList<DocumentResponse> result = documents
            .Select(d => new DocumentResponse(d.Id, d.FileName, d.Status.ToString(), d.UploadedAt))
            .ToList();

        return new Result<IReadOnlyList<DocumentResponse>>(result);
    }
}
