using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.User.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ILogger<AddressController> _logger;

    public AddressController(IAddressService addressService, ILogger<AddressController> logger)
    {
        _addressService = addressService;
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
    /// Get all addresses for current user
    /// </summary>
    /// <response code="200">Addresses retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddresses(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var addresses = await _addressService.GetUserAddressesAsync(userId, cancellationToken);
        return Ok(addresses);
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Address retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Address doesn't belong to user</response>
    /// <response code="404">Address not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> GetAddressById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var address = await _addressService.GetAddressByIdAsync(userId, id, cancellationToken);
        
        if (address == null)
            return NotFound(new { message = "Address not found" });

        return Ok(address);
    }

    /// <summary>
    /// Create new address
    /// </summary>
    /// <param name="dto">Address information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">Address created successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var address = await _addressService.CreateAddressAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAddressById), new { id = address.Id }, address);
    }

    /// <summary>
    /// Update address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="dto">Updated address information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Address updated successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Address doesn't belong to user</response>
    /// <response code="404">Address not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> UpdateAddress(Guid id, [FromBody] UpdateAddressDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var address = await _addressService.UpdateAddressAsync(userId, id, dto, cancellationToken);
        return Ok(address);
    }

    /// <summary>
    /// Delete address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Address deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Address doesn't belong to user</response>
    /// <response code="404">Address not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _addressService.DeleteAddressAsync(userId, id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Set address as default
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Address set as default successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Address doesn't belong to user</response>
    /// <response code="404">Address not found</response>
    [HttpPut("{id:guid}/set-default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var address = await _addressService.SetDefaultAddressAsync(userId, id, cancellationToken);
        return Ok(address);
    }
}
