using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.User.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(new { message = "Registration successful. Please check your email to verify your account.", user });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _authService.VerifyEmailAsync(token, cancellationToken);
        return Ok(new { message = "Email verified successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(dto.Email, cancellationToken);
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, cancellationToken);
        return Ok(new { message = "Password reset successfully" });
    }
}

public record RefreshTokenRequest(string RefreshToken);
