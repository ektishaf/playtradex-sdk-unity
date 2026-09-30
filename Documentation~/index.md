# PlayTradeX SDK Documentation

[← Repository README](../README.md)

PlayTradeX is a native blockchain SDK with a Unity C# interface for
integrating EVM-compatible blockchain functionality into games and
interactive applications.

This documentation targets **PlayTradeX SDK `0.2.0-alpha`** and **Unity
`6000.3`**.

> **Current Alpha platforms:** Windows x64 and Android arm64-v8a\
> **Android minimum:** API Level 26\
> **Release status:** Alpha / Pre-release\
> **Configuration:** `Edit > Project Settings > PlayTradeX`

------------------------------------------------------------------------

## Documentation

  -----------------------------------------------------------------------
  Guide                               Purpose
  ----------------------------------- -----------------------------------
  [Getting                            Install PlayTradeX, configure
  Started](getting-started.md)        storage, networks and wallets,
                                      generate identifiers, initialize
                                      the SDK, and wait for readiness.

  [Transactions](transactions.md)     Work with configured networks and
                                      wallets, exact blockchain units,
                                      native balances, transfers, and
                                      transaction approval.

  [Smart Contracts](contracts.md)     Perform network-aware contract
                                      reads and transaction-producing
                                      contract writes.

  [Wallet Management](wallet.md)      Understand the PlayTradeX identity
                                      wallet, configured external
                                      wallets, secure storage, encrypted
                                      export/import, Android document
                                      access, and wallet security.
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## Recommended Reading Order

New integrations should begin with [Getting
Started](getting-started.md).

After the SDK is initialized successfully, continue with:

1.  [Transactions](transactions.md)
2.  [Smart Contracts](contracts.md)
3.  [Wallet Management](wallet.md)

The Unity package also includes a sample demonstrating:

-   SDK initialization
-   Network selection
-   Wallet selection
-   Native balance retrieval
-   Native currency transfers
-   Contract reads and writes
-   Transaction consent
-   Wallet export/import
-   Response and error handling

------------------------------------------------------------------------

## What's New in `0.2.0-alpha`

`0.2.0-alpha` moves PlayTradeX from the original single-network
configuration model to centralized project configuration.

Major additions include:

-   Multiple configured EVM networks
-   Multiple RPC URLs per network
-   Human-readable network names
-   Multiple application-managed wallets
-   PlayTradeX identity-wallet and external-wallet execution
-   Built-in EVM testnet presets
-   `GeneratedNetworks` constants
-   `GeneratedWallets` constants
-   Centralized `Project Settings > PlayTradeX`
-   PlayTradeX About and Updater tooling
-   Updated Unity sample

The previous model of configuring a single RPC URL and Chain ID directly
on the lifecycle component is no longer the recommended `0.2.0-alpha`
configuration model.

------------------------------------------------------------------------

## Project Configuration

Open:

`Edit > Project Settings > PlayTradeX`

The project settings contain the PlayTradeX storage, network, and wallet
configuration.

The settings asset is stored under:

``` text
Assets/PlayTradeX/Resources/PlayTradeXSettings.asset
```

Generated identifiers are placed under:

``` text
Assets/PlayTradeX/Generated/
├── Networks/
│   └── GeneratedNetworks.cs
└── Wallets/
    └── GeneratedWallets.cs
```

Use the generated constants instead of scattering raw network and wallet
ID strings throughout application code.

------------------------------------------------------------------------

## SDK Readiness

PlayTradeX must initialize successfully before normal blockchain
operations begin.

For initialization-driven application flow:

``` csharp
PlayTradeXUnity.Ready += OnPlayTradeXReady;
```

When you only need the current state:

``` csharp
if (PlayTradeXLifecycle.IsInitialized)
{
    Debug.Log("PlayTradeX SDK is ready.");
}
```

See [Getting Started → SDK
Initialization](getting-started.md#sdk-initialization).

------------------------------------------------------------------------

## Execution Model

`0.2.0-alpha` separates network selection from transaction signing.

``` text
Operation
   │
   ├── Network ID
   │      └── Network configuration
   │           ├── Chain ID
   │           └── RPC URL(s)
   │
   └── Wallet source (when signing is required)
          ├── PlayTradeX Identity Wallet
          └── Configured External Wallet
```

Read-only operations require a network.

Transaction-producing operations additionally require a signing wallet
source and may enter the transaction-consent flow.

------------------------------------------------------------------------

## Platform Support

  Platform   Architecture   `0.2.0-alpha`
  ---------- -------------- ---------------
  Windows    x64            Supported
  Android    arm64-v8a      Supported
  macOS      ---            Planned
  iOS        ---            Planned
  Linux      ---            Planned

Android integration includes Keystore-backed secure storage,
document/content URI access, CA certificate setup, native library
loading, and Storage Access Framework wallet workflows.

------------------------------------------------------------------------

## Important Alpha Notes

-   Use PlayTradeX operations only after successful SDK initialization.
-   Configure networks centrally under `Project Settings > PlayTradeX`.
-   Use HTTPS RPC endpoints.
-   Public RPC infrastructure can change independently of PlayTradeX.
-   Regenerate network/wallet classes after changing their IDs.
-   Wallet import replaces sensitive persistent identity-wallet state
    and requires an application restart before normal blockchain
    operations continue.
-   Never log private keys, wallet passwords, or exported wallet
    contents.
-   Test wallet, transaction, network, and contract behavior thoroughly
    before using the Alpha SDK with assets of significant value.

------------------------------------------------------------------------

## Roadmap to `1.0.0`

The planned December `1.0.0` release is intended to expand the current
foundation with:

-   Unreal Engine support with an included sample
-   Continued Unity support and sample improvements
-   macOS support
-   iOS support
-   Expanded network/RPC reliability
-   Continued wallet lifecycle improvements
-   Expanded SDK tooling and documentation
-   Production-readiness work

> Roadmap targets may evolve before the stable release.

------------------------------------------------------------------------

## Version

``` text
PlayTradeX SDK: 0.2.0-alpha
Unity:           6000.3
Windows:         x64
Android:         arm64-v8a
Android API:     26+
```

------------------------------------------------------------------------

[← Repository README](../README.md)
