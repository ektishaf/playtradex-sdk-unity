# PlayTradeX SDK for Unity

**Native blockchain infrastructure for Unity games and interactive
applications.**

PlayTradeX is a cross-platform blockchain SDK designed for game
developers who want to integrate EVM-compatible blockchain functionality
without moving wallet, networking, cryptography, and platform-specific
implementation into gameplay code.

The Unity package exposes a developer-friendly C# API while the core
blockchain functionality remains inside the native PlayTradeX SDK.

> **Current Release:** `0.1.0-alpha`\
> **Unity:** `6000.3`\
> **Status:** Alpha\
> **Supported Platforms:** Windows x64 and Android arm64-v8a\
> **Next Major Release:** `1.0.0` planned for December --- Unreal
> Engine, multi-network support, macOS and iOS

------------------------------------------------------------------------

## Why PlayTradeX?

Blockchain integration in a game should not require every Unity
developer to build wallet management, native cryptography, transaction
handling, secure storage, ABI utilities, HTTP networking, and platform
bridges from scratch.

PlayTradeX provides these capabilities behind a Unity-oriented API so
developers can focus on their application and gameplay architecture.

The Alpha release includes native wallet management, EVM transactions,
smart contract interaction, secure platform storage, transaction
approval, wallet backup and restore, exact blockchain unit conversion,
and Unity Editor tooling.

------------------------------------------------------------------------

## Features

-   Native EVM blockchain integration
-   Unity C# API
-   Native wallet management
-   Secure wallet storage
-   Native currency balance queries and transfers
-   Smart contract reads and writes
-   Transaction approval flow
-   Human-readable ABI conversion
-   ABI Converter Unity Editor utility
-   Encrypted wallet export and import
-   Exact string-based blockchain unit conversion
-   Android Keystore integration
-   Android Storage Access Framework integration for wallet files
-   HTTPS/TLS certificate verification
-   Asynchronous blockchain operations
-   Unity main-thread callback dispatch

------------------------------------------------------------------------

## Platform Support

  Platform   Architecture   Status
  ---------- -------------- -----------------
  Windows    x64            Alpha Supported
  Android    arm64-v8a      Alpha Supported
  macOS      ---            Planned
  Linux      ---            Planned
  iOS        ---            Planned

Windows and Android are the supported targets for `0.1.0-alpha`.
Additional platforms are planned for future releases.

------------------------------------------------------------------------

# Installation

## Unity Package Manager

PlayTradeX is distributed as a Unity package and can be installed
directly from Git.

In **Unity 6000.3**, open:

`Window > Package Manager`

Select:

`+ > Install package from git URL...`

Enter:

``` text
https://github.com/ektishaf/playtradex-sdk-unity.git#v0.1.0-alpha
```

Using the tagged release is recommended so your project does not
unexpectedly track development changes.

> **Important:** Install PlayTradeX using the complete Git URL including the
> release tag. For this release, use
> `https://github.com/ektishaf/playtradex-sdk-unity.git#v0.1.0-alpha`.
> The `#v0.1.0-alpha` tag pins the package to this specific SDK release.

## `manifest.json`

You can also add PlayTradeX directly to your project's
`Packages/manifest.json`:

``` json
{
  "dependencies": {
    "com.playtradex.sdk": "https://github.com/ektishaf/playtradex-sdk-unity.git#v0.1.0-alpha"
  }
}
```

------------------------------------------------------------------------

# Requirements

-   Unity `6000.3`
-   Windows x64 or Android arm64-v8a
-   Android API Level 26 or newer
-   Internet connection for blockchain operations
-   An EVM-compatible RPC endpoint

------------------------------------------------------------------------

# Getting Started

## Before Running the Sample

### Import TMP Essential Resources

The PlayTradeX sample UI uses TextMesh Pro. Before running the sample,
make sure the required TMP Essential Resources are available in the
project.

In Unity, open:

`Window > TextMeshPro > Import TMP Essential Resources`

Complete the import before running the PlayTradeX sample scene.

### Verify the EventSystem Input Module

The PlayTradeX sample UI requires a working Unity `EventSystem`.

After importing the sample, select the `EventSystem` GameObject in the
sample scene and verify that it has an input module compatible with the
input handling configured for your project.

If Unity indicates that the current input module is not compatible, use
the **Add Input Module** option when available, or add the appropriate
input module for the input system selected by your project.

You can review the project's input configuration under:

`Edit > Project Settings > Player > Other Settings > Active Input Handling`

Use the input module that corresponds to the project's selected input
handling configuration.

> PlayTradeX does not require developers to switch to a specific Unity
> input system. This requirement applies to the included sample UI so its
> buttons and other UI controls can receive input correctly.

## 1. Add the Lifecycle Component

Add the `PlayTradeXLifecycle` component to a GameObject in your initial
Unity scene.

Configure:

-   RPC URL
-   Chain ID
-   Storage Path, if required
-   Auto Initialize

