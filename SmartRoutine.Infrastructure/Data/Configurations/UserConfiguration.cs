using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id)
            .ValueGeneratedNever();
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));
            
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        // Navigation properties
        builder.HasMany(u => u.Routines)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Routines_Users_UserId");
    }
} 