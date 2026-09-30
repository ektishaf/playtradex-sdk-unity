# PlayTradeX SDK for Unity

**Native blockchain infrastructure for Unity games and interactive applications.**

PlayTradeX is a cross-platform blockchain SDK for game developers who want to integrate EVM-compatible blockchain functionality without moving wallet management, networking, cryptography, secure storage, transaction execution, and platform-specific code into gameplay systems.

The Unity package provides a developer-friendly C# layer while the core blockchain functionality remains inside the native PlayTradeX SDK.

> **Current Release:** `0.2.0-alpha`  
> **Unity:** `6000.3`  
> **Status:** Alpha / Pre-release  
> **Supported Platforms:** Windows x64 and Android arm64-v8a  
> **Configuration:** `Edit > Project Settings > PlayTradeX`  
> **Stable Target:** `1.0.0` planned for December

---

## Why PlayTradeX?

Blockchain integration in a game should not require every Unity team to build wallet handling, native cryptography, RPC networking, transaction signing, secure platform storage, ABI tooling, native bridges, and platform-specific wallet workflows from scratch.

PlayTradeX puts those responsibilities behind a Unity-oriented SDK so game code can work with **configured networks**, **wallet IDs**, and familiar asynchronous operations.

`0.2.0-alpha` expands the original SDK foundation with:

- Multi-network EVM configuration
- Multiple RPC endpoints per network
- Multiple application-managed wallets
- Network-aware blockchain execution
- Identity-wallet and external-wallet execution
- Built-in EVM testnet presets
- Generated network and wallet constants
- Centralized Unity Project Settings
- SDK updater and package information tooling
- Expanded Windows and Android integration
- Updated Unity sample

---

# Features

## Blockchain

- Native EVM blockchain integration
- Multiple configured EVM networks
- Multiple RPC endpoints per network
- Native currency balance queries
- Native currency transfers
- Read-only smart contract calls
- Transaction-producing smart contract writes
- Network-aware execution
- Application-controlled transaction consent
- Asynchronous blockchain operations
- Exact string-based blockchain unit conversion

## Wallets

- Native PlayTradeX identity wallet
- Multiple application-managed external wallets
- Wallet generation from Unity Project Settings
- Developer-defined wallet IDs
- Secure platform storage for the PlayTradeX identity wallet
- Encrypted wallet export and import
- Android Keystore integration
- Android Storage Access Framework integration
- Android `content://` wallet file support

## Unity Developer Experience

- Centralized `Project Settings > PlayTradeX` configuration
- Built-in EVM testnet presets
- Generated `GeneratedNetworks` constants
- Generated `GeneratedWallets` constants
- Automatic settings asset initialization
- Automatic generated-folder initialization
- ABI Converter Editor utility
- SDK About window
- SDK update tooling
- Installation and revision information
- Unity main-thread callback dispatch
- Included sample scene and scripts

## Native Platform Integration

- Windows x64
- Android arm64-v8a
- Android API Level 26+
- HTTPS/TLS certificate verification
- Native C++ SDK underneath the Unity integration
- C ABI bridge for Unity/native interoperability

---

# Platform Support

| Platform | Architecture | `0.2.0-alpha` |
|---|---|---|
| Windows | x64 | **Alpha Supported** |
| Android | arm64-v8a | **Alpha Supported** |
| macOS | — | Planned |
| Linux | — | Planned |
| iOS | — | Planned |

Windows and Android are the supported targets for `0.2.0-alpha`.

---

# Installation

## Unity Package Manager

In **Unity 6000.3**, open:

`Window > Package Manager`

Choose:

`+ > Install package from git URL...`

Enter:

```text
https://github.com/ektishaf/playtradex-sdk-unity.git#v0.2.0-alpha
```

Using the tagged release is strongly recommended.

The `#v0.2.0-alpha` revision pins the project to this exact SDK release instead of following changes on the repository branch.

## `manifest.json`

