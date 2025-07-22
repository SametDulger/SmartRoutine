using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Infrastructure.Data.Configurations;

public class RoutineConfiguration : IEntityTypeConfiguration<Routine>
{
    public void Configure(EntityTypeBuilder<Routine> builder)
    {
        builder.ToTable("Routines");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedNever();
            
        builder.Property(r => r.UserId)
            .IsRequired();
            
        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasConversion(
                title => title.Value,
                value => RoutineTitle.Create(value)
            );
            
        builder.Property(r => r.Description)
            .HasMaxLength(1000);
            
        // TimeOfDay ile ilgili tüm property mapping ve value converter kaldırıldı
            
        builder.Property(r => r.RepeatType)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Routines_UserId");
            
        builder.HasIndex(r => new { r.UserId, r.IsActive })
            .HasDatabaseName("IX_Routines_UserId_IsActive");

        // Navigation properties
        builder.HasOne(r => r.User)
            .WithMany(u => u.Routines)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Routines_Users_UserId");
            
        builder.HasMany(r => r.RoutineLogs)
            .WithOne(rl => rl.Routine)
            .HasForeignKey(rl => rl.RoutineId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RoutineLogs_Routines_RoutineId");
    }
} 