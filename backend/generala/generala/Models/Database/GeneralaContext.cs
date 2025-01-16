using generala.Models.Database.Entities;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace generala.Models.Database;


public class GeneralaContext : DbContext
{

    private const string DATABASE_PATH = "Generala.db";

    // Tablas

    //public DbSet<Game_invitation> Game_invitation { get; set; }

    //public DbSet<Game_history> Game_history { get; set; }

   // public DbSet<Game> Game { get; set; }
    //public DbSet<Friend_request> Friend_request { get; set; }

    //public DbSet<Friendship> Friendship { get; set; }

    public DbSet<User> User { get; set; }

    // Crea archivo SQLite
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string basedir = AppDomain.CurrentDomain.BaseDirectory;
        optionsBuilder.UseSqlite($"DataSource={basedir}{DATABASE_PATH}");
    }

}