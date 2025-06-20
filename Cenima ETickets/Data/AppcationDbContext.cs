﻿using Cenima_ETickets.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Cenima_ETickets.ViewModel;

namespace Cenima_ETickets.Data
{
    public class AppcationDbContext : DbContext
    {
        public DbSet<Movie> movies { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Cenima> cenimas { get; set; }
        public DbSet<Actor> actors { get; set; }
        public DbSet<ActorMovie> ActorMovies { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=.;DataBase=Cenima_ETickets;Integrated Security=True;" +
                                "Trust Server Certificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActorMovie>()
                .HasKey(am => new { am.ActorId, am.MovieId });

            modelBuilder.Entity<Actor>()
                .HasMany(e => e.movies)
                .WithMany(e => e.actors)
                .UsingEntity<ActorMovie>();
        }
    
    }
}