You can also add PlayTradeX directly to the project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.playtradex.sdk": "https://github.com/ektishaf/playtradex-sdk-unity.git#v0.2.0-alpha"
  }
}
```

---

# Requirements

- Unity `6000.3`
- Windows x64 or Android arm64-v8a
- Android API Level 26 or newer
- Internet access for blockchain operations
- At least one configured EVM network
- At least one working RPC endpoint for every network you intend to use

---

# Quick Start

The most important change in `0.2.0-alpha` is that **network and wallet configuration is centralized in Unity Project Settings**.

You no longer need to treat a single RPC URL and Chain ID on a scene component as the application's blockchain configuration.

The recommended setup is:

```text
Install PlayTradeX
        ↓
Project Settings > PlayTradeX
        ↓
Configure Storage
        ↓
Configure Networks
        ↓
Configure / Generate Wallets if needed
        ↓
Generate Network + Wallet constants
        ↓
Add PlayTradeXLifecycle to startup scene
        ↓
Wait until PlayTradeX is ready
        ↓
Use configured network/wallet IDs
```

---

# 1. Open PlayTradeX Project Settings

Open:

`Edit > Project Settings > PlayTradeX`

This is the central developer configuration surface for PlayTradeX.

The configuration belongs to the Unity project rather than the installed package.

PlayTradeX creates the project settings asset under:

```text
Assets/PlayTradeX/Resources/PlayTradeXSettings.asset
```

Generated developer constants are placed under:

```text
Assets/PlayTradeX/Generated/
├── Networks/
│   └── GeneratedNetworks.cs
└── Wallets/
    └── GeneratedWallets.cs
