using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace FasterXchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletService _walletService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        ITransactionRepository transactionRepository,
        IWalletService walletService,
        ITransactionService transactionService,
        ILogger<WebhooksController> logger)
    {
        _transactionRepository = transactionRepository;
        _walletService = walletService;
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandleStripePaymentSuccess(paymentIntent!);
                    break;

                case Events.PaymentIntentPaymentFailed:
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    await HandleStripePaymentFailure(failedPayment!);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }

    [HttpPost("flutterwave")]
    public Task<IActionResult> FlutterwaveWebhook([FromBody] object payload)
    {
        // Flutterwave webhook handling
        _logger.LogInformation("Received Flutterwave webhook");
        
        // TODO: Implement Flutterwave webhook verification and processing
        // This would verify the webhook signature and process bank transfer confirmations
        
        return Task.FromResult<IActionResult>(Ok());
    }

    private async Task HandleStripePaymentSuccess(PaymentIntent paymentIntent)
    {
        var transaction = await _transactionRepository.GetByExternalReferenceIdAsync(paymentIntent.Id);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for Stripe payment intent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        // Update transaction status
        await _transactionService.UpdateTransactionStatusAsync(
            transaction.Id,
            TransactionStatus.Completed);

        // Update wallet balance if not already updated
        if (transaction.WalletId.HasValue && transaction.Status != TransactionStatus.Completed)
        {
            await _walletService.UpdateWalletBalanceAsync(
                transaction.WalletId.Value,
                transaction.Amount,
                isDebit: false);
        }
    }

    private async Task HandleStripePaymentFailure(PaymentIntent paymentIntent)
    {
        var transaction = await _transactionRepository.GetByExternalReferenceIdAsync(paymentIntent.Id);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for Stripe payment intent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        await _transactionService.UpdateTransactionStatusAsync(
            transaction.Id,
            TransactionStatus.Failed,
            paymentIntent.LastPaymentError?.Message ?? "Payment failed");
    }
}

