# FasterXchange Backend API

Cross-Border Remittance App + P2P FX Marketplace + Wallet System + Escrow - Backend Monolith

## Project Structure

```
FasterXchange/
├── FasterXchange.Api/              # Web API project (Controllers, Program.cs)
├── FasterXchange.Application/     # Application layer (DTOs, Contracts, Interfaces)
├── FasterXchange.Domain/           # Domain layer (Entities, Enums)
├── FasterXchange.Infrastructure/  # Infrastructure layer (EF Core, Repositories, Services)
└── FasterXchange.sln              # Solution file
```

## Architecture

This project follows Clean Architecture principles:

- **Domain**: Core business entities and enums
- **Application**: Contracts, DTOs, and service interfaces
- **Infrastructure**: Data access, external services, and implementations
- **API**: Controllers and API configuration

## Prerequisites

- .NET 8 SDK
- PostgreSQL 12+ (or Docker)
- Visual Studio / Rider / VS Code

## Setup

### 1. Database Setup

Create a PostgreSQL database:

```bash
# Using Docker
docker run --name fasterxchange-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=FasterXchangeDb -p 5432:5432 -d postgres

# Or using psql
createdb FasterXchangeDb
```

### 2. Configuration

Update `appsettings.json` or `appsettings.Development.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FasterXchangeDb;Username=postgres;Password=postgres"
  }
}
```

### 3. External Services (Optional for Development)

#### OTP Services
For OTP functionality, configure Twilio (SMS) and SendGrid (Email) in `appsettings.json`:

```json
{
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromPhoneNumber": "+1234567890"
  },
  "SendGrid": {
    "ApiKey": "your-api-key",
    "FromEmail": "noreply@fasterxchange.com",
    "FromName": "FasterXchange"
  }
}
```

**Note**: In development, if these are not configured, OTP codes will be logged to the console instead of being sent.

#### Payment Providers
For wallet funding, configure Stripe and Flutterwave in `appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublicKey": "pk_test_...",
    "ReturnUrl": "https://fasterxchange.com/payment/return"
  },
  "Flutterwave": {
    "SecretKey": "FLWSECK_TEST-...",
    "PublicKey": "FLWPUBK_TEST-...",
    "EncryptionKey": "..."
  }
}
```

**Note**: In development, if these are not configured, payments will be simulated.

#### KYC Providers
Configure your KYC provider in `appsettings.json`:

```json
{
  "Kyc": {
    "Provider": "Sumsub",
    "Sumsub": {
      "AppToken": "your-app-token",
      "SecretKey": "your-secret-key",
      "BaseUrl": "https://api.sumsub.com"
    },
    "VerifyMe": {
      "ApiKey": "your-api-key",
      "BaseUrl": "https://api.verifyme.ng"
    }
  }
}
```

**Provider Options:**
- `Sumsub` - Full-featured KYC with client SDK support
- `VerifyMe` - Server-side KYC for Nigeria
- `ClientSDK` - For client-side SDK integration

**Note**: In development, if KYC credentials are not configured, KYC will be simulated.

### 4. JWT Configuration

Update JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProduction!",
    "Issuer": "FasterXchange",
    "Audience": "FasterXchange",
    "AccessTokenExpiryMinutes": "60",
    "RefreshTokenExpiryDays": "30"
  }
}
```

### 5. Run the Application

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
cd FasterXchange.Api
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Database Migrations

To create and apply migrations:

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create a migration
cd FasterXchange.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../FasterXchange.Api

# Apply migrations
dotnet ef database update --startup-project ../FasterXchange.Api
```

**Note**: The application currently uses `EnsureCreated()` in development mode, which will create the database automatically. For production, use migrations.

## API Endpoints

### Authentication

#### Register
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",  // or "phoneNumber": "+1234567890"
  "fullName": "John Doe"  // optional
}
```

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "identifier": "user@example.com",  // email or phone
  "deviceId": "device-123",  // optional
  "deviceName": "iPhone 12",  // optional
  "deviceType": "iOS"  // optional
}
```

#### Verify OTP
```
POST /api/auth/verify-otp
Content-Type: application/json

{
  "identifier": "user@example.com",
  "code": "123456",
  "type": 0,  // 0=Registration, 1=Login, etc.
  "deviceId": "device-123",  // optional
  "deviceName": "iPhone 12",  // optional
  "deviceType": "iOS"  // optional
}
```

### Wallet Management

