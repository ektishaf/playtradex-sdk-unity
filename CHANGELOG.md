# Changelog

All notable changes to the **PlayTradeX SDK for Unity** are documented in this file.

The format of this changelog is intended to provide developers with a clear history of public SDK releases, supported platforms, major capabilities, behavioral changes, and known limitations.

---

## [Unreleased]

Development toward the stable `1.0.0` release is ongoing.

### Planned for 1.0.0

The following capabilities are currently planned for the December `1.0.0` release:

- Unreal Engine support with an included sample.
- Continued Unity support with an included sample.
- Multi-network EVM support.
- Support for executing calls across multiple contracts and configured networks.
- macOS support.
- iOS support.
- Expanded SDK tooling and documentation.
- Continued wallet lifecycle improvements.
- Continued platform integration and production-readiness work.

> Planned features and platform targets may change before the stable `1.0.0` release.

---

## [0.1.0-alpha] - 2026-09-20

### Initial Alpha Release

The first public Alpha release of the **PlayTradeX SDK for Unity**.

This release establishes the initial PlayTradeX Unity integration and native SDK foundation for EVM-compatible blockchain functionality.

The Alpha release targets **Unity 6000.3** and introduces native support for **Windows x64** and **Android arm64-v8a**.

### Added

#### Unity Integration

- Unity C# API for accessing PlayTradeX functionality.
- `PlayTradeXLifecycle` for SDK lifecycle and initialization management.
- `PlayTradeXUnity.Ready` event for initialization-driven application flow.
- SDK initialization-state access through `PlayTradeXLifecycle.IsInitialized`.
- Unity main-thread callback dispatch.
- Native callback lifetime management.
- Unity sample demonstrating common PlayTradeX integration workflows.

#### Native Platform Integration

- Windows x64 native PlayTradeX integration.
- Android arm64-v8a native PlayTradeX integration.
- Automatic Android platform initialization.
- Native library integration for supported Unity targets.
- HTTPS/TLS certificate verification.

#### Wallet

- Native wallet creation.
- Existing wallet loading during SDK initialization.
- Platform-specific secure wallet storage.
- Android Keystore-backed secure storage.
- Encrypted wallet export.
- Synchronous encrypted wallet import.
- Android Storage Access Framework integration for wallet backup files.
- Android `content://` URI support for wallet export and import.

#### Blockchain

- Native EVM blockchain integration.
- Native blockchain balance queries.
- Native currency transactions.
- Asynchronous blockchain operations.
- Transaction response handling.

#### Smart Contracts

- Read-only smart contract operations.
- Transaction-producing smart contract write operations.
- Human-readable ABI conversion.
- Unity ABI Converter Editor utility.
- Application-controlled transaction consent and approval flow.

#### Blockchain Units

- Exact string-based blockchain unit conversion.
- Human-readable native currency to wei conversion.
- Wei to human-readable native currency conversion.
- Token base-unit conversion using configurable token decimals.
- Conversion utilities designed to avoid floating-point precision loss.

#### Developer Experience

- Unity sample scene and example scripts.
- Unity Editor ABI tooling.
- Consistent managed response models for SDK operations.
- Main-thread delivery of Unity-facing asynchronous callbacks.
- Documentation for installation, initialization, transactions, smart contracts, and wallet management.

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