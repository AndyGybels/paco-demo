using LanguageExt.Common;
using MediatR;

namespace PacoDemo.Api.Application.Queries.GetDocument;

public record GetDocumentQuery(Guid DocumentId) : IRequest<Result<DocumentResponse>>;

public record DocumentResponse(Guid Id, string FileName, string Status, DateTime UploadedAt);
