using FasterXchange.Application.Contracts.DTOs.Auth;
using FasterXchange.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FasterXchange.Api.Controllers;

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
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new RegisterResponseDto
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // Get IP address and user agent
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            request.UserAgent = Request.Headers["User-Agent"].ToString();

            var response = await _authService.LoginAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<VerifyOtpResponseDto>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        try
        {
            // Get IP address and user agent
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            request.UserAgent = Request.Headers["User-Agent"].ToString();

            var response = await _authService.VerifyOtpAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OTP verification");
            return StatusCode(500, new VerifyOtpResponseDto
            {
                Success = false,
                Message = "An error occurred during OTP verification"
            });
        }
    }

    [Authorize]
    [HttpPost("pin")]
    public async Task<ActionResult<AuthActionResponseDto>> SetPin([FromBody] SetPinRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _authService.SetTransactionPinAsync(userId, request);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting transaction PIN");
            return StatusCode(500, new AuthActionResponseDto
            {
                Success = false,
                Message = "An error occurred while setting the PIN"
            });
        }
    }

    [Authorize]
    [HttpPost("pin/verify")]
    public async Task<ActionResult<AuthActionResponseDto>> VerifyPin([FromBody] VerifyPinRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _authService.VerifyTransactionPinAsync(userId, request);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying transaction PIN");
            return StatusCode(500, new AuthActionResponseDto
            {
                Success = false,
                Message = "An error occurred while verifying the PIN"
            });
        }
    }

    [Authorize]
    [HttpPost("biometrics/trust")]
    public async Task<ActionResult<TrustedDeviceDto>> TrustDevice([FromBody] BiometricTrustRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var device = await _authService.TrustDeviceForBiometricsAsync(userId, request);
            if (device == null)
                return BadRequest(new { message = "Unable to trust device" });
            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trusting device for biometrics");
            return StatusCode(500, new { message = "An error occurred while trusting the device" });
        }
    }

    [Authorize]
    [HttpPost("biometrics/disable")]
    public async Task<ActionResult<AuthActionResponseDto>> DisableBiometrics([FromBody] BiometricDisableRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _authService.DisableBiometricsForDeviceAsync(userId, request.DeviceId);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling biometrics for device");
            return StatusCode(500, new AuthActionResponseDto
            {
                Success = false,
                Message = "An error occurred while disabling biometrics"
            });
        }
    }

    [Authorize]
    [HttpGet("devices/trusted")]
    public async Task<ActionResult<List<TrustedDeviceDto>>> GetTrustedDevices()
    {
        try
        {
            var userId = GetUserId();
            var devices = await _authService.GetTrustedDevicesAsync(userId);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trusted devices");
            return StatusCode(500, new { message = "An error occurred while retrieving devices" });
        }
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
}