```

> Do not edit generated files manually. Regenerate them from PlayTradeX Project Settings after changing identifiers.

---

# 2. Configure Storage

In:

`Project Settings > PlayTradeX`

configure **Storage Path** when a custom location is required.

If the field is left empty, PlayTradeX uses:

```csharp
Application.persistentDataPath
```

## Windows

A custom absolute writable path can be used when required.

Example:

```text
C:\MyGame\PlayTradeX
```

## Android

Leaving the storage path empty is recommended so PlayTradeX uses the application's platform-specific persistent storage location.

Broad Android storage permissions are not required for the wallet document-picker workflow.

---

# 3. Configure Networks

Networks are configured under:

`Edit > Project Settings > PlayTradeX > Networks`

Each network configuration can contain:

| Setting | Purpose |
|---|---|
| `ID` | Developer-facing unique network identifier |
| `Network Name` | Human-readable network name |
| `RPC URLs` | One or more RPC endpoints |
| `Chain ID` | EVM chain ID |
| `Symbol` | Native currency symbol |
| `Block Explorer URL` | Explorer base URL |
| `Is Testnet` | Identifies a test network where applicable |
| `Preset ID` | Associates a preset-created network with its PlayTradeX preset where applicable |

A **network ID** is intended for code.

A **network name** is intended for developers and user-facing interfaces.

For example:

```text
ID:                 bsc-testnet
Network Name:       Binance Smart Chain Testnet
Chain ID:           97
Symbol:             tBNB
Block Explorer URL: https://testnet.bscscan.com
```

## Multiple RPC Endpoints

A network can contain multiple RPC URLs.

This lets a network configuration carry more than one endpoint instead of requiring a single RPC URL to be hardcoded throughout the project.

Use reliable HTTPS endpoints appropriate for the network being configured.

---

# 4. Built-in Testnet Presets

`0.2.0-alpha` introduces package-owned EVM testnet presets.

In the **Networks** section, PlayTradeX provides:

- **Add Testnet**
- **Add All Testnets**
- **Remove Testnets**
- **Generate Class**

### Add Testnet

Opens the PlayTradeX testnet catalog and lets you add an individual network without manually entering all of its configuration.

### Add All Testnets

Adds the available PlayTradeX testnet presets that are not already configured.

### Remove Testnets

Removes configured PlayTradeX testnets while leaving custom/mainnet configurations intact.

### Initial Testnet Catalog

The initial catalog includes:

- Ethereum Sepolia
- Binance Smart Chain Testnet
- opBNB Testnet
- Polygon Amoy
- Arbitrum Sepolia
- Avalanche Fuji C-Chain
- Linea Sepolia
- ZKsync Era Sepolia
- Gnosis Chiado

Preset data includes:

- Network ID
- Network name
- Ecosystem
- Chain ID
- Native currency symbol
- RPC endpoint(s)
- Block explorer URL
- Testnet classification

> Public RPC endpoints are external infrastructure and can change independently of PlayTradeX. If an endpoint becomes unavailable, configure another valid endpoint for that network.

---

# 5. Generate Network Constants

After configuring networks, click:

**Generate Class**

PlayTradeX generates:

```text
Assets/PlayTradeX/Generated/Networks/GeneratedNetworks.cs
```

This lets application code refer to configured network IDs without scattering raw strings throughout the project.

Conceptually:

```csharp
GeneratedNetworks.EthereumSepolia
GeneratedNetworks.BscTestnet
GeneratedNetworks.PolygonAmoy
```

The generated constant values come from the configured **network IDs**.

If you rename a network ID, regenerate the class.

---

# 6. Configure Wallets

Wallets are managed from the **Wallets** section of PlayTradeX Project Settings.

`0.2.0-alpha` supports application-managed wallet configuration in addition to the native PlayTradeX identity wallet.

A configured external wallet can contain:

- Developer-defined wallet ID
- Wallet address
- Private key

You can either:

- Add an existing wallet configuration manually, or
- Use the PlayTradeX wallet generation workflow and add the generated wallet to the settings

## Wallet IDs

Wallet IDs are developer-facing identifiers used to select configured wallets.

They let gameplay and application code work with identifiers instead of passing private keys throughout the project.

> **Security:** Treat external-wallet configuration as sensitive developer/application configuration. Never print, log, commit, transmit, or unintentionally expose private keys.

---

# 7. Generate Wallet Constants

After configuring wallets, generate the wallet class from PlayTradeX Project Settings.

PlayTradeX generates:

```text
Assets/PlayTradeX/Generated/Wallets/GeneratedWallets.cs
```

Application code can then use generated identifiers conceptually like:

```csharp
GeneratedWallets.PlayerWallet
GeneratedWallets.TreasuryWallet
```

instead of raw identifiers:

```csharp
"player-wallet"
"treasury-wallet"
```

Regenerate the class whenever wallet IDs change.

---

# 8. Add the Lifecycle Component

Add the `PlayTradeXLifecycle` component to a GameObject in the application's startup scene.

The lifecycle component initializes the required platform integration and native PlayTradeX SDK using the PlayTradeX project configuration.

For `0.2.0-alpha`, **RPC URLs and Chain IDs belong to network configuration**, rather than representing a single scene-level blockchain configuration.

Do not start blockchain operations until PlayTradeX initialization has completed successfully.

---

# 9. Know When PlayTradeX Is Ready

For initialization-driven application flow, subscribe to:

```csharp
PlayTradeXUnity.Ready += OnPlayTradeXReady;

private void OnPlayTradeXReady()
{
    Debug.Log("PlayTradeX is ready.");
}
```

When you only need to query the current state:

```csharp
if (PlayTradeXLifecycle.IsInitialized)
{
    Debug.Log("PlayTradeX is ready.");
}
```

Use the `Ready` event when code should react to initialization and `PlayTradeXLifecycle.IsInitialized` when checking the current SDK state.

---

# Network and Wallet Execution Model

`0.2.0-alpha` separates **which network to use** from **which wallet signs a transaction**.

```text
Blockchain Operation
        │
        ├── Network
        │      ↓
        │   Network ID
        │      ↓
        │   Network Configuration
        │   ├── Chain ID
        │   └── RPC endpoint(s)
        │
        └── Wallet Source
               ├── PlayTradeX Identity Wallet
               └── Configured External Wallet
