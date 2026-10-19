using CDCTarget.Models;
using Microsoft.EntityFrameworkCore;

namespace CDCTarget.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // These properties represent your actual database tables
        public DbSet<TopicEvent> TopicEvents { get; set; }
        public DbSet<SubscriptionEvent> SubscriptionEvents { get; set; }
    }
}