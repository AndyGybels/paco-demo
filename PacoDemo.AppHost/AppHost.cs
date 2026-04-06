var builder = DistributedApplication.CreateBuilder(args);

var rabbitMq = builder.AddRabbitMQ("pacodemo-rabbitmq");

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

var api = builder.AddProject<Projects.PacoDemo_Api>("pacodemo-api")
    .WaitFor(rabbitMq)
    .WithReference(rabbitMq)
    .WaitFor(postgresdb)
    .WithReference(postgresdb);

builder.AddProject<Projects.PacoDemo_Processor>("pacodemo-processor")
    .WaitFor(rabbitMq)
    .WithReference(rabbitMq)
    .WithReference(api);

builder.Build().Run();
