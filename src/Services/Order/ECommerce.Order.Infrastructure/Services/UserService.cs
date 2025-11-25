using System.Text.Json;
using ECommerce.Order.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserService(HttpClient httpClient, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info for userId: {UserId}. Status: {StatusCode}", 
                    userId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfoDto>(content, _jsonOptions);
            
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for userId: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ValidateUserExistsAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user exists for userId: {UserId}", userId);
            return false;
        }
    }
}
