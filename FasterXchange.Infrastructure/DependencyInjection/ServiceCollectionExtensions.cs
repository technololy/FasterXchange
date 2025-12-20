using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Infrastructure.Data;
using FasterXchange.Infrastructure.ExternalServices;
using FasterXchange.Infrastructure.KycProviders;
using FasterXchange.Infrastructure.Repositories;
using FasterXchange.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FasterXchange.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("FasterXchange.Infrastructure")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IKycRepository, KycRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();

        // KYC Provider Factory
        services.AddSingleton<KycProviderFactory>();
        
        // KYC Provider - resolved via factory based on configuration
        services.AddScoped<IKycProvider>(sp =>
        {
            var factory = sp.GetRequiredService<KycProviderFactory>();
            return factory.CreateProvider();
        });

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IKycService, KycService>();

        // External Services
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<IEmailService, SendGridEmailService>();

        return services;
    }
}

