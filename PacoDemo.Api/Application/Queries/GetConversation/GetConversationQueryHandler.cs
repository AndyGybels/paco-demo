using LanguageExt.Common;
using MediatR;
using PacoDemo.Api.Application.Queries.AskQuestion;
using PacoDemo.Api.Domain;

namespace PacoDemo.Api.Application.Queries.GetConversation;

public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, Result<IReadOnlyList<ConversationEntry>>>
{
    private readonly IQaRepository _qaRepository;

    public GetConversationQueryHandler(IQaRepository qaRepository) => _qaRepository = qaRepository;

    public async Task<Result<IReadOnlyList<ConversationEntry>>> Handle(
        GetConversationQuery query, CancellationToken cancellationToken)
    {
        var entries = await _qaRepository.GetByDocumentIdAsync(query.DocumentId);

        IReadOnlyList<ConversationEntry> result = entries
            .Select(e => new ConversationEntry(
                e.Id,
                e.Question,
                e.Answer,
                e.Sources.Select(s => new QuestionSource(s.PageNumber, s.LineNumber, s.Excerpt, s.ChunkIndex)).ToList(),
                e.AskedAt))
            .ToList();

        return new Result<IReadOnlyList<ConversationEntry>>(result);
    }
}
