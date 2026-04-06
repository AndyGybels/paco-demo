using MassTransit;
using Microsoft.AspNetCore.SignalR;
using PacoDemo.Api.Domain;
using PacoDemo.Api.Hubs;
using PacoDemo.Contracts.Events;

namespace PacoDemo.Api.Application.Consumers;

public class DocumentProcessedConsumer : IConsumer<DocumentProcessedEvent>
{
    private readonly IDocumentRepository _repository;
    private readonly IHubContext<DocumentHub> _hubContext;
    private readonly ILogger<DocumentProcessedConsumer> _logger;

    public DocumentProcessedConsumer(
        IDocumentRepository repository,
        IHubContext<DocumentHub> hubContext,
        ILogger<DocumentProcessedConsumer> logger)
    {
        _repository = repository;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentProcessedEvent> context)
    {
        var evt = context.Message;

        await _repository.UpdateStatusAsync(evt.DocumentId, DocumentStatus.Ready);

        await _hubContext.Clients.All.SendAsync(
            "DocumentStatusChanged",
            evt.DocumentId,
            DocumentStatus.Ready.ToString());

        _logger.LogInformation("Document {DocumentId} marked as Ready ({ChunkCount} chunks)",
            evt.DocumentId, evt.ChunkCount);
    }
}
