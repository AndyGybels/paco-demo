using LanguageExt;
using LanguageExt.Common;
using MediatR;
using PacoDemo.Api.Common;
using PacoDemo.Api.Domain;

namespace PacoDemo.Api.Application.Queries.GetDocument;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, Result<DocumentResponse>>
{
    private readonly IDocumentRepository _repository;

    public GetDocumentQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DocumentResponse>> Handle(
        GetDocumentQuery query, CancellationToken cancellationToken)
    {
        var optDoc = await _repository.GetByIdAsync(query.DocumentId);

        return optDoc.Match(
            Some: doc => new Result<DocumentResponse>(
                new DocumentResponse(doc.Id, doc.FileName, doc.Status.ToString(), doc.UploadedAt)),
            None: new Result<DocumentResponse>(
                new NotFoundException($"Document {query.DocumentId} not found.")));
    }
}
