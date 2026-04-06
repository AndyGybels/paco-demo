using LanguageExt.Common;
using MediatR;

namespace PacoDemo.Api.Application.Commands.UploadDocument;

public record UploadDocumentCommand(string FileName, Stream Content) : IRequest<Result<UploadDocumentResponse>>;

public record UploadDocumentResponse(Guid DocumentId, string Status);
