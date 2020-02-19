using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Contexts
{
    public class ShoppingContext : DbContext
    {
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "shopping.sqlite",
            };

            optionsBuilder.UseSqlite(new SqliteConnection(connectionStringBuilder.ToString()));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingList>()
                .ToTable("ShoppingLists");
            modelBuilder.Entity<Item>().ToTable("Items");

            Password password = Password.Generate("demo");
            password.ID = Guid.NewGuid();

            modelBuilder.Entity<Password>()
                .ToTable("Password")
                .HasData(password);
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasData(new User {
                    ID = Guid.NewGuid(),
                    Name = "Anonymous",
                    PasswordID = password.ID,
                });
        }
    }
}
