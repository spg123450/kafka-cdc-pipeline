using ChangeStreamManager;
using MessageBus;
using ServiceMessageBus;

var builder = Host.CreateApplicationBuilder(args);

// Register the Singleton Message Bus
builder.Services.AddSingleton<IMessageBus, InMemoryMessageBus>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();