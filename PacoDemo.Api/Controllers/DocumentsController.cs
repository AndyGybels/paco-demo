using MediatR;
using Microsoft.AspNetCore.Mvc;
using PacoDemo.Api.Application.Commands.UploadDocument;
using PacoDemo.Api.Application.Queries.GetAllDocuments;
using PacoDemo.Api.Application.Queries.GetDocument;
using PacoDemo.Api.Common;
using PacoDemo.Api.Infrastructure;

namespace PacoDemo.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly LocalFileStorage _fileStorage;

    public DocumentsController(ISender sender, LocalFileStorage fileStorage)
    {
        _sender = sender;
        _fileStorage = fileStorage;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct) =>
        (await _sender.Send(new UploadDocumentCommand(file.FileName, file.OpenReadStream()), ct))
            .ToActionResult();

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        (await _sender.Send(new GetAllDocumentsQuery(), ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        (await _sender.Send(new GetDocumentQuery(id), ct)).ToActionResult();

    [HttpGet("{id:guid}/file")]
    public IActionResult GetFile(Guid id)
    {
        if (!_fileStorage.Exists(id))
            return NotFound();

        return PhysicalFile(_fileStorage.GetPath(id), "application/pdf");
    }
}
