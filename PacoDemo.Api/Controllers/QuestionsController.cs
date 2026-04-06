using MediatR;
using Microsoft.AspNetCore.Mvc;
using PacoDemo.Api.Application.Queries.AskQuestion;
using PacoDemo.Api.Application.Queries.GetConversation;
using PacoDemo.Api.Common;

namespace PacoDemo.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly ISender _sender;

    public QuestionsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken ct) =>
        (await _sender.Send(new AskQuestionQuery(request.DocumentId, request.Question), ct))
            .ToActionResult();

    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> GetConversation(Guid documentId, CancellationToken ct) =>
        (await _sender.Send(new GetConversationQuery(documentId), ct)).ToActionResult();
}

public record AskRequest(Guid DocumentId, string Question);
