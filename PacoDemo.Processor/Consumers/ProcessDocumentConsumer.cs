using MassTransit;
using PacoDemo.Contracts.Commands;
using PacoDemo.Contracts.Events;
using PacoDemo.Processor.Application.Handlers;

namespace PacoDemo.Processor.Consumers;

public class ProcessDocumentConsumer : IConsumer<ProcessDocumentCommand>
{
    private readonly ProcessDocumentHandler _handler;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessDocumentConsumer> _logger;

    public ProcessDocumentConsumer(
        ProcessDocumentHandler handler,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessDocumentConsumer> logger)
    {
        _handler = handler;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessDocumentCommand> context)
    {
        var cmd = context.Message;
        _logger.LogInformation("Processing document {DocumentId} ({FileName})", cmd.DocumentId, cmd.FileName);

        var document = await _handler.HandleAsync(
            cmd.DocumentId,
            cmd.FileName,
            cmd.FileContent,
            context.CancellationToken);

        await _publishEndpoint.Publish(new DocumentProcessedEvent
        {
            DocumentId = cmd.DocumentId,
            ChunkCount = document.Chunks.Count
        });

        _logger.LogInformation("Document {DocumentId} ready — {ChunkCount} chunks", cmd.DocumentId, document.Chunks.Count);
    }
}
