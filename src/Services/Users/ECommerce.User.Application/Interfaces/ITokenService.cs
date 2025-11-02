namespace ECommerce.User.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Domain.Entities.User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
}
