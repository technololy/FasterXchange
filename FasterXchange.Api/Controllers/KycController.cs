using FasterXchange.Application.Contracts.DTOs.Kyc;
using FasterXchange.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FasterXchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KycController : ControllerBase
{
    private readonly IKycService _kycService;
    private readonly ILogger<KycController> _logger;

    public KycController(IKycService kycService, ILogger<KycController> logger)
    {
        _kycService = kycService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartKycResponseDto>> StartKyc([FromBody] StartKycRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _kycService.StartKycVerificationAsync(userId, request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting KYC verification");
            return StatusCode(500, new StartKycResponseDto
            {
                Success = false,
                Message = "An error occurred while starting KYC verification"
            });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<KycStatusDto>> GetKycStatus()
    {
        try
        {
            var userId = GetUserId();
            var status = await _kycService.GetKycStatusAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KYC status");
            return StatusCode(500, new { message = "An error occurred while retrieving KYC status" });
        }
    }

    [HttpPost("resubmit")]
    public async Task<ActionResult<StartKycResponseDto>> ResubmitKyc([FromBody] StartKycRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _kycService.ResubmitKycAsync(userId, request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resubmitting KYC");
            return StatusCode(500, new StartKycResponseDto
            {
                Success = false,
                Message = "An error occurred while resubmitting KYC"
            });
        }
    }
}

