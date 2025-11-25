using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.User.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        // Try to get from API Gateway header first (trusted)
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }
        }

        // Fallback to JWT claims (if called directly without Gateway)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userIdFromClaim))
        {
            return userIdFromClaim;
        }

        throw new UnauthorizedAccessException("User ID not found");
    }

    /// <summary>
    /// Get all active sessions for current user
    /// </summary>
    /// <param name="currentRefreshToken">Optional: Current refresh token to mark current session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Sessions retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetSessions(
        [FromQuery] string? currentRefreshToken = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var sessions = await _sessionService.GetUserSessionsAsync(userId, currentRefreshToken, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    /// <param name="sessionId">Session ID to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Session revoked successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Session not found</response>
    [HttpDelete("{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _sessionService.RevokeSessionAsync(userId, sessionId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Revoke all sessions except current one
    /// </summary>
    /// <param name="request">Optional: Current refresh token to keep active</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">All other sessions revoked successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("revoke-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RevokeAllSessions(
        [FromBody] RevokeAllSessionsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _sessionService.RevokeAllSessionsAsync(userId, request?.ExceptRefreshToken, cancellationToken);
        return Ok(new { message = "All other sessions have been revoked" });
    }
}

public record RevokeAllSessionsRequest(string? ExceptRefreshToken);
