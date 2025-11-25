namespace ECommerce.Order.Application.Interfaces;

public interface IUserService
{
    Task<UserInfoDto?> GetUserInfoAsync(Guid userId);
    Task<bool> ValidateUserExistsAsync(Guid userId);
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
