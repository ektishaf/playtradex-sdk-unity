# Changelog

All notable changes to the **PlayTradeX SDK for Unity** are documented in this file.

The format of this changelog is intended to provide developers with a clear history of public SDK releases, supported platforms, major capabilities, behavioral changes, and known limitations.

---

## [Unreleased]

Development toward the stable `1.0.0` release is ongoing.

### Planned for 1.0.0

The following capabilities are currently planned for the December `1.0.0` release:

- Unreal Engine support with an included sample.
- Continued Unity support and sample improvements.
- macOS support.
- iOS support.
- Expanded SDK tooling and documentation.
- Continued wallet lifecycle improvements.
- Continued network and RPC reliability improvements.
- Expanded production-readiness work across supported platforms.

> Planned features and platform targets may change before the stable `1.0.0` release.

---

## [0.2.0-alpha] - 2026-09-30

### Multi-Network, Multi-Wallet and Developer Tooling Alpha

This release significantly expands the **PlayTradeX SDK for Unity** with configurable multi-network support, multiple wallet configurations, network-aware blockchain execution, improved Unity Project Settings integration, built-in EVM testnet presets, package update tooling, and an expanded sample.

The release continues to target **Unity 6000.3** with native support for **Windows x64** and **Android arm64-v8a**.

### Added

#### Multi-Network Support

- Configurable EVM blockchain networks.
- Multiple networks can be registered with PlayTradeX.
- Network selection through developer-defined network IDs.
- Network-specific blockchain execution.
- Support for multiple RPC endpoints per configured network.
- Configurable chain ID for each network.
- Configurable native currency symbol for each network.
- Configurable human-readable network name.
- Configurable block explorer URL for each network.
- Runtime network configuration generated from Unity Project Settings.
- `GeneratedNetworks` constants for accessing configured network IDs without hardcoded strings.

#### Testnet Presets

- Built-in EVM testnet preset catalog.
- `Add Testnet` dropdown in PlayTradeX Project Settings.
- `Add All Testnets` support.
- `Remove Testnets` support.
- Automatic population of network configuration from testnet presets.
- Testnet presets include:
  - Network ID.
  - Network name.
  - Ecosystem.
  - Chain ID.
  - Native currency symbol.
  - RPC endpoints.
  - Block explorer URL.
  - Testnet classification.
- Initial preset support for:
  - Ethereum Sepolia.
  - Binance Smart Chain Testnet.
  - opBNB Testnet.
  - Polygon Amoy.
  - Arbitrum Sepolia.
  - Avalanche Fuji C-Chain.
  - Linea Sepolia.
  - ZKsync Era Sepolia.
  - Gnosis Chiado.

#### Multi-Wallet Support

- Multiple developer-configured wallets.
- Wallet configuration through PlayTradeX Project Settings.
- Developer-defined wallet IDs.
- Wallet selection by wallet ID.
- External wallet support for blockchain operations.
- Existing PlayTradeX identity wallet support retained.
- Wallet generation directly from PlayTradeX Project Settings.
- Generated wallets can be added to the project's wallet configuration.
- Manual wallet configuration support.
- `GeneratedWallets` constants for accessing configured wallet IDs without hardcoded strings.

#### Execution Context

- Network-aware execution context for native blockchain operations.
- Wallet-source selection between the PlayTradeX identity wallet and external wallets.
- External private-key execution support through the native SDK.
- Network-aware native currency transactions.
- Network-aware smart contract write operations.
- Unity-facing overloads for selecting configured networks and wallets.
- C ABI adapters for identity-wallet and external-wallet execution.

#### PlayTradeX Project Settings

- Dedicated **Project > PlayTradeX** settings interface.
- Centralized PlayTradeX configuration inside Unity Project Settings.
- Configurable SDK storage path.
- Network configuration management.
- Wallet configuration management.
- Network preset management.
- Network constants generation.
- Wallet constants generation.
- Automatic PlayTradeX settings asset creation.
- Automatic PlayTradeX project folder initialization.
- Automatic generated-code folder initialization.

#### Generated Configuration

