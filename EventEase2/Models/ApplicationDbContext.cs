using EventEase.Models;
using EventEase2.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase2.Data
{
    public class EventEaseContext : DbContext
    {
        public EventEaseContext(DbContextOptions<EventEaseContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed EventType data
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, TypeName = "Conference" },
                new EventType { EventTypeId = 2, TypeName = "Wedding" },
                new EventType { EventTypeId = 3, TypeName = "Concert" },
                new EventType { EventTypeId = 4, TypeName = "Workshop" },
                new EventType { EventTypeId = 5, TypeName = "Party" },
                new EventType { EventTypeId = 6, TypeName = "Exhibition" }
            );
        }
    }
}