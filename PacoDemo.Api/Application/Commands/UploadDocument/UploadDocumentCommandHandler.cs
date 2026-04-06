using LanguageExt.Common;
using MassTransit;
using MediatR;
using PacoDemo.Api.Common;
using PacoDemo.Api.Domain;
using PacoDemo.Api.Infrastructure;
using PacoDemo.Contracts.Commands;

namespace PacoDemo.Api.Application.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResponse>>
{
    private readonly IDocumentRepository _repository;
    private readonly LocalFileStorage _fileStorage;
    private readonly IPublishEndpoint _publishEndpoint;

    public UploadDocumentCommandHandler(
        IDocumentRepository repository,
        LocalFileStorage fileStorage,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<UploadDocumentResponse>> Handle(
        UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.FileName))
            return new Result<UploadDocumentResponse>(new BadRequestException("File name is required."));

        if (!command.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return new Result<UploadDocumentResponse>(new BadRequestException("Only PDF files are accepted."));

        using var ms = new MemoryStream();
        await command.Content.CopyToAsync(ms, cancellationToken);
        var fileContent = ms.ToArray();

        var document = Document.Create(command.FileName);
        await _repository.AddAsync(document);
        await _fileStorage.SaveAsync(document.Id, fileContent, cancellationToken);

        document.MarkProcessing();
        await _repository.UpdateStatusAsync(document.Id, document.Status);

        await _publishEndpoint.Publish(new ProcessDocumentCommand
        {
            DocumentId  = document.Id,
            FileName    = document.FileName,
            FileContent = fileContent
        }, cancellationToken);

        return new Result<UploadDocumentResponse>(
            new UploadDocumentResponse(document.Id, document.Status.ToString()));
    }
}
