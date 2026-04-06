using MassTransit;
using PacoDemo.Contracts.Queries;
using PacoDemo.Processor.Application.Handlers;

namespace PacoDemo.Processor.Consumers;

public class AskQuestionConsumer : IConsumer<AskQuestionRequest>
{
    private readonly AskQuestionHandler _handler;
    private readonly ILogger<AskQuestionConsumer> _logger;

    public AskQuestionConsumer(AskQuestionHandler handler, ILogger<AskQuestionConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AskQuestionRequest> context)
    {
        var req = context.Message;
        _logger.LogInformation("Question for document {DocumentId}: {Question}", req.DocumentId, req.Question);

        var (answer, sources) = await _handler.HandleAsync(
            req.DocumentId, req.Question, context.CancellationToken);

        await context.RespondAsync(new AskQuestionResponse
        {
            Answer = answer,
            Sources = sources.Select(c => new SourceChunk
            {
                PageNumber = c.PageNumber,
                LineNumber = c.StartLine,
                Excerpt    = c.Excerpt,
                ChunkIndex = c.ChunkIndex
            }).ToList()
        });
    }
}
