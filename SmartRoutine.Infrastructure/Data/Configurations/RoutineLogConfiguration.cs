using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoutine.Domain.Entities;

namespace SmartRoutine.Infrastructure.Data.Configurations;

public class RoutineLogConfiguration : IEntityTypeConfiguration<RoutineLog>
{
    public void Configure(EntityTypeBuilder<RoutineLog> builder)
    {
        builder.ToTable("RoutineLogs");
        
        builder.HasKey(rl => rl.Id);
        
        builder.Property(rl => rl.Id)
            .ValueGeneratedNever();
            
        builder.Property(rl => rl.RoutineId)
            .IsRequired();
            
        builder.Property(rl => rl.CompletedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(rl => rl.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(rl => rl.RoutineId)
            .HasDatabaseName("IX_RoutineLogs_RoutineId");
            
        builder.HasIndex(rl => rl.CompletedAt)
            .HasDatabaseName("IX_RoutineLogs_CompletedAt");
            
        builder.HasIndex(rl => new { rl.RoutineId, rl.CompletedAt })
            .HasDatabaseName("IX_RoutineLogs_RoutineId_CompletedAt");

        // Navigation properties
        builder.HasOne(rl => rl.Routine)
            .WithMany(r => r.RoutineLogs)
            .HasForeignKey(rl => rl.RoutineId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RoutineLogs_Routines_RoutineId");
    }
} 