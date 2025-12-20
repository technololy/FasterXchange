using FasterXchange.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace FasterXchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KycWebhooksController : ControllerBase
{
    private readonly IKycService _kycService;
    private readonly ILogger<KycWebhooksController> _logger;

    public KycWebhooksController(IKycService kycService, ILogger<KycWebhooksController> logger)
    {
        _kycService = kycService;
        _logger = logger;
    }

    [HttpPost("{providerName}")]
    public async Task<IActionResult> ProcessWebhook(
        string providerName,
        [FromBody] object payload)
    {
        try
        {
            // Get signature from header (provider-specific header name)
            var signature = Request.Headers["X-Signature"].FirstOrDefault() ?? 
                          Request.Headers["X-Webhook-Signature"].FirstOrDefault() ??
                          string.Empty;

            _logger.LogInformation("Received KYC webhook from provider {ProviderName}", providerName);

            var success = await _kycService.ProcessKycWebhookAsync(providerName, payload, signature);
            
            if (success)
            {
                return Ok(new { message = "Webhook processed successfully" });
            }

            return BadRequest(new { message = "Failed to process webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing KYC webhook from {ProviderName}", providerName);
            return StatusCode(500, new { message = "An error occurred while processing webhook" });
        }
    }

    [HttpPost("client-sdk")]
    public async Task<IActionResult> ProcessClientSdkWebhook([FromBody] object payload)
    {
        try
        {
            // Client SDK webhook - typically sent by the client application after SDK completion
            var signature = Request.Headers["X-Client-Signature"].FirstOrDefault() ?? string.Empty;

            _logger.LogInformation("Received Client SDK KYC webhook");

            var success = await _kycService.ProcessKycWebhookAsync("ClientSDK", payload, signature);
            
            if (success)
            {
                return Ok(new { message = "Webhook processed successfully" });
            }

            return BadRequest(new { message = "Failed to process webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Client SDK KYC webhook");
            return StatusCode(500, new { message = "An error occurred while processing webhook" });
        }
    }
}

