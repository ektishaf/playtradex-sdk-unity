# Getting Started

[← Documentation Home](index.md) · [Next: Transactions
→](transactions.md)

This guide walks through the minimum setup required to initialize
PlayTradeX in a Unity project and know when the SDK is ready for
blockchain operations.

> **Documentation target:** PlayTradeX `0.1.0-alpha` · Unity `6000.3`

------------------------------------------------------------------------

## Requirements

-   Unity `6000.3`
-   Windows x64 or Android arm64-v8a
-   Android API Level 26 or newer
-   Internet access for blockchain operations
-   An EVM-compatible RPC endpoint

------------------------------------------------------------------------

## Installation

Install PlayTradeX through Unity Package Manager.

In **Unity 6000.3**, open:

`Window > Package Manager`

Choose:

`+ > Install package from git URL...`

Enter the tagged Alpha package URL:

``` text
https://github.com/ektishaf/playtradex-sdk-unity.git#v0.1.0-alpha
```

Using a tagged release is recommended so the project does not
unexpectedly track development changes.

You can also add the package to `Packages/manifest.json`:

``` json
{
  "dependencies": {
    "com.playtradex.sdk": "https://github.com/ektishaf/playtradex-sdk-unity.git#v0.1.0-alpha"
  }
}
```

------------------------------------------------------------------------

## Import the Unity Sample

The package includes a Unity sample intended to provide a practical
reference integration.

From Unity Package Manager, select the PlayTradeX package and import the
included sample.

The sample demonstrates:

-   SDK initialization
-   Native balance retrieval
-   Native currency transfers
-   Contract reads
-   Contract writes
-   Transaction consent
-   Wallet operations
-   Response handling

For a first integration, running the sample before wiring PlayTradeX
into production gameplay code is recommended.

------------------------------------------------------------------------

## SDK Initialization

Add `PlayTradeXLifecycle` to a GameObject in the initial scene of your
application.

Configure the component with:

-   **RPC URL** --- the EVM-compatible RPC endpoint used by the SDK
-   **Chain ID** --- the chain identifier for the configured network
-   **Storage Path** --- when required by the target
    platform/integration
-   **Auto Initialize** --- enables lifecycle-driven SDK initialization

For `0.1.0-alpha`, normal PlayTradeX operations should begin only after
initialization succeeds.

------------------------------------------------------------------------

## Recommended: React to the Ready Event

For systems that should start as soon as PlayTradeX becomes available,
subscribe to `PlayTradeXUnity.Ready`.

``` csharp
PlayTradeXUnity.Ready += OnPlayTradeXReady;

private void OnPlayTradeXReady()
{
    Debug.Log("PlayTradeX SDK is ready.");

    // Start systems that depend on PlayTradeX here.
}
```

This is useful for initialization-driven application flow because
dependent systems do not need to repeatedly poll the SDK state.

Remember to manage event subscriptions according to the lifetime of the
subscribing object.

------------------------------------------------------------------------

## Check the Current Initialization State

When code only needs to know whether initialization has already
completed, query:

``` csharp
if (PlayTradeXLifecycle.IsInitialized)
{
    Debug.Log("PlayTradeX SDK is ready.");
}
```

Use the `Ready` event to react to initialization completion and
`PlayTradeXLifecycle.IsInitialized` to inspect the current state.

------------------------------------------------------------------------

## What to Do After Initialization

Once PlayTradeX is ready, continue with the feature relevant to your
application:

-   [Transactions](transactions.md) --- balances, unit conversion,
    transfers, and approval flow
-   [Smart Contracts](contracts.md) --- contract reads and writes
-   [Wallet Management](wallet.md) --- secure wallet lifecycle, export,
    and import

------------------------------------------------------------------------

## Android Notes

PlayTradeX initializes its Android platform bridge automatically through
Unity. Unity application code does not need to manually provide an
Android `Context` to the native SDK.

The Alpha Android integration provides:

-   Android Keystore-backed secure storage
-   Android document/content URI access
-   Wallet backup and restore through the system document picker
-   CA certificate setup
-   Native PlayTradeX library loading

The current Android Alpha target is:

``` text
arm64-v8a
```

------------------------------------------------------------------------

## Troubleshooting Checklist

If a PlayTradeX operation is attempted before the SDK is ready, first
verify:

1.  `PlayTradeXLifecycle` exists in the initial scene.
2.  The RPC URL and Chain ID are configured correctly.
3.  Auto Initialize is enabled if lifecycle-driven initialization is
    expected.
4.  Your dependent code waits for `PlayTradeXUnity.Ready` or verifies
    `PlayTradeXLifecycle.IsInitialized`.
5.  The current build target is supported by `0.1.0-alpha`.

------------------------------------------------------------------------

[← Documentation Home](index.md) · [Next: Transactions
→](transactions.md)
