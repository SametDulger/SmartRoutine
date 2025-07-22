using System.ComponentModel.DataAnnotations;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Domain.Entities;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public class User : BaseEntity
{
    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    [Required]
    public Email Email { get; set; } = null!;
    /// <summary>
    /// Kullanıcı adı/isim
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; private set; } = string.Empty;

    [Required]
    public UserRole Role { get; private set; } = UserRole.User;
    
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Routine> Routines { get; private set; } = new List<Routine>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { Email = Email.Create("dummy@dummy.com"); DisplayName = ""; } // EF Core constructor

    public User(Email email, string passwordHash, UserRole role = UserRole.User, string displayName = "") : base()
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        DisplayName = displayName;
        
        // Raise domain event
        AddDomainEvent(new UserRegisteredEvent(Id, email.Value));
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
        SetUpdatedAt();
    }

    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            throw new ArgumentException("Display name cannot be empty.");
        }
        
        DisplayName = newDisplayName;
        SetUpdatedAt();
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdatedAt();
    }

    public void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            SetUpdatedAt();
        }
    }
} 