using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            ShoppingList list = new ShoppingList
            {
                ID = Guid.NewGuid(),
                Name = "Shopping",
            };

            Item item = new Item
            {
                Name = "Potatoes",
                State = ItemState.Wanted,
            };

            Password password = Password.Generate("demo");
            password.ID = Guid.NewGuid();
            Role UserRole = new Role { ID = Guid.NewGuid(), Name = "User" };
            Role AdminRole = new Role { ID = Guid.NewGuid(), Name = "Admin" };
            User demoUser = new User { 
                ID = Guid.NewGuid(),
                Name = "Demo",
                PasswordID = password.ID,
                CurrentListID = list.ID,
            };

            UserRole ur1 = new UserRole()
            {
                RoleId = UserRole.ID,
                UserId = demoUser.ID,
            };
            UserRole ur2 = new UserRole()
            {
                RoleId = AdminRole.ID,
                UserId = demoUser.ID,
            };

            modelBuilder.Entity<ShoppingList>()
                .ToTable("ShoppingLists")
                .HasData(list);

            modelBuilder.Entity<Item>()
                .ToTable("Items");

            modelBuilder.Entity<Password>()
                .ToTable("Password")
                .HasData(password);

            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasData(demoUser);

            modelBuilder.Entity<Role>()
                .ToTable("Roles")
                .HasData(UserRole, AdminRole);

            modelBuilder.Entity<UserRole>()
                .ToTable("UserRoles")
                .HasData(ur1, ur2); 
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<UserList>()
                .ToTable("UserLists")
                .HasData(new UserList { UserId = demoUser.ID, ListId = list.ID });
            modelBuilder.Entity<UserList>().HasKey(ul => new { ul.UserId, ul.ListId });
            modelBuilder.Entity<UserList>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.Lists)
                .HasForeignKey(ul => ul.UserId);
            modelBuilder.Entity<UserList>()
                .HasOne(ul => ul.List)
                .WithMany(l => l.Users)
                .HasForeignKey(ul => ul.ListId);

        }
    }
}
