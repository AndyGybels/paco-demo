using LanguageExt;
using LanguageExt.Common;
using MassTransit;
using MediatR;
using PacoDemo.Api.Common;
using PacoDemo.Api.Domain;
using PacoDemo.Contracts.Queries;

namespace PacoDemo.Api.Application.Queries.AskQuestion;

public class AskQuestionQueryHandler : IRequestHandler<AskQuestionQuery, Result<QuestionResult>>
{
    private readonly IDocumentRepository _repository;
    private readonly IQaRepository _qaRepository;
    private readonly IRequestClient<AskQuestionRequest> _questionClient;

    public AskQuestionQueryHandler(
        IDocumentRepository repository,
        IQaRepository qaRepository,
        IRequestClient<AskQuestionRequest> questionClient)
    {
        _repository = repository;
        _qaRepository = qaRepository;
        _questionClient = questionClient;
    }

    public async Task<Result<QuestionResult>> Handle(
        AskQuestionQuery query, CancellationToken cancellationToken)
    {
        var optDoc = await _repository.GetByIdAsync(query.DocumentId);

        if (optDoc.IsNone)
            return new Result<QuestionResult>(
                new NotFoundException($"Document {query.DocumentId} not found."));

        var document = optDoc.Match(d => d, () => throw new InvalidOperationException());

        if (document.Status != DocumentStatus.Ready)
            return new Result<QuestionResult>(
                new ConflictException($"Document is not ready for questions (status: {document.Status})."));

        var response = await _questionClient.GetResponse<AskQuestionResponse>(
            new AskQuestionRequest
            {
                DocumentId = query.DocumentId,
                Question   = query.Question
            }, cancellationToken);

        var msg = response.Message;
        var sources = msg.Sources
            .Select(s => new QuestionSource(s.PageNumber, s.LineNumber, s.Excerpt, s.ChunkIndex))
            .ToList();

        await _qaRepository.AddAsync(QaEntry.Create(
            query.DocumentId,
            query.Question,
            msg.Answer,
            sources.Select(s => new QaSource(s.PageNumber, s.LineNumber, s.Excerpt, s.ChunkIndex)).ToList()));

        return new Result<QuestionResult>(new QuestionResult(msg.Answer, sources));
    }
}
