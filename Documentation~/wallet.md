# Wallet Management

[← Smart Contracts](contracts.md) · [Documentation Home](index.md)

PlayTradeX manages the application wallet through the native SDK and
platform-specific secure storage.

Wallet functionality is intentionally kept behind the SDK so Unity
gameplay code does not need to manage raw private-key persistence
directly.

> **Documentation target:** PlayTradeX `0.1.0-alpha` · Unity `6000.3`

------------------------------------------------------------------------

## Wallet Lifecycle

During SDK initialization, PlayTradeX prepares the wallet required by
the application and uses platform-specific storage for sensitive
persistent wallet data.

Normal blockchain operations should begin only after PlayTradeX
initialization succeeds.

See [Getting Started](getting-started.md) for lifecycle configuration
and the `PlayTradeXUnity.Ready` event.

------------------------------------------------------------------------

## Secure Storage

PlayTradeX uses platform-specific secure storage where appropriate.

On Android, the Alpha integration uses Android Keystore-backed secure
storage.

Application code should never copy private keys into `PlayerPrefs`,
ordinary configuration files, debug output, analytics events, or other
application-managed plaintext storage.

------------------------------------------------------------------------

## Wallet Export

Wallet export creates an encrypted PlayTradeX wallet backup.

Applications should ask the user for an export password and allow the
user to select an appropriate destination.

On Android, PlayTradeX integrates with the Android Storage Access
Framework. The user chooses the destination through the system document
picker, and the SDK supports Android `content://` document URIs for the
wallet backup.

### Export Security

-   Never log the export password.
-   Never log the exported wallet contents.
-   Do not upload wallet backups without explicit user intent.
-   Treat the exported `.ptx` file as sensitive even though the backup
    is encrypted.
-   Use an application UX that makes the destination and backup
    operation clear to the user.

------------------------------------------------------------------------

## Wallet Import

Wallet import restores an encrypted PlayTradeX wallet backup.

On Android, the user can select the backup through the system document
picker and PlayTradeX can read the selected document through its content
URI.

Wallet import is intentionally synchronous because it modifies sensitive
persistent wallet state.

### Required Alpha Flow

For `0.1.0-alpha`:

1.  Initialize PlayTradeX.
2.  Import the wallet.
3.  Verify that the import succeeded.
4.  Ask the user to restart the application.
5.  After restart, allow PlayTradeX to initialize normally using the
    imported wallet.

``` csharp
WalletImportResponse response =
    PlayTradeXSdk.ImportWallet(password, inputPath);

if (response.Success)
{
    Debug.Log("Wallet imported. Restart the application.");
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

> **Important:** Do not continue normal blockchain operations using the
> previous wallet session after a successful import.

------------------------------------------------------------------------

## Android Document Access

The Android Alpha integration uses the platform document picker for
wallet backup files.

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
-   Store private keys in `PlayerPrefs`.
-   Log wallet passwords.
-   Display private keys unnecessarily.
-   Upload wallet backups without explicit user intent.
-   Continue using the previous wallet session after a successful wallet
    import.
-   Disable TLS certificate verification for blockchain networking.

Applications should clearly communicate wallet backup and restore
actions to the user and test the complete lifecycle on each supported
target platform.

------------------------------------------------------------------------

## After Import

A successful import changes persistent wallet state. Restart the
application before resuming normal blockchain operations.

After restart, wait for PlayTradeX to initialize successfully before
querying balances, sending transactions, or interacting with contracts.

Continue with:

-   [Transactions](transactions.md)
-   [Smart Contracts](contracts.md)

------------------------------------------------------------------------

[← Smart Contracts](contracts.md) · [Documentation Home](index.md)
