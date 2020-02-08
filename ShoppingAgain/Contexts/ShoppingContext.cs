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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "Shopping.db",
            };

            optionsBuilder.UseSqlite(new SqliteConnection(connectionStringBuilder.ToString()));
        }
    }
}
