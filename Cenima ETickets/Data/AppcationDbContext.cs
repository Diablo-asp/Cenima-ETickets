using Cinema_ETickets.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Cinema_ETickets.ViewModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Cinema_ETickets.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {            
        }

        public DbSet<Movie> movies { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Cenima> cenimas { get; set; }
        public DbSet<Actor> actors { get; set; }
        public DbSet<ActorMovie> ActorMovies { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public ApplicationDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=.;DataBase=Cinema_ETickets;Integrated Security=True;" +
                                "Trust Server Certificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ActorMovie>() 
                .HasKey(am => new { am.ActorId, am.MovieId });

            modelBuilder.Entity<Actor>()
                .HasMany(e => e.movies)
                .WithMany(e => e.actors)
                .UsingEntity<ActorMovie>();
        }
    
    }
}
