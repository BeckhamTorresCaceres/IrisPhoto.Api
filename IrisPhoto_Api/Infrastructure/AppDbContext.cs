using IrisPhoto_Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IrisPhoto_Api.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tus DbSets siguen estando aquí
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ¡Magia! Aplica automáticamente todos los *Mapping.cs que crees en el ensamblado
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