`PlayTradeXLifecycle` initializes the required platform integration and
the native PlayTradeX SDK.

For `0.1.0-alpha`, PlayTradeX operations should only be used after SDK
initialization has completed successfully.

## 2. Know When PlayTradeX Is Ready

For code that needs to react as soon as SDK initialization completes,
subscribe to the `PlayTradeXUnity.Ready` event:

``` csharp
PlayTradeXUnity.Ready += OnPlayTradeXReady;

private void OnPlayTradeXReady()
{
    Debug.Log("PlayTradeX is ready.");
}
```

When you only need to query the current initialization state, use:

``` csharp
if (PlayTradeXLifecycle.IsInitialized)
{
    Debug.Log("PlayTradeX is ready.");
}
```

Use the `Ready` event for initialization-driven application flow and
`PlayTradeXLifecycle.IsInitialized` when checking the current SDK state.

------------------------------------------------------------------------

# Native Balance

Retrieve the active wallet's native blockchain balance:

``` csharp
NativeBalanceResponse response = await PlayTradeXSdk.GetNativeBalanceAsync();

if (response.Success)
{
    Debug.Log("Balance: " + response.Balance);
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

Blockchain balances returned by the SDK use their base-unit
representation. For an 18-decimal EVM native currency:

``` csharp
string readable = PlayTradeXUnits.FromWei(response.Balance);
Debug.Log(readable);
```

------------------------------------------------------------------------

# Exact Blockchain Units

Avoid using `float` or `double` for blockchain currency values when
exact precision matters. PlayTradeX provides string-based conversion
utilities.

## Native Currency

``` csharp
string wei = PlayTradeXUnits.ToWei("0.01");
```

Result:

``` text
10000000000000000
```

Convert back:

``` csharp
string amount = PlayTradeXUnits.FromWei("10000000000000000");
```

## Token Amounts

For ERC-20 tokens, provide the token's decimals:

``` csharp
string amount = PlayTradeXUnits.ToBaseUnit("1.5", 18);
```

Result:

``` text
1500000000000000000
```

For a 6-decimal token:

``` csharp
string amount = PlayTradeXUnits.ToBaseUnit("12.5", 6);
```

Convert back with:

``` csharp
string readable = PlayTradeXUnits.FromBaseUnit("12500000", 6);
```

------------------------------------------------------------------------

# Send Native Currency

The low-level PlayTradeX transaction API accepts the amount in wei.

``` csharp
string amountWei = PlayTradeXUnits.ToWei("0.01");
TransactionResponse response = await PlayTradeXSdk.SendEthAsync(destinationAddress, amountWei);
```

Advanced developers can provide the wei value directly:

``` csharp
await PlayTradeXSdk.SendEthAsync(destinationAddress, "10000000000000000");
```

------------------------------------------------------------------------

# Smart Contract Reads

PlayTradeX supports read-only EVM contract calls.

``` csharp
ContractReadResponse response =
    await PlayTradeXSdk.ReadAsync(contractAddress, functionSignature, parameters);
```

Read operations query contract state without submitting a blockchain
transaction.

------------------------------------------------------------------------

# Smart Contract Writes

Contract writes create blockchain transactions.

``` csharp
TransactionResponse response =
    await PlayTradeXSdk.WriteAsync(contractAddress, functionSignature, parameters);
```

Applications can use the PlayTradeX transaction consent callback to
present transaction information to the player before approving or
rejecting a transaction.

------------------------------------------------------------------------

# Transaction Approval

PlayTradeX supports application-controlled transaction approval. A game
or application can present:

-   Contract
-   Function
-   Parameters
-   Value
-   Estimated gas

before allowing the transaction to continue.

This keeps the final approval experience under the application's control
while transaction execution remains handled through the SDK.

See the included PlayTradeX sample for an example transaction approval
UI.

------------------------------------------------------------------------

# ABI Converter

PlayTradeX includes an ABI Converter inside the Unity Editor.

Open:

`PlayTradeX > ABI Converter`

Paste a standard JSON contract ABI.

The converter can generate a C# contract interface containing
human-readable function signatures and can optionally embed:

-   Contract ABI
-   Human-readable ABI
-   Contract address

This reduces the need to manually maintain Solidity function signature
strings throughout a Unity project.

------------------------------------------------------------------------

# Wallet Export

PlayTradeX can create an encrypted wallet backup using a password
supplied by the user.

The application provides the export password and a destination selected
by the user.

On Android, PlayTradeX integrates with the Android Storage Access
Framework so the user can choose the destination through the system
document picker. The SDK supports Android `content://` document URIs for
the resulting wallet backup.

Never log wallet passwords or exported wallet contents.

------------------------------------------------------------------------

# Wallet Import

Wallet import restores an encrypted PlayTradeX wallet backup selected by
the user.

On Android, wallet backups can be selected through the system document
picker and read through Android content URIs.