```

Applications are therefore no longer designed around one globally hardcoded chain.

## Read Operations

Read-only blockchain operations select a configured network but do not require a signing wallet.

## Transaction Operations

Native currency transfers and contract writes require:

- A configured network
- A wallet source
- Transaction approval where applicable

Use generated network and wallet identifiers instead of embedding private keys, chain IDs, or RPC configuration throughout gameplay scripts.

---

# Native Balance

PlayTradeX can query native currency balances against configured EVM networks.

The returned blockchain balance uses its base-unit representation.

For an 18-decimal EVM native currency:

```csharp
string readable =
    PlayTradeXUnits.FromWei(
        response.Balance);

Debug.Log(readable);
```

Use the network-aware API demonstrated by the included `0.2.0-alpha` sample when selecting a configured network and wallet.

---

# Exact Blockchain Units

Avoid using `float` or `double` for blockchain currency values when exact precision matters.

PlayTradeX provides string-based conversion utilities.

## Native Currency → Wei

```csharp
string wei =
    PlayTradeXUnits.ToWei("0.01");
```

Result:

```text
10000000000000000
```

Convert back:

```csharp
string amount =
    PlayTradeXUnits.FromWei(
        "10000000000000000");
```

## Token Amounts

For an 18-decimal token:

```csharp
string amount =
    PlayTradeXUnits.ToBaseUnit(
        "1.5",
        18);
```

Result:

```text
1500000000000000000
```

For a 6-decimal token:

```csharp
string amount =
    PlayTradeXUnits.ToBaseUnit(
        "12.5",
        6);
```

Convert back:

```csharp
string readable =
    PlayTradeXUnits.FromBaseUnit(
        "12500000",
        6);
```

---

# Send Native Currency

Native currency transfers are transaction-producing operations.

The low-level transaction layer uses base-unit values, so a human-readable amount can be converted first:

```csharp
string amountWei =
    PlayTradeXUnits.ToWei("0.01");
```

In `0.2.0-alpha`, transaction execution is network-aware and can use either:

- The PlayTradeX identity wallet, or
- A configured external wallet

Use the corresponding network/wallet overload demonstrated in the included sample.

The transaction may enter the PlayTradeX transaction-consent flow before submission.

---

# Smart Contract Reads

PlayTradeX supports read-only EVM contract calls.

A read:

- Selects a configured network
- Uses a human-readable function ABI/signature
- Accepts function parameters according to the SDK API
- Does not create a blockchain transaction
- Does not require transaction approval

The ABI Converter can help generate and maintain human-readable contract information.

---

# Smart Contract Writes

Contract writes create blockchain transactions.

A write can select:

- Configured network
- PlayTradeX identity wallet or configured external wallet
- Contract address
- Function
- Parameters
- Transaction value where applicable

Transaction-producing calls can use the PlayTradeX consent flow before submission.

---

# Transaction Approval

PlayTradeX supports application-controlled transaction consent.

Before a transaction is submitted, the application can present transaction information such as:

- Contract
- Function
- Parameters
- Value
- Estimated gas
- Preparation status
- Simulation information where available

The application can then approve or deny the pending transaction.

Approve:

```csharp
PlayTradeXSdk.ApproveTransaction(
    transactionId);
```

Deny:

```csharp
PlayTradeXSdk.DenyTransaction(
    transactionId);
