using CDCTarget.Data;
using CDCTarget.Models;

namespace CDCTarget.Modules
{
    public class Logger
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public Logger(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void LogSystemEvent(string message, string source)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.SubscriptionEvents.Add(new SubscriptionEvent
            {
                Message = message,
                Source = source,
                Type = "INFO"
            });
            db.SaveChanges();
        }
    }
}