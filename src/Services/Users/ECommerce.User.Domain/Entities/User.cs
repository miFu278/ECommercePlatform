using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.User.Domain.Entities;

public class User : BaseEntity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Email verification
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpires { get; set; }
    
    // Password reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
    
    // Account status
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // ISoftDeletable
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public bool IsEmailVerificationTokenValid => 
        !string.IsNullOrEmpty(EmailVerificationToken) && 
        EmailVerificationTokenExpires.HasValue && 
        EmailVerificationTokenExpires.Value > DateTime.UtcNow;
    
    public bool IsPasswordResetTokenValid => 
        !string.IsNullOrEmpty(PasswordResetToken) && 
        PasswordResetTokenExpires.HasValue && 
        PasswordResetTokenExpires.Value > DateTime.UtcNow;
    
    public bool IsLockedOut => 
        IsLocked && 
        LockoutEnd.HasValue && 
        LockoutEnd.Value > DateTime.UtcNow;
    
    public bool CanLogin => 
        IsActive && 
        !IsLockedOut && 
        EmailVerified;
    
    // Helper methods
    public void IncrementFailedLoginAttempts(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;
        
        if (FailedLoginAttempts >= maxAttempts)
        {
            IsLocked = true;
            LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }
    
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
    }
    
    public void UnlockAccount()
    {
        IsLocked = false;
        LockoutEnd = null;
        FailedLoginAttempts = 0;
    }
}