```

A failed transaction preparation or simulation is different from a user denial.

Applications should present preparation/simulation failures appropriately rather than reporting them as if the player pressed **Deny**.

See the included sample for the complete transaction-consent UI flow.

---

# ABI Converter

PlayTradeX includes an ABI Converter inside the Unity Editor.

Open:

`PlayTradeX > ABI Converter`

Paste a standard JSON contract ABI.

The converter can produce human-readable contract information and generate a C# contract interface according to the available tool options.

This reduces the need to manually maintain Solidity function signatures throughout a Unity project.

---

# Wallet Export

PlayTradeX can create an encrypted backup of the native PlayTradeX wallet using a password supplied by the user.

The application provides the export password and a destination selected by the user.

On Android, PlayTradeX uses the Android Storage Access Framework so the user can select a destination through the system document picker.

Android `content://` document URIs are supported.

Never log:

- Wallet passwords
- Private keys
- Exported wallet contents

---

# Wallet Import

Wallet import restores an encrypted PlayTradeX wallet backup selected by the user.

On Android, the backup can be selected through the system document picker and read through a `content://` URI.

Wallet import is intentionally synchronous because it modifies sensitive persistent wallet state.

Recommended flow:

1. Initialize PlayTradeX.
2. Ask the user to select the wallet backup.
3. Import the wallet using the supplied password.
4. Confirm that the import succeeded.
5. Restart the application.
6. Allow PlayTradeX to initialize normally using the imported wallet.

```csharp
WalletImportResponse response =
    PlayTradeXSdk.ImportWallet(
        password,
        inputPath);

if (response.Success)
{
    Debug.Log(
        "Wallet imported. Restart the application.");
}
```

Do not continue normal blockchain operations using the previous wallet session after replacing the persisted identity wallet.

---

# Android Integration

PlayTradeX automatically initializes its Android platform bridge through Unity.

Unity developers do not need to manually pass an Android `Context` to the native SDK.

The Android integration provides:

- Android Keystore-backed secure storage
- Android document/content URI access
- Wallet backup and restore through the system document picker
- CA certificate setup
- Native PlayTradeX library loading
- arm64-v8a native support

Current Android target:

```text
arm64-v8a
API Level 26+
```

Broad storage permissions are not required for the Storage Access Framework wallet workflow.

---

# PlayTradeX Updater

`0.2.0-alpha` includes PlayTradeX package update tooling.

The updater provides information about the installed package and available PlayTradeX releases.

For Git installations, install tagged releases whenever possible:

```text
https://github.com/ektishaf/playtradex-sdk-unity.git#v0.2.0-alpha
```

A tagged installation gives the project a reproducible SDK revision.

An installation without an explicit Git revision can be reported as:

```text
Unpinned
```

---

# About PlayTradeX

The PlayTradeX Editor tooling includes package information such as:

- Installed SDK version
- Release channel
- Unity compatibility
- Installation source
- Installed Git revision where available

This information is useful when reporting integration issues.

---

# Sample

The Unity package includes a PlayTradeX sample that can be imported from Unity Package Manager.

The sample demonstrates:

- SDK initialization
- Network selection
- Wallet selection
- Native balance retrieval
- Native currency transfers
- Contract reads
- Contract writes
- Transaction consent
- Wallet export
- Wallet import
- Response and error handling

The sample scene is:

```text
PlayTradeXSample.unity
```

When the sample scene is opened, its Editor helper configures the Game View for a **1920 × 1080 Full HD** preview where supported by the targeted Unity Editor version.

---

## Before Running the Sample

### Import TMP Essential Resources

The sample UI uses TextMesh Pro.

Open:

`Window > TextMeshPro > Import TMP Essential Resources`

and complete the import before running the sample.

### Verify the EventSystem

The sample requires a working Unity `EventSystem`.

Select the `EventSystem` GameObject and verify that its input module is compatible with the input system configured by the project.

The project's input configuration can be reviewed under:

`Edit > Project Settings > Player > Other Settings > Active Input Handling`

PlayTradeX itself does not require developers to switch to a specific Unity input system.

This requirement applies to the included sample UI.

---

# Security

PlayTradeX is designed so sensitive blockchain functionality can remain behind the SDK/native integration rather than being spread throughout gameplay code.

Applications integrating PlayTradeX should follow these rules:

