using Microsoft.EntityFrameworkCore;
using StudentEvent.Domain.Entities;

namespace StudentEvent.Infra.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Requisito de Sênior: Índice único no MicrosoftId para buscas rápidas
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.MicrosoftId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}