using LanguageExt.Common;
using MediatR;
using PacoDemo.Api.Application.Queries.GetDocument;

namespace PacoDemo.Api.Application.Queries.GetAllDocuments;

public record GetAllDocumentsQuery : IRequest<Result<IReadOnlyList<DocumentResponse>>>;
