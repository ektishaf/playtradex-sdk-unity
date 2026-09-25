# PlayTradeX SDK Documentation

[← Repository README](../README.md)

PlayTradeX is a native blockchain SDK with a Unity C# interface for
integrating EVM-compatible blockchain functionality into games and
interactive applications.

This documentation targets **PlayTradeX SDK `0.1.0-alpha`** and **Unity
`6000.3`**.

> **Current Alpha platforms:** Windows x64 and Android arm64-v8a\
> **Android minimum:** API Level 26\
> **Release status:** Alpha

------------------------------------------------------------------------

## Documentation

  -----------------------------------------------------------------------
  Guide                               Purpose
  ----------------------------------- -----------------------------------
  [Getting                            Install the package, configure
  Started](getting-started.md)        `PlayTradeXLifecycle`, and wait for
                                      SDK readiness.

  [Transactions](transactions.md)     Work with blockchain units, native
                                      balances, native currency
                                      transfers, and transaction
                                      approval.

  [Smart Contracts](contracts.md)     Perform read-only contract calls
                                      and transaction-producing contract
                                      writes.

  [Wallet Management](wallet.md)      Understand wallet storage,
                                      encrypted export/import, Android
                                      document access, and wallet
                                      security.
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## Recommended Reading Order

New integrations should begin with [Getting
Started](getting-started.md). After the SDK is initialized successfully,
continue with [Transactions](transactions.md), [Smart
Contracts](contracts.md), and [Wallet Management](wallet.md) as required
by your project.

The Unity package also includes a sample that demonstrates
initialization, native balance retrieval, native currency transfers,
contract reads and writes, transaction consent, wallet operations, and
response handling.

------------------------------------------------------------------------

## SDK Readiness

PlayTradeX must be initialized successfully before normal blockchain
operations are started.

For initialization-driven application flow, subscribe to the
`PlayTradeXUnity.Ready` event. When you only need to query the current
state, use `PlayTradeXLifecycle.IsInitialized`.

See [Getting Started → SDK
Initialization](getting-started.md#sdk-initialization) for the
recommended setup.

------------------------------------------------------------------------

## Platform Support

  Platform   Architecture   `0.1.0-alpha`
  ---------- -------------- ---------------------
  Windows    x64            Supported
  Android    arm64-v8a      Supported
  macOS      ---            Planned for `1.0.0`
  iOS        ---            Planned for `1.0.0`
  Linux      ---            Planned

Android integration includes Keystore-backed secure storage,
document/content URI access, CA certificate setup, and native library
loading.

------------------------------------------------------------------------

## Coming in December --- `1.0.0`

The planned December `1.0.0` release expands PlayTradeX beyond the
current Alpha foundation.

Planned work includes:

-   Unreal Engine support with an included sample
-   Continued Unity support with an included sample
-   Multi-network support
-   Calling multiple functions across multiple contracts and configured
    networks
-   macOS support
-   iOS support
-   Continued SDK, tooling, documentation, wallet lifecycle, and
    production-readiness improvements

> The `1.0.0` items above are roadmap targets and may evolve before the
> stable release.

------------------------------------------------------------------------

## Important Alpha Notes

-   Use PlayTradeX operations only after successful SDK initialization.
-   Wallet import replaces sensitive persistent wallet state and
    requires an application restart before normal blockchain operations
    continue.
-   Test wallet and transaction behavior thoroughly before using the
    Alpha SDK with assets of significant value.
-   Use HTTPS RPC endpoints and do not disable TLS certificate
    verification.
-   Never log private keys, wallet passwords, or exported wallet
    contents.

------------------------------------------------------------------------

## Version

``` text
PlayTradeX SDK: 0.1.0-alpha
Unity:           6000.3
Windows:         x64
Android:         arm64-v8a
Android API:     26+
```

------------------------------------------------------------------------

[← Repository README](../README.md)
