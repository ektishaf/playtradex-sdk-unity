# Wallet Management

[← Smart Contracts](contracts.md) · [Documentation Home](index.md)

PlayTradeX `0.4.0-alpha` supports two wallet concepts:

1.  The native **PlayTradeX identity wallet**
2.  Developer-configured **external wallets**

The identity wallet remains managed through the native SDK and
platform-specific secure storage. External wallets can be configured in
Unity Project Settings and selected through wallet IDs for transaction
execution.

> **Documentation target:** PlayTradeX `0.4.0-alpha` · Unity `6000.3`

------------------------------------------------------------------------

## Wallet Model

``` text
PlayTradeX Wallet Sources
        │
        ├── Identity Wallet
        │      ├── Native SDK lifecycle
        │      ├── Secure persistent storage
        │      └── Export / Import
        │
        └── External Wallets
               ├── Configured in Project Settings
               ├── Developer-defined wallet ID
               ├── Address
               └── Private key
```

Normal blockchain operations should begin only after PlayTradeX
initialization succeeds.

Wallet configuration and lifecycle behavior remain compatible with the previous Alpha release. `0.4.0-alpha` primarily expands the native RPC layer with health-aware load balancing, runtime endpoint health tracking, and automatic failover for blockchain operations performed with these wallets.

------------------------------------------------------------------------

## PlayTradeX Identity Wallet

During SDK initialization, PlayTradeX prepares the identity wallet
required by the application and uses platform-specific storage for
sensitive persistent wallet data.

The identity wallet is suitable for operations that use the SDK-managed
wallet source.

Its lifecycle includes:

-   Creation/loading
-   Secure persistent storage
-   Transaction signing
-   Encrypted export
-   Encrypted import

------------------------------------------------------------------------

## Configured External Wallets

`0.4.0-alpha` retains application-managed wallet configuration under:

`Edit > Project Settings > PlayTradeX`

A configured external wallet can contain:

-   Wallet ID
-   Address
-   Private key

The wallet ID is the developer-facing identifier.

Application code should select the wallet by ID rather than passing a
private key through gameplay systems.

You can:

-   Add an existing wallet manually
-   Generate a wallet from PlayTradeX Project Settings
-   Add the generated wallet to the project configuration

> External wallets in Project Settings are developer/application-managed
> credentials. Developers are responsible for determining whether this
> model is appropriate for the security architecture of a shipping
> application.

------------------------------------------------------------------------

## Generated Wallet IDs

After configuring wallets, generate the wallet constants.

PlayTradeX creates:

``` text
Assets/PlayTradeX/Generated/Wallets/GeneratedWallets.cs
```

Generated identifiers can be used conceptually like:

``` csharp
GeneratedWallets.PlayerWallet
GeneratedWallets.TreasuryWallet
```

instead of raw strings.

Regenerate the class after changing wallet IDs.

------------------------------------------------------------------------

## Identity vs External Wallet Execution

Transaction-producing operations can conceptually select either:

``` text
Identity
```

or:

``` text
External + Wallet ID
```

The Unity layer resolves the configured wallet and maps the request into
the native execution context.

Read-only contract operations generally do not require a signing wallet.

Native transfers and contract writes do.

Use the included `0.4.0-alpha` sample as the compile-ready reference for
exact wallet-aware API overloads.

------------------------------------------------------------------------

## Secure Storage

PlayTradeX uses platform-specific secure storage where appropriate for
the native identity-wallet lifecycle.

On Android, the Alpha integration uses Android Keystore-backed secure
storage.

Application code should never copy private keys into:

-   `PlayerPrefs`
-   Debug output
-   Analytics events
-   Logs
-   Unprotected application storage
-   Network requests that do not explicitly require the secret

------------------------------------------------------------------------

## Wallet Export

Wallet export creates an encrypted backup of the native PlayTradeX
identity wallet.

Applications should ask the user for an export password and allow the
user to select an appropriate destination.

On Android, PlayTradeX integrates with the Android Storage Access
Framework.

The user chooses the destination through the system document picker, and
the SDK supports Android `content://` document URIs.

### Export Security

-   Never log the export password.
-   Never log exported wallet contents.
-   Do not upload wallet backups without explicit user intent.
-   Treat the exported `.ptx` file as sensitive even though it is
    encrypted.
-   Make the destination and backup operation clear to the user.

------------------------------------------------------------------------

## Wallet Import

Wallet import restores an encrypted PlayTradeX identity-wallet backup.

On Android, the user can select the backup through the system document
picker and PlayTradeX can read it through its content URI.

Wallet import is intentionally synchronous because it modifies sensitive
persistent wallet state.

### Required Flow

For `0.4.0-alpha`:

1.  Initialize PlayTradeX.
2.  Import the wallet.
3.  Verify that the import succeeded.
4.  Ask the user to restart the application.
5.  After restart, allow PlayTradeX to initialize normally using the
    imported identity wallet.

``` csharp
WalletImportResponse response =
    PlayTradeXSdk.ImportWallet(
        password,
        inputPath);

if (response.Success)
{
    Debug.Log(
        "Wallet imported. Restart the application.");
}
else
{
    Debug.LogError(
        response.ErrorMessage);
}
```

> **Important:** Do not continue normal blockchain operations using the
> previous identity-wallet session after a successful import.

------------------------------------------------------------------------

## Android Document Access

The Android integration uses the platform document picker for wallet
backup files.

This provides user-controlled document access without requiring broad
storage permissions for the wallet document-picker workflow.

The Android integration includes:

-   Keystore-backed secure storage
-   Content URI access
-   System document picker integration
-   Wallet export
-   Wallet import
-   Native SDK platform initialization

------------------------------------------------------------------------

## Wallet Security Checklist

Applications integrating PlayTradeX should never:

-   Log private keys.
-   Commit real private keys to source control.
-   Store private keys in `PlayerPrefs`.
-   Log wallet passwords.
-   Display private keys unnecessarily.
-   Pass private keys throughout gameplay code when a configured wallet
    ID can be used.
-   Upload wallet backups without explicit user intent.
-   Continue using the previous identity-wallet session after successful
    import.
-   Disable TLS certificate verification for blockchain networking.

Applications should clearly communicate wallet backup and restore
actions and test the complete lifecycle on every supported target
platform.

------------------------------------------------------------------------

## External Wallet Configuration Warning

Unlike the native identity wallet, external wallets entered in Unity
Project Settings are application/developer-managed configuration.

Before shipping a product with such a wallet, consider:

-   Who owns the wallet
-   Whether the credential should exist in the client build
-   Whether the wallet is intended only for development/testing
-   Whether a server-side or user-owned signing model is more
    appropriate
-   What an attacker could do if the client-side credential were
    extracted

Do not use a high-value production private key in a client application
merely because the SDK supports external-wallet execution.

------------------------------------------------------------------------

## After Identity Wallet Import

A successful import changes persistent identity-wallet state.

Restart the application before resuming normal blockchain operations.

After restart, wait for PlayTradeX to initialize successfully before:

-   Querying balances
-   Sending transactions
-   Performing contract writes

Continue with:

-   [Transactions](transactions.md)
-   [Smart Contracts](contracts.md)

------------------------------------------------------------------------

[← Smart Contracts](contracts.md) · [Documentation Home](index.md)
