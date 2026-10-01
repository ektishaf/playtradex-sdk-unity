# Getting Started

[← Documentation Home](index.md) · [Next: Transactions
→](transactions.md)

This guide walks through the minimum setup required to configure and
initialize PlayTradeX `0.3.0-alpha` in a Unity project.

> **Documentation target:** PlayTradeX `0.3.0-alpha` · Unity `6000.3`

------------------------------------------------------------------------

## Requirements

-   Unity `6000.3`
-   Windows x64 or Android arm64-v8a
-   Android API Level 26 or newer
-   Internet access for blockchain operations
-   At least one configured EVM network
-   At least one working HTTPS RPC endpoint for every network you intend
    to use

------------------------------------------------------------------------

## Installation

In Unity open:

`Window > Package Manager`

Choose:

`+ > Install package from git URL...`

Enter:

``` text
https://github.com/ektishaf/playtradex-sdk-unity.git#v0.3.0-alpha
```

Using the tagged release is recommended so the project is pinned to this
exact SDK revision.

You can also add PlayTradeX to `Packages/manifest.json`:

``` json
{
  "dependencies": {
    "com.playtradex.sdk": "https://github.com/ektishaf/playtradex-sdk-unity.git#v0.3.0-alpha"
  }
}
```

------------------------------------------------------------------------

## Open PlayTradeX Project Settings

Open:

`Edit > Project Settings > PlayTradeX`

This is the central configuration surface for `0.3.0-alpha`.

PlayTradeX stores project configuration under:

``` text
Assets/PlayTradeX/Resources/PlayTradeXSettings.asset
```

Generated network and wallet identifiers are created under:

``` text
Assets/PlayTradeX/Generated/
├── Networks/
│   └── GeneratedNetworks.cs
└── Wallets/
    └── GeneratedWallets.cs
```

Do not manually edit generated classes.

------------------------------------------------------------------------

## Configure Storage

Configure **Storage Path** when a custom location is required.

When left empty, PlayTradeX uses:

``` csharp
Application.persistentDataPath
```

On Android, leaving the field empty is recommended.

------------------------------------------------------------------------

## Configure Networks

Add the EVM networks the application needs.

A network configuration can contain:

-   `ID`
-   `Network Name`
-   `RPC URLs`
-   `Chain ID`
-   `Symbol`
-   `Block Explorer URL`
-   `Is Testnet`
-   `Preset ID`

Example:

``` text
ID:                 bsc-testnet
Network Name:       Binance Smart Chain Testnet
Chain ID:           97
Symbol:             tBNB
Block Explorer URL: https://testnet.bscscan.com
```

The **ID** is the stable developer-facing identifier used by code.\
The **Network Name** is the human-readable network name.

A network can contain multiple RPC URLs.

In `0.3.0-alpha`, PlayTradeX validates configured endpoints against the network Chain ID and can automatically fail over to another configured endpoint after eligible RPC failures. Wrong-chain endpoints are rejected. Validation results are cached, and temporarily unavailable endpoints can enter a cooldown period before being retried.

> Multiple RPC URLs provide reliability and failover in `0.3.0-alpha`; requests are not load-balanced across healthy endpoints.

------------------------------------------------------------------------

## Add Testnet Presets

The Networks section provides:

-   **Add Testnet**
-   **Add All Testnets**
-   **Remove Testnets**
-   **Generate Class**

`Add Testnet` lets you select a built-in PlayTradeX testnet preset.

The initial preset catalog includes:

-   Ethereum Sepolia
-   Binance Smart Chain Testnet
-   opBNB Testnet
-   Polygon Amoy
-   Arbitrum Sepolia
-   Avalanche Fuji C-Chain
-   Linea Sepolia
-   ZKsync Era Sepolia
-   Gnosis Chiado

Preset configuration includes network ID, network name, ecosystem, chain
ID, native symbol, RPC URL(s), explorer URL, and testnet classification.

Public RPC endpoints are external infrastructure and can change
independently of PlayTradeX.

------------------------------------------------------------------------

## Generate Network Constants

After configuring networks, click:

**Generate Class**

PlayTradeX creates:

``` text
Assets/PlayTradeX/Generated/Networks/GeneratedNetworks.cs
```

Application code can then use generated network identifiers conceptually
like:

``` csharp
GeneratedNetworks.EthereumSepolia
GeneratedNetworks.BscTestnet
```

Regenerate the class whenever network IDs change.

------------------------------------------------------------------------

## Configure Wallets

The Wallets section supports application-managed external wallets in
addition to the native PlayTradeX identity wallet.

A configured external wallet can contain:

-   Wallet ID
-   Address
-   Private key

You can add an existing wallet manually or use the PlayTradeX wallet
generation workflow.

Wallet IDs let application code select configured wallets without
passing private keys through gameplay code.

