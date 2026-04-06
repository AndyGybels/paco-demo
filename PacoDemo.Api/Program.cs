using System.Reflection;
using MassTransit;
using Npgsql;
using PacoDemo.Api.Application.Consumers;
using PacoDemo.Api.Domain;
using PacoDemo.Api.Hubs;
using PacoDemo.Api.Infrastructure;
using PacoDemo.Contracts.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddSingleton<LocalFileStorage>();
builder.Services.AddScoped<IQaRepository, QaRepository>();

builder.AddRabbitMQClient("pacodemo-rabbitmq");
builder.AddNpgsqlDataSource(connectionName: "postgresdb");

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DocumentProcessedConsumer>();
    x.AddRequestClient<AskQuestionRequest>(new Uri("queue:ask-question"));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var configService = ctx.GetRequiredService<IConfiguration>();
        var connectionString = configService.GetConnectionString("pacodemo-rabbitmq");

        cfg.Host(connectionString);

        cfg.ReceiveEndpoint("document-processed-api", e =>
        {
            e.ConfigureConsumer<DocumentProcessedConsumer>(ctx);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await DatabaseInitializer.EnsureCreatedAsync(
    app.Services.GetRequiredService<NpgsqlDataSource>());

app.MapDefaultEndpoints();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<DocumentHub>("/hubs/documents");
app.MapFallbackToFile("index.html");

app.Run();