- Automatic `GeneratedNetworks.cs` generation.
- Automatic `GeneratedWallets.cs` generation.
- Constants generated from developer-defined network and wallet IDs.
- Project-owned generated files separated from package-owned runtime code.
- Generated network configuration designed to avoid hardcoded network identifiers in gameplay code.
- Generated wallet configuration designed to avoid hardcoded wallet identifiers in gameplay code.

#### SDK Updater

- PlayTradeX Unity SDK update tooling.
- Installed package version detection.
- Git installation detection.
- Installed Git revision reporting.
- Release/update information display.
- Support for tagged Git package installations.
- Support for local package development workflows.
- Update checks integrated into the PlayTradeX Editor tooling.

#### About Window

- PlayTradeX SDK About interface.
- Package version information.
- Installation source information.
- Git revision information.
- Unity compatibility information.
- PlayTradeX package metadata display.
- PlayTradeX Editor branding and package icon integration.

#### Unity Sample

- Expanded PlayTradeX sample for multi-network operation.
- Network selection UI.
- Wallet selection UI.
- Native balance query example.
- Native currency transaction example.
- Smart contract read example.
- Smart contract write example.
- Wallet export example.
- Wallet import example.
- Sample configuration based on PlayTradeX Project Settings.
- Automatic Full HD `1920x1080` Game View configuration when opening the PlayTradeX sample scene.

---

### Changed

#### Blockchain Execution

- Blockchain operations are now network-aware.
- Native currency operations can execute against explicitly selected configured networks.
- Smart contract operations can execute against explicitly selected configured networks.
- Transaction execution can use either the PlayTradeX identity wallet or a configured external wallet.
- Native SDK interfaces expanded to support execution context information.
- C ABI expanded with dedicated external-wallet entry points for Unity interoperability.

#### Network Configuration

- Network configuration expanded from the initial network model to support multiple developer-configured EVM networks.
- Network configuration now supports multiple RPC endpoints.
- Network configuration now includes chain ID.
- Network configuration now includes network name.
- Network configuration now includes native currency symbol.
- Network configuration now includes block explorer information.
- Unity-specific configuration is converted into runtime/native-facing network configuration.

#### Wallet Configuration

- Wallet handling expanded to support multiple configured wallets.
- Wallet IDs can now be used by Unity application code instead of directly exposing private keys throughout gameplay code.
- External wallet execution is resolved through configured wallet information.
- Existing PlayTradeX identity-wallet behavior remains available.

#### Unity Integration

- PlayTradeX configuration moved toward a centralized Project Settings workflow.
- Package initialization expanded to create required settings and generated-code structures.
- Unity-facing API expanded for network and wallet selection.
- Sample workflows updated for the new network and wallet configuration model.

#### Native Integration

- Native C++ interfaces expanded for network-aware execution.
- Unity C ABI expanded while preserving C-compatible entry points.
- Native and managed integration updated for multi-network and external-wallet operations.
- Windows and Android native binaries updated for the expanded SDK interfaces.

#### Android Integration

- Android initialization and secure-storage integration improved.
- JNI initialization flow improved for secure wallet storage.
- Android wallet export and import integration retained through the Storage Access Framework.

#### Developer Experience

- PlayTradeX configuration is now primarily managed through Unity Project Settings.
- Network and wallet IDs can be represented by generated constants.
- Testnet setup no longer requires manually entering all standard network information.
- Package metadata and release information are exposed through PlayTradeX Editor tooling.

---

### Fixed

#### Android

- Improved JNI initialization for Android secure storage.
- Fixed secure-storage initialization ordering issues.
- Improved Android platform initialization reliability.

#### Unity Editor

- Improved PlayTradeX settings initialization.
- Improved generated folder creation and management.
- Improved package metadata display.
- Improved installed package revision reporting.
- Improved About window metadata presentation.

#### SDK Integration

- Improved managed/native configuration consistency.
- Improved network and wallet resolution across Unity and native SDK boundaries.
- Improved callback and configuration handling for expanded blockchain execution workflows.

---

### Supported Platforms

| Platform | Architecture | Status |
|---|---|---|
| Windows | x64 | Alpha Supported |
| Android | arm64-v8a | Alpha Supported |
| macOS | — | Planned |
| Linux | — | Planned |
| iOS | — | Planned |

### Unity Compatibility

```text
Unity 6000.3