Wallet import is intentionally synchronous because it modifies sensitive
persistent wallet state.

For `0.1.0-alpha`:

1.  Initialize PlayTradeX.
2.  Import the wallet.
3.  Confirm the import succeeded.
4.  Restart the application.
5.  Allow PlayTradeX to initialize normally using the imported wallet.

``` csharp
WalletImportResponse response = PlayTradeXSdk.ImportWallet(password, inputPath);

if (response.Success)
{
    Debug.Log("Wallet imported. Restart the application.");
}
```

Do not continue normal blockchain operations using the previous session
after replacing the wallet.

------------------------------------------------------------------------

# Android Integration

PlayTradeX automatically initializes its Android platform bridge through
Unity. Unity developers do not need to manually pass an Android
`Context` to the native SDK.

The Android integration provides:

-   Android Keystore-backed secure storage
-   Android document/content URI access
-   Wallet backup and restore through the system document picker
-   CA certificate setup
-   Native PlayTradeX library loading

The `0.1.0-alpha` Android package currently includes:

``` text
arm64-v8a
```

The Android integration uses platform APIs for secure storage and
document access; broad storage permissions are not required for the
wallet document-picker workflow.

------------------------------------------------------------------------

# Security

PlayTradeX is designed so sensitive blockchain functionality remains
inside the native SDK.

The SDK uses platform-specific secure storage and authenticated
encryption where appropriate.

Applications integrating PlayTradeX should follow these rules:

-   Never log private keys.
-   Never log wallet passwords.
-   Never store private keys in `PlayerPrefs`.
-   Never use `float` or `double` for blockchain currency values when
    exact precision is required.
-   Always verify transaction information before approval.
-   Use HTTPS RPC endpoints.
-   Do not disable TLS certificate verification.
-   Keep PlayTradeX updated as security fixes are released.

------------------------------------------------------------------------

# Samples

A PlayTradeX sample is included with the Unity package and can be
imported through Unity Package Manager.

The sample demonstrates:

-   SDK initialization
-   Native balance retrieval
-   Native currency transfers
-   Contract reads
-   Contract writes
-   Transaction consent
-   Wallet operations
-   Response handling

The sample provides a practical starting point for integrating
PlayTradeX into a Unity `6000.3` project.

------------------------------------------------------------------------

# Alpha Notice

PlayTradeX `0.1.0-alpha` is an early release intended for development
and testing.

APIs, behavior, platform support, and package structure may change
before the stable `1.0.0` release.

Developers should thoroughly test wallet and transaction functionality
in their own environment before using PlayTradeX with assets of
significant value.

------------------------------------------------------------------------

# Coming in December --- PlayTradeX 1.0.0

The `0.1.0-alpha` release establishes the Unity and native SDK
foundation. The planned December `1.0.0` release expands PlayTradeX into
a broader multi-engine, multi-platform blockchain development SDK.

Planned for `1.0.0`:

-   **Unreal Engine support** --- native PlayTradeX integration for
    Unreal Engine with an included sample.
-   **Unity sample** --- the Unity package continues to include a
    practical sample demonstrating SDK initialization, wallet
    operations, transactions, contract interaction, transaction consent,
    and response handling.
-   **Multi-network support** --- applications will be able to work with
    multiple EVM-compatible networks instead of being limited to a
    single active network configuration.
-   **Cross-network, multi-contract execution** --- developers will be
    able to call multiple functions across multiple smart contracts and
    multiple configured networks through PlayTradeX.
-   **macOS support** --- extending the native SDK and engine
    integration beyond the current Windows and Android Alpha targets.
-   **iOS support** --- bringing PlayTradeX wallet, blockchain, and
    platform integration to iOS.
-   **Expanded production readiness** --- continued work on platform
    integration, wallet lifecycle, developer tooling, documentation,
    samples, and SDK stability.

The direction for `1.0.0` is to let game teams integrate blockchain
functionality through a consistent PlayTradeX workflow across engines,
contracts, networks, and supported platforms without rebuilding the
underlying wallet, networking, cryptography, and platform layers for
every project.

> The December `1.0.0` items above are planned roadmap targets and may
> evolve as the SDK approaches its stable release.

------------------------------------------------------------------------

# Documentation

Additional package documentation is available under:

``` text
Documentation~/
```

------------------------------------------------------------------------

# Contributing & Feedback

PlayTradeX is currently in Alpha. Developer feedback from real Unity
integrations is valuable for improving the API, platform support,
tooling, documentation, and production readiness of future releases.

When reporting an issue, include the Unity version, target platform,
architecture, PlayTradeX package version, and the smallest reproducible
example where possible.

------------------------------------------------------------------------

# License

See `LICENSE.md` for licensing information.

------------------------------------------------------------------------

**PlayTradeX --- native blockchain infrastructure for game developers.**

Copyright © 2026 PlayTradeX.
