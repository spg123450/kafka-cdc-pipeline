using CDCTarget.Data;
using CDCTarget.Models;
using MessageBus;
using System.Text.Json;
using Topics;

namespace CDCTarget.Modules
{
    public class EventManager
    {
        private readonly IMessageBus _messageBus;
        private readonly Logger _logger;
        private readonly Processor _processor;
        private readonly ThroughputDetector _throughputDetector;
        private readonly IServiceScopeFactory _scopeFactory;

        public EventManager(
            IMessageBus messageBus, Logger logger,
            Processor processor, ThroughputDetector throughputDetector,
            IServiceScopeFactory scopeFactory)
        {
            _messageBus = messageBus;
            _logger = logger;
            _processor = processor;
            _throughputDetector = throughputDetector;
            _scopeFactory = scopeFactory;
        }

        public void StartListening()
        {
            _logger.LogSystemEvent("Event Manager initialized and listening to internal bus.", "CDC Target Startup");
            _messageBus.Subscribe<CdcEvent>("cdc_events_topic", HandleIncomingPayload);
            Console.WriteLine("[EVENT MANAGER] Ready and waiting for CDC payloads...");
        }

        private void HandleIncomingPayload(CdcEvent payload)
        {
            // 1. Save directly to SQLite (Storage)
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.TopicEvents.Add(new TopicEvent
            {
                OperationType = payload.OperationType,
                DocumentId = payload.DocumentKey?._id,
                PayloadData = JsonSerializer.Serialize(payload)
            });
            db.SaveChanges();

            // 2. Distribute to other modules
            _throughputDetector.RecordEventSpike();
            _processor.ProcessAndSync(payload);
        }
    }
}