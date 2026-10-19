using System.ComponentModel.DataAnnotations;

namespace CDCTarget.Models
{
    public class TopicEvent
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? OperationType { get; set; }
        public string? DocumentId { get; set; }
        public string? PayloadData { get; set; }
    }
}