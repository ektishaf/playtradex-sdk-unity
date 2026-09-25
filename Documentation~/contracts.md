# Smart Contracts

[← Transactions](transactions.md) · [Documentation Home](index.md) ·
[Next: Wallet Management →](wallet.md)

PlayTradeX supports read and write operations against EVM-compatible
smart contracts through its Unity C# API.

> **Documentation target:** PlayTradeX `0.1.0-alpha` · Unity `6000.3`\
> PlayTradeX must be initialized before using contract APIs. See
> [Getting Started](getting-started.md).

------------------------------------------------------------------------

## Contract Reads

Read operations query contract state without submitting a blockchain
transaction.

``` csharp
ContractReadResponse response =
    await PlayTradeXSdk.ReadAsync(
        contractAddress,
        functionSignature,
        parameters);

if (response.Success)
{
    Debug.Log(response.Result);
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

Use reads for operations that inspect blockchain state without changing
contract state.

------------------------------------------------------------------------

## Contract Writes

Write operations create blockchain transactions.

``` csharp
TransactionResponse response =
    await PlayTradeXSdk.WriteAsync(
        contractAddress,
        functionSignature,
        parameters);

if (response.Success)
{
    Debug.Log("Contract transaction submitted.");
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

Because writes produce transactions, applications should integrate them
with the PlayTradeX transaction approval flow.

See [Transactions → Transaction
Approval](transactions.md#transaction-approval).

------------------------------------------------------------------------

## Function Signatures and Parameters

PlayTradeX contract APIs accept a contract address, function signature,
and parameters.

Keep contract integration data centralized in your application rather
than scattering raw function signature strings throughout gameplay code.

For projects with larger ABIs, use the PlayTradeX ABI Converter
described below.

------------------------------------------------------------------------

## ABI Converter

PlayTradeX includes an ABI Converter inside the Unity Editor.

Open:

`PlayTradeX > ABI Converter`

Paste a standard JSON contract ABI.

The converter can generate a C# contract interface containing
human-readable function signatures and can optionally embed:

-   Contract ABI
-   Human-readable ABI
-   Contract address

This helps reduce manual maintenance of Solidity function signature
strings across a Unity project.

------------------------------------------------------------------------

## Read vs. Write

  Operation          Changes blockchain state   Creates transaction
  ---------------- -------------------------- ---------------------
  Contract read                            No                    No
  Contract write                          Yes                   Yes

Treat write operations as transactions and present appropriate consent
information to the player before approval.

------------------------------------------------------------------------

## Integration Guidelines

-   Wait for successful SDK initialization before contract calls.
-   Verify the target contract address and function signature.
-   Keep contract parameters in the format expected by the target
    function.
-   Handle unsuccessful responses explicitly.
-   Use transaction consent for writes.
-   Avoid logging sensitive wallet information.
-   Test contract interactions against the intended network and deployed
    contract before production use.

------------------------------------------------------------------------

## Coming in `1.0.0`

The planned December `1.0.0` release includes multi-network support,
with the goal of allowing developers to work with multiple configured
EVM networks and call multiple functions across multiple contracts and
networks through PlayTradeX.

This is a roadmap target and is not presented as part of the current
`0.1.0-alpha` API.

------------------------------------------------------------------------

[← Transactions](transactions.md) · [Documentation Home](index.md) ·
[Next: Wallet Management →](wallet.md)
