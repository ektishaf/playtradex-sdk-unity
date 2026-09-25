# Transactions

[← Getting Started](getting-started.md) · [Documentation Home](index.md)
· [Next: Smart Contracts →](contracts.md)

PlayTradeX supports native EVM currency operations and
transaction-producing smart contract operations through its Unity C#
API.

This guide focuses on exact blockchain values, native balances, native
currency transfers, response handling, and application-controlled
transaction approval.

> **Documentation target:** PlayTradeX `0.1.0-alpha` · Unity `6000.3`\
> PlayTradeX must be initialized before using transaction APIs. See
> [Getting Started](getting-started.md).

------------------------------------------------------------------------

## Exact Blockchain Units

Blockchain currency values should not be represented with `float` or
`double` when exact precision matters.

PlayTradeX provides string-based conversion utilities so values can be
converted without introducing floating-point precision loss.

### Human-Readable Native Currency to Wei

``` csharp
string amountWei = PlayTradeXUnits.ToWei("0.01");
```

Result:

``` text
10000000000000000
```

### Wei to Human-Readable Native Currency

``` csharp
string amount = PlayTradeXUnits.FromWei("10000000000000000");
```

Result:

``` text
0.01
```

### Token Base Units

For ERC-20 values, specify the token's decimal precision:

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

Convert a base-unit value back with:

``` csharp
string readable = PlayTradeXUnits.FromBaseUnit("12500000", 6);
```

------------------------------------------------------------------------

## Native Balance

Retrieve the active wallet's native blockchain balance:

``` csharp
NativeBalanceResponse response = await PlayTradeXSdk.GetNativeBalanceAsync();

if (response.Success)
{
    string readable = PlayTradeXUnits.FromWei(response.Balance);
    Debug.Log("Balance: " + readable);
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

The SDK returns blockchain balances in their base-unit representation.
Convert them for display only when appropriate for the configured
network.

------------------------------------------------------------------------

## Send Native Currency

The low-level native transfer API accepts the amount in wei.

``` csharp
string amountWei = PlayTradeXUnits.ToWei("0.01");

TransactionResponse response =
    await PlayTradeXSdk.SendEthAsync(destinationAddress, amountWei);

if (response.Success)
{
    Debug.Log("Transaction submitted successfully.");
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

Advanced integrations may provide the exact wei string directly:

``` csharp
await PlayTradeXSdk.SendEthAsync(
    destinationAddress,
    "10000000000000000");
```

Keep transaction values as exact strings rather than converting through
floating-point types.

------------------------------------------------------------------------

## Transaction Approval

PlayTradeX supports application-controlled transaction consent.

Before a transaction continues, the application can present transaction
information such as:

-   Contract
-   Function
-   Parameters
-   Value
-   Estimated gas

This allows the game's own UI and UX to control the final approval
experience while PlayTradeX handles transaction execution through the
SDK.

The included Unity sample demonstrates transaction consent and response
handling.

------------------------------------------------------------------------

## Handling Responses

Always inspect the response before treating an operation as successful.

``` csharp
if (response.Success)
{
    // Continue with application-specific success handling.
}
else
{
    Debug.LogError(response.ErrorMessage);
}
```

Do not expose sensitive wallet data in logs while diagnosing transaction
failures.

------------------------------------------------------------------------

## Transaction Integration Guidelines

-   Wait for successful PlayTradeX initialization before sending
    transactions.
-   Convert human-readable amounts with `PlayTradeXUnits`.
-   Avoid `float` and `double` for exact blockchain values.
-   Validate destination addresses and application inputs before
    starting a transfer.
-   Present meaningful transaction information before user approval.
-   Handle unsuccessful responses explicitly.
-   Use HTTPS RPC endpoints.
-   Never log private keys or wallet passwords.

------------------------------------------------------------------------

## Smart Contract Transactions

Contract writes also create blockchain transactions. Continue with
[Smart Contracts](contracts.md) for read and write examples.

------------------------------------------------------------------------

[← Getting Started](getting-started.md) · [Documentation Home](index.md)
· [Next: Smart Contracts →](contracts.md)
