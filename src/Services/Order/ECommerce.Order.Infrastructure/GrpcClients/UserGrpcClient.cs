using ECommerce.User.Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Infrastructure.GrpcClients;

public interface IUserGrpcClient
{
    Task<UserInfoResponse> GetUserInfoAsync(string userId);
    Task<ValidateUserResponse> ValidateUserAsync(string userId);
    Task<UserAddressesResponse> GetUserAddressesAsync(string userId);
}

public class UserGrpcClient : IUserGrpcClient
{
    private readonly UserGrpcService.UserGrpcServiceClient _client;
    private readonly ILogger<UserGrpcClient> _logger;

    public UserGrpcClient(IConfiguration configuration, ILogger<UserGrpcClient> logger)
    {
        _logger = logger;
        
        var grpcUrl = configuration["Services:User:GrpcUrl"] ?? "http://localhost:5010";
        var channel = GrpcChannel.ForAddress(grpcUrl);
        _client = new UserGrpcService.UserGrpcServiceClient(channel);
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string userId)
    {
        try
        {
            var request = new UserInfoRequest { UserId = userId };
            return await _client.GetUserInfoAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user info for {UserId}", userId);
            throw;
        }
    }

    public async Task<ValidateUserResponse> ValidateUserAsync(string userId)
    {
        try
        {
            var request = new ValidateUserRequest { UserId = userId };
            return await _client.ValidateUserAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserAddressesResponse> GetUserAddressesAsync(string userId)
    {
        try
        {
            var request = new UserAddressesRequest { UserId = userId };
            return await _client.GetUserAddressesAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user addresses for {UserId}", userId);
            throw;
        }
    }
}
