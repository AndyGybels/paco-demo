using MassTransit;
using PacoDemo.Processor.Application.Handlers;
using PacoDemo.Processor.Consumers;
using PacoDemo.Processor.Domain.Services;
using PacoDemo.Processor.Infrastructure.Ollama;
using PacoDemo.Processor.Infrastructure.Pdf;
using PacoDemo.Processor.Infrastructure.VectorStore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Infrastructure — singletons that hold state across messages
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<ITextExtractor, PdfPigTextExtractor>();

// Ollama HTTP clients — typed clients registered via IHttpClientFactory
builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Application handlers
builder.Services.AddScoped<ProcessDocumentHandler>();
builder.Services.AddScoped<AskQuestionHandler>();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessDocumentConsumer>();
    x.AddConsumer<AskQuestionConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var configService = ctx.GetRequiredService<IConfiguration>();
        var connectionString = configService.GetConnectionString("pacodemo-rabbitmq");
        
        cfg.Host(connectionString);

        cfg.ReceiveEndpoint("process-document", e =>
        {
            e.ConfigureConsumer<ProcessDocumentConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("ask-question", e =>
        {
            e.ConfigureConsumer<AskQuestionConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
