using System.Net.Http.Json;
using Confluent.Kafka;
using MessageBus;
using System.Text.Json;
using Topics;

namespace ChangeStreamManager
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IMessageBus messageBus)
        {
            _logger = logger;
            _configuration = configuration;
            _messageBus = messageBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                SaslUsername = _configuration["Kafka:SaslUsername"],
                SaslPassword = _configuration["Kafka:SaslPassword"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var topic = _configuration["Kafka:Topic"];
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation("Listening to Kafka topic: {Topic}. Routing to internal Message Bus.", topic);

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult != null)
                    {
                        var rawJson = consumeResult.Message.Value;

                        try
                        {
                            // 1. Parse the JSON into our C# model
                            var cdcEvent = JsonSerializer.Deserialize<CdcEvent>(rawJson, jsonOptions);

                            if (cdcEvent != null && !string.IsNullOrEmpty(cdcEvent.OperationType))
                            {
                                _logger.LogInformation("Parsed CDC Event: {OpType} on ID: {Id}",
                                    cdcEvent.OperationType, cdcEvent.DocumentKey?._id);
                                // Create an HTTP Client to bridge the microservices
                                using var httpClient = new HttpClient();

                                // Replace 'cdcEvent' below with whatever your deserialized variable is actually named 
                                // (e.g., if you named it parsedEvent, use parsedEvent instead of cdcEvent)
                                var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/CDCTargetApi/receive", cdcEvent);

                                if (response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("Successfully forwarded event to CDCTarget API!");
                                }

                                // 2. Publish to the internal bus (Parallel execution via our Fire-and-Forget logic)
                                await _messageBus.PublishAsync("cdc_events_topic", cdcEvent);
                            }
                        }
                        catch (Exception parseEx)
                        {
                            _logger.LogError(parseEx, "Failed to parse CDC Event JSON.");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}