#### Get Wallets
```
GET /api/wallet/balance
Authorization: Bearer {token}
```

#### Get Wallet by Currency
```
GET /api/wallet/balance/{currency}
Authorization: Bearer {token}
```

#### Fund Wallet
```
POST /api/wallet/fund
Authorization: Bearer {token}
Content-Type: application/json

{
  "currency": 0,  // 0=CAD, 1=NGN
  "amount": 100.00,
  "method": 0,  // 0=DebitCard, 1=BankTransfer, 2=VirtualAccount
  "cardToken": "tok_visa",  // For card payments
  "interacEmail": "user@example.com",  // For Interac
  "senderName": "John Doe"  // For bank transfers
}
```

#### Get Transaction History
```
GET /api/wallet/transactions?currency=0&type=0&status=2&page=1&pageSize=20
Authorization: Bearer {token}
```

#### Get Transaction
```
GET /api/wallet/transactions/{transactionId}
Authorization: Bearer {token}
```

### KYC Management

#### Start KYC Verification
```
POST /api/kyc/start
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "John Doe",
  "dateOfBirth": "1990-01-01",
  "address": "123 Main St, Toronto, ON",
  "countryCode": "CA",
  "idType": "Passport",
  "useClientSdk": false
}
```

#### Get KYC Status
```
GET /api/kyc/status
Authorization: Bearer {token}
```

#### Resubmit KYC
```
POST /api/kyc/resubmit
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "John Doe",
  "dateOfBirth": "1990-01-01",
  "address": "123 Main St, Toronto, ON",
  "countryCode": "CA",
  "idType": "Passport"
}
```

### Webhooks

#### Stripe Webhook
```
POST /api/webhooks/stripe
```

#### Flutterwave Webhook
```
POST /api/webhooks/flutterwave
```

#### KYC Webhook (Provider-specific)
```
POST /api/kyc-webhooks/{providerName}
Headers: X-Signature or X-Webhook-Signature
```

#### Client SDK KYC Webhook
```
POST /api/kyc-webhooks/client-sdk
Headers: X-Client-Signature
```

## Features Implemented

### Sprint 1 - Auth & Core Infra ✅
✅ User Registration (Email/Phone)
✅ OTP-based Authentication (Passwordless)
✅ JWT Token Generation
✅ Device Binding & Tracking
✅ User Entity with Wallets
✅ User Settings & Notification Preferences
✅ Repository Pattern
✅ Dependency Injection
✅ Swagger/OpenAPI Documentation

### Sprint 2 - Wallet & Ledger ✅
✅ Wallet Management (View balances, CAD & NGN)
✅ Wallet Funding Methods:
   - Debit Card (Stripe integration)
   - Bank Transfer (Interac for CAD)
   - Virtual Account (NGN via Flutterwave)
✅ Transaction History & Ledger
✅ Balance Tracking (Total, On-Hold, Available)
✅ Payment Provider Integration (Stripe, Flutterwave)
✅ Webhook Handlers for Payment Callbacks
✅ Transaction Status Management

### Sprint 3 - KYC Integration ✅
✅ Abstract KYC Provider Interface (Vendor-agnostic)
✅ Multiple Provider Support:
   - Sumsub (with Client SDK support)
   - VerifyMe
   - Client SDK mode (for client-side integration)
✅ KYC Provider Factory (Easy vendor switching)
✅ KYC Verification Flow:
   - Initiate verification
   - Status tracking
   - Resubmission support
✅ Webhook Processing (Provider callbacks)
✅ Client SDK Configuration Support
✅ KYC Status Management & Validation
✅ User KYC Status Synchronization

## Next Steps (Future Sprints)

- Sprint 4: Remittance Engine
- Sprint 5: P2P Marketplace & Escrow
- Sprint 6: Admin Portal

## Development Notes

- The application uses passwordless authentication with OTP
- OTP codes expire after 10 minutes
- Failed OTP attempts are limited to 3 before locking
- Account lockout after 3 failed login attempts (15 minutes)
- JWT tokens are used for authenticated requests
- Device tracking is implemented for security

## Security Considerations

- Change the JWT secret key in production
- Use environment variables for sensitive configuration
- Enable HTTPS in production
- Implement rate limiting
- Add request validation middleware
- Use secure password hashing (BCrypt) for optional passwords
- Implement refresh token rotation

## License

Copyright © 2025 FasterXchange Technology Inc.