- Never log private keys.
- Never log wallet passwords.
- Never commit real private keys to source control.
- Never store private keys in `PlayerPrefs`.
- Do not expose external-wallet private keys through gameplay/UI code.
- Never use `float` or `double` for currency values when exact blockchain precision is required.
- Verify transaction information before approval.
- Use HTTPS RPC endpoints.
- Do not disable TLS certificate verification.
- Keep PlayTradeX updated as fixes are released.
- Use test networks and test assets while developing and validating an integration.

> External wallets configured in Unity Project Settings are application/developer-managed credentials. Developers are responsible for deciding whether that configuration model is appropriate for their shipping application's security architecture.

---

# Migrating from `0.1.0-alpha`

The largest architectural change in `0.2.0-alpha` is the move from a single network configuration toward centralized multi-network and multi-wallet configuration.

## `0.1.0-alpha`

```text
Lifecycle
    ↓
Single RPC URL
Single Chain ID
    ↓
Blockchain operations
```

## `0.2.0-alpha`

```text
Project Settings > PlayTradeX
        ↓
┌─────────────────────────┐
│ Storage                 │
│ Networks[]              │
│ Wallets[]               │
└─────────────────────────┘
        ↓
GeneratedNetworks
GeneratedWallets
        ↓
Network-aware / wallet-aware operations
```

When upgrading:

1. Open `Edit > Project Settings > PlayTradeX`.
2. Move network configuration into the **Networks** section.
3. Configure one or more RPC endpoints for each network.
4. Add or generate application-managed wallets if required.
5. Generate `GeneratedNetworks.cs`.
6. Generate `GeneratedWallets.cs`.
7. Update application code to select networks by network ID.
8. Update external-wallet operations to select configured wallets by wallet ID.
9. Use the `0.2.0-alpha` sample as the reference integration.
10. Test transaction approval, wallet operations, and blockchain calls before shipping.

Do not rely on a single scene-level RPC URL or Chain ID as the application's network model.

---

# Alpha Notice

PlayTradeX `0.2.0-alpha` is a prerelease intended for development, integration testing, and developer feedback.

APIs, configuration structures, behavior, platform support, and package structure may change before the stable `1.0.0` release.

Developers should thoroughly test wallet and transaction functionality in their own environment before using PlayTradeX with assets of significant value.

---

# Roadmap to PlayTradeX `1.0.0`

`0.2.0-alpha` already introduces multi-network configuration and network-aware execution to the Unity SDK.

The planned December `1.0.0` release is intended to expand the SDK further across engines and platforms.

Planned areas include:

- **Unreal Engine support** — native PlayTradeX integration with an included Unreal sample.
- **Continued Unity support** — sample, tooling, API, and integration improvements.
- **macOS support**.
- **iOS support**.
- **Expanded network and RPC reliability**.
- **Continued wallet lifecycle improvements**.
- **Expanded SDK tooling and documentation**.
- **Production-readiness work** across supported platforms.

> Roadmap items are planned targets and may change as the SDK approaches the stable release.

---

# Documentation

Additional package documentation is available under:

```text
Documentation~/
```

For practical integration behavior, also review the included sample corresponding to the installed SDK version.

---

# Contributing & Feedback

PlayTradeX is currently in Alpha.

Developer feedback from real Unity integrations is valuable for improving:

- API design
- Network configuration
- Wallet workflows
- Platform support
- Editor tooling
- Documentation
- Samples
- SDK stability

When reporting an issue, include:

- PlayTradeX package version
- Unity version
- Target platform
- Architecture
- Network being used
- Whether the PlayTradeX identity wallet or an external wallet is being used, when relevant
- The smallest reproducible example possible

Never include private keys, wallet passwords, seed phrases, or other secrets in an issue report.

---

# License

See `LICENSE.md` for licensing information.

---

**PlayTradeX — native blockchain infrastructure for game developers.**

Copyright © 2026 PlayTradeX.