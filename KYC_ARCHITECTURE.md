# KYC Provider Abstraction Architecture

## Overview

The KYC system is designed with a provider-agnostic architecture that allows easy switching between different KYC vendors (Sumsub, VerifyMe, etc.) and supports both server-side and client-side SDK integrations.

## Architecture Components

### 1. Abstract Interface (`IKycProvider`)

The core abstraction that all KYC providers must implement:

```csharp
public interface IKycProvider
{
    string ProviderName { get; }
    bool SupportsClientSdk { get; }
    
    Task<KycInitiationResult> InitiateVerificationAsync(KycInitiationRequest request);
    Task<KycStatusResult> GetVerificationStatusAsync(string externalKycId);
    Task<KycWebhookResult> ProcessWebhookAsync(object webhookPayload, string signature);
    bool VerifyWebhookSignature(object payload, string signature);
    Task<ClientSdkConfig?> GetClientSdkConfigAsync(Guid userId);
}
```

### 2. Base Provider Class (`BaseKycProvider`)

Provides common functionality and helper methods for all providers:
- Status mapping utilities
- Common logging
- Configuration access

### 3. Provider Implementations

#### SumsubKycProvider
- Full-featured KYC provider
- Supports both server-side and client SDK
- Returns access tokens for client SDK integration
- Webhook support for status updates

#### VerifyMeKycProvider
- Nigeria-focused KYC provider
- Server-side only (no client SDK)
- Specialized for Nigerian ID verification

#### ClientSdkKycProvider
- For client-side SDK integration
- Client handles verification directly
- Backend only tracks status via webhooks
- Useful when client wants full control over KYC flow

### 4. Provider Factory (`KycProviderFactory`)

Dynamically creates provider instances based on configuration:

```json
{
  "Kyc": {
    "Provider": "Sumsub"  // or "VerifyMe", "ClientSDK"
  }
}
```

### 5. KYC Service (`IKycService`)

Business logic layer that uses the provider abstraction:
- Manages KYC documents in database
- Synchronizes user KYC status
- Handles resubmissions
- Processes webhooks

## Usage Patterns

### Server-Side Flow

1. Client calls `POST /api/kyc/start`
2. Service initiates verification with provider
3. Provider returns verification URL
4. User redirected to provider's verification page
5. Provider sends webhook on completion
6. Service updates KYC status

### Client SDK Flow

1. Client calls `POST /api/kyc/start` with `useClientSdk: true`
2. Service initiates verification with provider
3. Provider returns `ClientSdkConfig` with access token
4. Client uses SDK with access token to complete verification
5. Client calls webhook endpoint when done
6. Service updates KYC status

### Adding a New Provider

1. Create new class inheriting from `BaseKycProvider`
2. Implement all abstract methods
3. Add provider to `KycProviderFactory`
4. Add configuration section to `appsettings.json`
5. Update provider selection logic if needed

Example:

```csharp
public class NewKycProvider : BaseKycProvider
{
    public override string ProviderName => "NewProvider";
    public override bool SupportsClientSdk => true;
    
    // Implement required methods...
}
```

## Configuration

### Provider Selection

Set in `appsettings.json`:
```json
{
  "Kyc": {
    "Provider": "Sumsub"
  }
}
```

### Provider-Specific Settings

Each provider can have its own configuration:

```json
{
  "Kyc": {
    "Sumsub": {
      "AppToken": "...",
      "SecretKey": "...",
      "BaseUrl": "https://api.sumsub.com"
    },
    "VerifyMe": {
      "ApiKey": "...",
      "BaseUrl": "https://api.verifyme.ng"
    }
  }
}
```

## Webhook Processing

All providers support webhook callbacks:

```
POST /api/kyc-webhooks/{providerName}
```

The service:
1. Verifies webhook signature
2. Parses provider-specific payload
3. Updates KYC document status
4. Synchronizes user KYC status

## Benefits of This Architecture

1. **Vendor Independence**: Easy to switch providers without changing business logic
2. **Multiple Provider Support**: Can support multiple providers simultaneously
3. **Client SDK Flexibility**: Supports both server-side and client-side flows
4. **Testability**: Easy to mock providers for testing
5. **Extensibility**: Simple to add new providers
6. **Configuration-Driven**: Provider selection via config, no code changes needed

## Future Enhancements

- Multi-provider fallback (try Sumsub, fallback to VerifyMe)
- Provider health monitoring
- A/B testing different providers
- Provider performance metrics
- Automatic provider selection based on user location

