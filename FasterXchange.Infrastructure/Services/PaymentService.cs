using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FasterXchange.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly string? _stripeSecretKey;
    private readonly string? _flutterwaveSecretKey;
    private readonly string? _flutterwavePublicKey;

    public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _stripeSecretKey = _configuration["Stripe:SecretKey"];
        _flutterwaveSecretKey = _configuration["Flutterwave:SecretKey"];
        _flutterwavePublicKey = _configuration["Flutterwave:PublicKey"];

        // Initialize Stripe
        if (!string.IsNullOrEmpty(_stripeSecretKey))
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }
    }

    public async Task<PaymentResult> ProcessCardPaymentAsync(
        Currency currency,
        decimal amount,
        string cardToken,
        string? paymentMethodId = null)
    {
        if (string.IsNullOrEmpty(_stripeSecretKey))
        {
            _logger.LogWarning("Stripe secret key not configured. Payment will be simulated.");
            // Simulate payment for development
            return new PaymentResult
            {
                Success = true,
                ExternalReferenceId = $"STRIPE_SIM_{Guid.NewGuid()}",
                EstimatedDeliveryAt = DateTime.UtcNow.AddMinutes(5)
            };
        }

        try
        {
            var amountInCents = (long)(amount * 100); // Convert to cents
            var currencyCode = currency == Currency.CAD ? "cad" : "ngn";

            PaymentIntentCreateOptions options;
            
            if (!string.IsNullOrEmpty(paymentMethodId))
            {
                // Use existing payment method
                options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currencyCode,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    ReturnUrl = _configuration["Stripe:ReturnUrl"] ?? "https://fasterxchange.com/payment/return"
                };
            }
            else
            {
                // Create payment intent with card token
                options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currencyCode,
                    PaymentMethodTypes = new List<string> { "card" },
                    PaymentMethod = cardToken,
                    Confirm = true,
                    ReturnUrl = _configuration["Stripe:ReturnUrl"] ?? "https://fasterxchange.com/payment/return"
                };
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentResult
                {
                    Success = true,
                    ExternalReferenceId = paymentIntent.Id,
                    EstimatedDeliveryAt = DateTime.UtcNow.AddMinutes(5)
                };
            }
            else if (paymentIntent.Status == "requires_action")
            {
                return new PaymentResult
                {
                    Success = false,
                    PaymentUrl = paymentIntent.NextAction?.RedirectToUrl?.Url,
                    ExternalReferenceId = paymentIntent.Id,
                    FailureReason = "Payment requires additional authentication"
                };
            }
            else
            {
                return new PaymentResult
                {
                    Success = false,
                    ExternalReferenceId = paymentIntent.Id,
                    FailureReason = $"Payment status: {paymentIntent.Status}"
                };
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment error");
            return new PaymentResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    public async Task<PaymentResult> ProcessInteracPaymentAsync(
        decimal amount,
        string interacEmail)
    {
        // Interac e-Transfer integration would go here
        // For now, return a simulated result
        _logger.LogInformation("Processing Interac payment for {Amount} to {Email}", amount, interacEmail);
        
        return new PaymentResult
        {
            Success = true,
            ExternalReferenceId = $"INTERAC_{Guid.NewGuid()}",
            EstimatedDeliveryAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<PaymentResult> CreateVirtualAccountAsync(
        Guid userId,
        string email,
        string phoneNumber,
        string fullName)
    {
        if (string.IsNullOrEmpty(_flutterwaveSecretKey))
        {
            _logger.LogWarning("Flutterwave secret key not configured. Virtual account will be simulated.");
            // Simulate virtual account creation
            var virtualAccountNumber = $"VA{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            return new PaymentResult
            {
                Success = true,
                VirtualAccountNumber = virtualAccountNumber,
                VirtualAccountBank = "Flutterwave Bank"
            };
        }

        try
        {
            // Flutterwave API integration would go here
            // This is a placeholder - actual implementation would use Flutterwave SDK or HTTP client
            _logger.LogInformation("Creating virtual account for user {UserId}", userId);

            // Simulate API call
            var virtualAccountNumber = $"VA{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            
            return new PaymentResult
            {
                Success = true,
                VirtualAccountNumber = virtualAccountNumber,
                VirtualAccountBank = "Flutterwave Bank",
                ExternalReferenceId = $"FLW_VA_{Guid.NewGuid()}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating virtual account");
            return new PaymentResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    public async Task<PaymentResult> VerifyBankTransferAsync(
        string referenceId,
        Currency currency)
    {
        // This would verify a bank transfer via webhook or polling
        // For now, return a simulated result
        _logger.LogInformation("Verifying bank transfer {ReferenceId} for {Currency}", referenceId, currency);
        
        return new PaymentResult
        {
            Success = true,
            ExternalReferenceId = referenceId,
            EstimatedDeliveryAt = DateTime.UtcNow.AddHours(1)
        };
    }
}

