# Smart Contracts

[← Transactions](transactions.md) · [Documentation Home](index.md) ·
[Next: Wallet Management →](wallet.md)

PlayTradeX supports read and write operations against EVM-compatible
smart contracts through its Unity C# API.

`0.3.0-alpha` makes contract execution network-aware and allows
transaction-producing writes to use the PlayTradeX identity wallet or a
configured external wallet.

> **Documentation target:** PlayTradeX `0.3.0-alpha` · Unity `6000.3`\
> PlayTradeX must be initialized before contract APIs are used.

------------------------------------------------------------------------

## Contract Reads

Read operations query contract state without submitting a blockchain
transaction.

A read selects:

-   Configured network
-   Contract address
-   Human-readable function ABI/signature
-   Function parameters

A signing wallet is not required for a normal read-only call.

Conceptually:

``` text
Read
  ├── Network ID
  ├── Contract Address
  ├── Function
  └── Parameters
```

Handle the returned response explicitly:

``` csharp
if (response.Success)
{
    Debug.Log(response.Result);
}
else
{
    Debug.LogError(
        response.ErrorMessage);
}
```

Use the included sample for the exact network-aware `ReadAsync`
signature exposed by `0.3.0-alpha`.

------------------------------------------------------------------------

## Contract Writes

Write operations create blockchain transactions.

A write selects:

-   Configured network
-   Signing wallet
-   Contract address
-   Function
-   Parameters
-   Transaction value where applicable

Conceptually:

``` text
Write
  ├── Network ID
  ├── Wallet Source
  │      ├── Identity
  │      └── External Wallet ID
  ├── Contract Address
  ├── Function
  └── Parameters
```

Because writes create transactions, applications should integrate them
with the PlayTradeX transaction approval flow.

See [Transactions → Transaction
Approval](transactions.md#transaction-approval).

Use the included sample for the exact network/wallet-aware `WriteAsync`
overloads exposed by the installed package.

------------------------------------------------------------------------

## Network Selection

Networks are configured under:

`Edit > Project Settings > PlayTradeX`

Generate `GeneratedNetworks.cs` after configuring network IDs.

Application code can then refer to configured networks using generated
constants rather than raw strings.

The selected network determines the EVM chain and RPC configuration used
for the call.

In `0.3.0-alpha`, configured RPC endpoints are validated against that network's Chain ID. Contract reads use the centralized RPC execution path and can fail over to another configured endpoint after eligible failures. Wrong-chain endpoints are rejected.

------------------------------------------------------------------------

## Wallet Selection for Writes

Contract reads do not normally need a signing wallet.

Contract writes do.

PlayTradeX supports:

### PlayTradeX Identity Wallet

Uses the wallet managed by the native PlayTradeX wallet lifecycle.

### Configured External Wallet

Uses an application-managed wallet configured under PlayTradeX Project
Settings and selected through its wallet ID.

Generate `GeneratedWallets.cs` to avoid scattering raw wallet ID strings
throughout application code.

Do not expose the resolved private key through gameplay code or logs.

------------------------------------------------------------------------

## Function Signatures and Parameters

PlayTradeX contract APIs accept contract information including a
contract address, human-readable function ABI/signature, and parameters.

Keep contract integration data centralized rather than scattering raw
function strings throughout gameplay code.

For larger ABIs, use the PlayTradeX ABI Converter.

------------------------------------------------------------------------

## ABI Converter

Open:

`PlayTradeX > ABI Converter`

Paste a standard JSON contract ABI.

The converter can generate a C# contract interface containing
human-readable function information and can optionally embed:

-   Contract ABI
-   Human-readable ABI
-   Contract address

This reduces manual maintenance of Solidity function signatures
throughout a Unity project.

------------------------------------------------------------------------

## Read vs. Write

  -------------------------------------------------------------------------
  Operation         Changes blockchain  Creates transaction Signing wallet
                                 state                      
  --------------- -------------------- -------------------- ---------------
  Contract read                     No                   No No

  Contract write                   Yes                  Yes Yes
  -------------------------------------------------------------------------

Treat write operations as transactions and present appropriate consent
information before approval.

------------------------------------------------------------------------

## Transaction Preparation and Simulation

Transaction-producing operations may provide preparation and simulation
information to the application's transaction-consent flow.

A preparation/simulation failure should be presented as a transaction
preparation failure, not as a user denial.

Approval should only be available when the prepared transaction can be
submitted.

See [Transactions](transactions.md) for transaction-consent guidance.

------------------------------------------------------------------------

## Integration Guidelines

-   Wait for successful SDK initialization before contract calls.
-   Select the intended configured network.
-   Verify the target contract address.
-   Verify the human-readable function ABI/signature.
-   Keep parameters in the format expected by the target function.
-   Handle unsuccessful responses explicitly.
-   Use transaction consent for writes.
-   Select the intended signing wallet for writes.
-   Avoid logging private keys or other sensitive wallet information.
-   Use HTTPS RPC endpoints.
-   Test against the intended network and deployed contract before
    production use.

------------------------------------------------------------------------

## `0.3.0-alpha` Change from the Previous Release

Multi-network and multi-RPC configuration remain part of the current Unity SDK architecture.

`0.3.0-alpha` adds chain-aware RPC validation and automatic failover. Contract reads can use alternative configured endpoints after eligible failures, while transaction-producing writes resolve a validated RPC during preparation and retain that prepared RPC through approval/submission.

RPC load balancing across healthy endpoints is not part of this release.

The broader `1.0.0` roadmap continues toward Unreal Engine support,
additional native platforms, expanded reliability, tooling, and
production readiness.

------------------------------------------------------------------------

[← Transactions](transactions.md) · [Documentation Home](index.md) ·
[Next: Wallet Management →](wallet.md)
