using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ShoppingAgain.Events
{
    internal class EventContext : DbContext
    {
        internal DbSet<DBEvent> EventLog { get; set; }
        internal DbSet<State> CurrentState { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "EventLog.sqlite",
            };

            optionsBuilder.UseSqlite(new SqliteConnection(connectionStringBuilder.ToString()));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DBEvent>().ToTable("EventLog");
        }

    }
}
