using Microsoft.EntityFrameworkCore;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Infrastructure.Data.Configurations;

namespace SmartRoutine.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Routine> Routines { get; set; }
    public DbSet<RoutineLog> RoutineLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tüm entity configuration'ları otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
} 