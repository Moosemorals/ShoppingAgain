using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;
using System;

namespace ShoppingAgain.Database
{
    public class ShoppingContext : DbContext
    {
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Option> Options { get; set; }


        private readonly IConfiguration conf;
        public ShoppingContext() : base()
        {
            conf = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", false, false)
                .Build();
        } 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = conf[Names.DatabasePath],
            };

            optionsBuilder.UseSqlite(new SqliteConnection(connectionStringBuilder.ToString()));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Option>()
                .ToTable("Options");

            modelBuilder.Entity<ShoppingList>()
                .ToTable("ShoppingLists");

            modelBuilder.Entity<Item>()
                .ToTable("Items");

            modelBuilder.Entity<Password>()
                .ToTable("Password");

            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<Role>()
                .ToTable("Roles")
                .HasData(
                    new Role {ID = Guid.NewGuid(), Name = Names.RoleAdmin }, 
                    new Role {ID = Guid.NewGuid(), Name = Names.RoleUser }
                );

            modelBuilder.Entity<UserFriend>()
                .ToTable("UserFriends");
            modelBuilder.Entity<UserFriend>().HasKey(uf => new { uf.UserID, uf.FriendID });
            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(uf => uf.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.Friend)
                .WithMany(u => u.FriendBack)
                .HasForeignKey(uf => uf.FriendID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .ToTable("UserRoles");
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserList>()
                .ToTable("UserLists");
            modelBuilder.Entity<UserList>().HasKey(ul => new { ul.UserId, ul.ListId });
            modelBuilder.Entity<UserList>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.Lists)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserList>()
                .HasOne(ul => ul.List)
                .WithMany(l => l.Users)
                .HasForeignKey(ul => ul.ListId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
