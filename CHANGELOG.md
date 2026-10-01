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

## [0.3.0-alpha] - 2026-10-01

### Multi-RPC Reliability and Automatic Failover Alpha

This release strengthens the **PlayTradeX SDK for Unity** networking layer with automatic RPC endpoint validation, chain-aware endpoint selection, RPC failover, validation caching, concurrent validation synchronization, and improved transaction RPC consistency.

Building on the multi-network and multi-RPC configuration introduced in `0.2.0-alpha`, this release makes configured RPC endpoints an active reliability mechanism during blockchain operations.

The release continues to target **Unity 6000.3** with native support for **Windows x64** and **Android arm64-v8a**.

### Added

#### RPC Endpoint Validation

- Automatic validation of configured RPC endpoints before use.
- EVM chain ID validation through `eth_chainId`.
- RPC endpoints are checked against the expected chain ID of their configured network.
- Wrong-chain RPC endpoints are automatically rejected.
- Invalid endpoints are skipped when another configured endpoint is available.
- Malformed chain-validation responses are handled without accepting the endpoint.
- RPC validation is centralized through the native RPC execution infrastructure.

#### Automatic RPC Failover

- Automatic failover across multiple RPC endpoints configured for the same network.
- Retryable transport and HTTP failures can move execution to another configured endpoint.
- Temporarily unavailable endpoints can be skipped while alternative endpoints are evaluated.
- Non-retryable failures stop execution instead of unnecessarily retrying other endpoints.
- RPC failover is available without requiring changes to the existing Unity-facing blockchain API.

#### RPC Validation Cache

- Chain-validation results are cached for previously validated RPC endpoints.
- Valid endpoints can be reused without repeating `eth_chainId` validation for every operation.
- Wrong-chain endpoints can be cached as invalid to avoid repeated validation attempts.
- Temporary RPC failures are tracked separately from permanently invalid endpoints.
- Temporary-failure cooldown prevents repeatedly attempting an unavailable endpoint during the cooldown period.
- RPC validation cache is cleared during SDK shutdown.

#### Concurrent RPC Validation

- Synchronization for RPC endpoint validation across concurrent SDK operations.
- Concurrent requests validating the same endpoint can share the validation result.
- Duplicate simultaneous `eth_chainId` validation requests are avoided where possible.
- RPC validation synchronization operates independently from transaction metadata collection concurrency.

#### Centralized RPC Execution

- Added centralized native RPC resolution and execution through `RpcExecutor`.
- RPC resolution considers configured endpoints, expected chain ID, cached validation state, and temporary endpoint availability.
- Added centralized RPC POST execution with endpoint failover handling.
- Blockchain operations can share RPC reliability behavior without duplicating endpoint-selection logic.

---

### Changed

#### Native Balance Queries

- Native currency balance operations now use centralized RPC resolution.
- Balance queries can automatically resolve a valid endpoint from the configured network RPC list.
- Balance queries benefit from chain validation, cached endpoint validation, and RPC failover.

#### Smart Contract Reads

- Smart contract read operations now use centralized RPC resolution.
- Contract reads can use another configured endpoint after an eligible endpoint failure.
- Contract reads benefit from chain-aware endpoint validation.

#### Transaction Metadata Collection

- Transaction metadata RPC requests now use the centralized RPC execution infrastructure.
- Transaction nonce retrieval uses validated RPC execution.
- Gas estimation uses validated RPC execution.
- Maximum priority fee retrieval uses validated RPC execution.
- Latest base fee retrieval uses validated RPC execution.
- Transaction metadata requests continue to execute concurrently while independently benefiting from RPC validation and failover.

#### Transaction Preparation

- Transaction preparation now resolves a validated RPC endpoint from the selected network.
- The resolved RPC endpoint is stored with the prepared transaction.
- Transactions are marked unavailable for submission when no valid RPC endpoint can be resolved.
- Transaction preparation preserves an appropriate preparation error when RPC resolution fails.
- The transaction consent flow remains available even when transaction submission cannot proceed.

#### Transaction Approval and Submission

- Transaction approval preserves the RPC endpoint selected during transaction preparation.
- The SDK does not unnecessarily re-resolve the RPC endpoint after application consent.
- Prepared transactions retain RPC consistency between preparation and submission.

#### Network Reliability

- Multiple RPC endpoints introduced in `0.2.0-alpha` are now actively used as a reliability mechanism.
- RPC endpoint selection is no longer limited to simply using the first configured endpoint.
- Network execution distinguishes valid, wrong-chain, temporarily unavailable, retryable-failure, and non-retryable-failure conditions.
- Different configured networks can execute blockchain operations concurrently using their own endpoint sets.

#### Android Release Builds

- Android Release binaries are stripped of unnecessary debug and symbol information before distribution.
- `libPlayTradeXSDK.so` Release size reduced from approximately **44 MB to 10 MB**.
- Production SDK logging is disabled for the Android Release configuration.
- Debug/development builds can retain debugging information where required.

---

### Fixed

#### RPC Reliability

- Prevented wrong-chain RPC endpoints from being accepted for blockchain execution.
- Improved behavior when the first configured RPC endpoint is unavailable.
- Improved handling of temporary RPC transport failures.
- Prevented unnecessary repeated validation of already validated endpoints.
- Improved behavior when multiple SDK operations attempt RPC validation concurrently.
- Improved recovery when an endpoint temporarily becomes unavailable.

#### Transaction Execution

- Improved RPC consistency between transaction preparation, consent, approval, and submission.
- Prevented transaction preparation from silently relying on an invalid or wrong-chain RPC endpoint.
- Improved failure reporting when no valid RPC endpoint is available for transaction submission.

#### SDK Lifecycle

- RPC validation state is cleared during SDK shutdown.
- Network validation state no longer persists beyond the intended SDK lifecycle.

#### Android Packaging

- Removed unnecessary debug and symbol data from distributed Android Release binaries.
- Reduced Android native SDK package size while preserving Release functionality.

---

### Behavioral Notes

#### RPC Failover

RPC endpoints are evaluated according to their configured network and expected chain ID. If an endpoint experiences an eligible temporary or retryable failure, the SDK can continue with another configured endpoint.

An endpoint reporting a different chain ID is rejected rather than used as a fallback.

#### Validation Caching

Successful chain validation is cached to reduce unnecessary RPC traffic. Invalid and temporarily unavailable endpoints are also tracked so repeated blockchain operations do not continuously perform the same unsuccessful validation work.

The validation cache is scoped to the SDK lifecycle and is cleared during shutdown.

#### Transaction RPC Consistency

Transaction preparation resolves the RPC endpoint associated with the prepared transaction. After application consent, transaction approval continues using that prepared RPC instead of performing an unrelated endpoint selection.

#### Failover vs. Load Balancing

`0.3.0-alpha` introduces **RPC reliability and automatic failover**, not RPC load balancing.

Multiple configured endpoints provide alternatives when endpoints are invalid or unavailable, but requests are not intentionally distributed across healthy RPC endpoints for load balancing.

RPC load balancing is planned for a future SDK release.

---

### API Compatibility

This release does **not require changes to the existing public Unity API** introduced in `0.2.0-alpha`.

The multi-RPC validation and failover behavior is implemented primarily within the native networking infrastructure.

Existing Unity integrations using the `0.2.0-alpha` public API can adopt the updated native binaries without migrating their Unity-facing blockchain calls.

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
```

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