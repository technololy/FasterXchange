using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FasterXchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(
        IWalletService walletService,
        ITransactionService transactionService,
        ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _transactionService = transactionService;
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

    [HttpGet("balance")]
    public async Task<ActionResult<List<WalletBalanceDto>>> GetWallets()
    {
        try
        {
            var userId = GetUserId();
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            return Ok(wallets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallets");
            return StatusCode(500, new { message = "An error occurred while retrieving wallets" });
        }
    }

    [HttpGet("balance/{currency}")]
    public async Task<ActionResult<WalletBalanceDto>> GetWalletByCurrency(string currency)
    {
        try
        {
            if (!Enum.TryParse<Currency>(currency, true, out var currencyEnum))
            {
                return BadRequest(new { message = "Invalid currency" });
            }

            var userId = GetUserId();
            var wallet = await _walletService.GetWalletByCurrencyAsync(userId, currencyEnum);
            
            if (wallet == null)
            {
                return NotFound(new { message = "Wallet not found" });
            }

            return Ok(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet");
            return StatusCode(500, new { message = "An error occurred while retrieving wallet" });
        }
    }

    [HttpPost("fund")]
    public async Task<ActionResult<FundWalletResponseDto>> FundWallet([FromBody] FundWalletRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _walletService.FundWalletAsync(userId, request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error funding wallet");
            return StatusCode(500, new FundWalletResponseDto
            {
                Success = false,
                Message = "An error occurred while processing the funding request"
            });
        }
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<TransactionHistoryResponseDto>> GetTransactionHistory(
        [FromQuery] TransactionHistoryRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _transactionService.GetTransactionHistoryAsync(userId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history");
            return StatusCode(500, new { message = "An error occurred while retrieving transaction history" });
        }
    }

    [HttpGet("transactions/{transactionId}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId, userId);
            
            if (transaction == null)
            {
                return NotFound(new { message = "Transaction not found" });
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction");
            return StatusCode(500, new { message = "An error occurred while retrieving transaction" });
        }
    }
}