> Never log, publish, or commit real private keys.

------------------------------------------------------------------------

## Generate Wallet Constants

Generate the wallet class after configuring wallet IDs.

PlayTradeX creates:

``` text
Assets/PlayTradeX/Generated/Wallets/GeneratedWallets.cs
```

Generated identifiers can be used conceptually like:

``` csharp
GeneratedWallets.PlayerWallet
GeneratedWallets.TreasuryWallet
```

Regenerate the class whenever wallet IDs change.

------------------------------------------------------------------------

## Import the Unity Sample

From Unity Package Manager, select PlayTradeX and import the included
sample.

The sample demonstrates:

-   SDK initialization
-   Network selection
-   Wallet selection
-   Native balance retrieval
-   Native currency transfers
-   Contract reads
-   Contract writes
-   Transaction consent
-   Wallet export/import
-   Response handling

For a first integration, the sample is the recommended reference for the
exact `0.3.0-alpha` network/wallet API usage.

### Sample UI Requirements

Import TMP Essential Resources:

`Window > TextMeshPro > Import TMP Essential Resources`

Also verify that the sample `EventSystem` has an input module compatible
with the project's selected Unity input system.

The sample's Editor helper configures its Game View for a `1920 × 1080`
Full HD preview when the sample scene is opened where supported by the
targeted Unity Editor version.

------------------------------------------------------------------------

## SDK Initialization

Add `PlayTradeXLifecycle` to a GameObject in the initial scene.

In `0.3.0-alpha`, network RPC URLs and Chain IDs belong to the
PlayTradeX network configuration rather than a single lifecycle-level
network configuration.

The lifecycle initializes the required platform bridge and native SDK
from the project configuration.

Normal blockchain operations should begin only after initialization
succeeds.

------------------------------------------------------------------------

## Recommended: React to the Ready Event

``` csharp
PlayTradeXUnity.Ready += OnPlayTradeXReady;

private void OnPlayTradeXReady()
{
    Debug.Log("PlayTradeX SDK is ready.");

    // Start systems that depend on PlayTradeX here.
}
```

Manage event subscriptions according to the lifetime of the subscribing
object.

------------------------------------------------------------------------

## Check the Current Initialization State

``` csharp
if (PlayTradeXLifecycle.IsInitialized)
{
    Debug.Log("PlayTradeX SDK is ready.");
}
```

Use `PlayTradeXUnity.Ready` to react to initialization and
`PlayTradeXLifecycle.IsInitialized` to inspect current state.

------------------------------------------------------------------------

## Network and Wallet Selection

The `0.3.0-alpha` execution model is:

``` text
Operation
   ├── Network ID
   │      └── configured Chain ID + RPC URL(s)
   │
   └── Wallet source when signing is required
          ├── PlayTradeX Identity Wallet
          └── Configured External Wallet
```

Use generated identifiers instead of scattering raw strings through
application code.

The included sample should be used as the compile-ready reference for
the exact overloads exposed by the installed package.

------------------------------------------------------------------------

## What to Do After Initialization

Continue with:

-   [Transactions](transactions.md) --- units, balances, transfers,
    networks, wallets, and transaction approval
-   [Smart Contracts](contracts.md) --- network-aware reads and writes
-   [Wallet Management](wallet.md) --- identity wallet, external
    wallets, secure storage, export, and import

------------------------------------------------------------------------

## Android Notes

PlayTradeX initializes its Android platform bridge automatically through
Unity.

Unity code does not need to manually provide an Android `Context`.

The Android integration provides:

-   Android Keystore-backed secure storage
-   Android document/content URI access
-   Wallet backup and restore through the system document picker
-   CA certificate setup
-   Native PlayTradeX library loading

Current Android target:

``` text
arm64-v8a
API Level 26+
```

Broad storage permissions are not required for the Storage Access
Framework wallet workflow.

------------------------------------------------------------------------

## Troubleshooting Checklist

If PlayTradeX is not ready or an operation fails, verify:

1.  `PlayTradeXLifecycle` exists in the startup scene.
2.  `PlayTradeXSettings.asset` exists and contains the expected
    configuration.
3.  The selected network ID exists.
4.  The network has the correct Chain ID.
5.  At least one configured RPC endpoint is valid, reachable, and reports the expected Chain ID.
6.  The selected external wallet ID exists when an external wallet is
    requested.
7.  Generated network/wallet classes were regenerated after ID changes.
8.  Dependent code waits for `PlayTradeXUnity.Ready` or checks
    `PlayTradeXLifecycle.IsInitialized`.
9.  The current build target is supported by `0.3.0-alpha`.
10. Android is using API Level 26+ and arm64-v8a.

------------------------------------------------------------------------

[← Documentation Home](index.md) · [Next: Transactions
→](transactions.md)
