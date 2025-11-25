using ECommerce.User.Application.Interfaces;
using ECommerce.User.Grpc;
using Grpc.Core;

namespace ECommerce.User.API.Grpc;

public class UserGrpcService : User.Grpc.UserGrpcService.UserGrpcServiceBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(
        IUserService userService,
        ILogger<UserGrpcService> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override async Task<UserInfoResponse> GetUserInfo(UserInfoRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserInfo called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
        }

        var user = await _userService.GetUserByIdAsync(userId, context.CancellationToken);
        
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));
        }

        return new UserInfoResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Username = user.Username ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            IsActive = user.IsActive,
            IsEmailVerified = user.EmailVerified
        };
    }

    public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ValidateUser called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new ValidateUserResponse
            {
                Valid = false,
                IsActive = false,
                Message = "Invalid user ID format"
            };
        }

        var user = await _userService.GetUserByIdAsync(userId, context.CancellationToken);
        
        if (user == null)
        {
            return new ValidateUserResponse
            {
                Valid = false,
                IsActive = false,
                Message = "User not found"
            };
        }

        if (!user.IsActive)
        {
            return new ValidateUserResponse
            {
                Valid = false,
                IsActive = false,
                Message = "User is not active"
            };
        }

        return new ValidateUserResponse
        {
            Valid = true,
            IsActive = true,
            Message = "User is valid"
        };
    }

    public override Task<UserAddressesResponse> GetUserAddresses(UserAddressesRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserAddresses called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
        }

        // TODO: Implement address service when addresses table is added
        var response = new UserAddressesResponse();
        
        // Return empty list for now
        return Task.FromResult(response);
    }
}
