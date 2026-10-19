using System.ComponentModel.DataAnnotations;

namespace CDCTarget.Models
{
    public class SubscriptionEvent
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? Source { get; set; }
    }
}