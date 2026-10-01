# Transactions

[← Getting Started](getting-started.md) · [Documentation Home](index.md)
· [Next: Smart Contracts →](contracts.md)

PlayTradeX supports native EVM currency operations and
transaction-producing smart contract operations through its Unity C#
API.

`0.3.0-alpha` adds network-aware execution and support for both the
PlayTradeX identity wallet and configured external wallets.

> **Documentation target:** PlayTradeX `0.3.0-alpha` · Unity `6000.3`\
> PlayTradeX must be initialized before transaction APIs are used.

------------------------------------------------------------------------

## Execution Context

A transaction is no longer tied to one globally configured chain.

Conceptually:

``` text
Transaction
   ├── Network ID
   │      └── Network configuration
   │           ├── Chain ID
   │           └── RPC URL(s)
   │
   └── Signing wallet
          ├── PlayTradeX Identity Wallet
          └── Configured External Wallet
```

Use the generated `GeneratedNetworks` and `GeneratedWallets` identifiers
where appropriate.

The included sample is the compile-ready reference for the exact
`0.3.0-alpha` Unity overloads.

------------------------------------------------------------------------

## Exact Blockchain Units

Blockchain currency values should not be represented with `float` or
`double` when exact precision matters.

### Human-Readable Native Currency to Wei

``` csharp
string amountWei =
    PlayTradeXUnits.ToWei("0.01");
```

Result:

``` text
10000000000000000
```

### Wei to Human-Readable Native Currency

``` csharp
string amount =
    PlayTradeXUnits.FromWei(
        "10000000000000000");
```

Result:

``` text
0.01
```

### Token Base Units

For an 18-decimal token:

``` csharp
string amount =
    PlayTradeXUnits.ToBaseUnit(
        "1.5",
        18);
```

Result:

``` text
1500000000000000000
```

For a 6-decimal token:

``` csharp
string amount =
    PlayTradeXUnits.ToBaseUnit(
        "12.5",
        6);
```

Convert back:

``` csharp
string readable =
    PlayTradeXUnits.FromBaseUnit(
        "12500000",
        6);
```

------------------------------------------------------------------------

## Native Balance

Native balance queries are network-aware in `0.3.0-alpha`.

Select the configured network whose native currency balance should be
queried and, where the API path requires a wallet selection, select the
intended identity/external wallet.

Returned balances use their base-unit representation.

For an 18-decimal native currency:

``` csharp
if (response.Success)
{
    string readable =
        PlayTradeXUnits.FromWei(
            response.Balance);

    Debug.Log("Balance: " + readable);
}
else
{
    Debug.LogError(
        response.ErrorMessage);
}
```

Use the included sample for the exact network/wallet-aware balance call
exposed by the installed package.

------------------------------------------------------------------------

## Send Native Currency

Native transfers create blockchain transactions.

Convert human-readable amounts before submission:

``` csharp
string amountWei =
    PlayTradeXUnits.ToWei("0.01");
```

A transfer selects:

-   Network ID
-   Destination address
-   Exact base-unit amount
-   PlayTradeX identity wallet or configured external wallet

The native SDK constructs and executes the transaction using the
selected execution context.

Keep transaction amounts as exact strings rather than converting through
floating-point types.

------------------------------------------------------------------------

## Identity Wallet vs External Wallet

### Identity Wallet

The PlayTradeX identity wallet is the wallet managed by the native SDK
and its secure-storage lifecycle.

### External Wallet

A configured external wallet is selected through its developer-defined
wallet ID.

The Unity layer resolves that wallet configuration and passes the
appropriate execution context to the native SDK.

Application/gameplay code should prefer wallet IDs rather than directly
handling private keys.

------------------------------------------------------------------------

## Transaction Approval

PlayTradeX supports application-controlled transaction consent.

Before a transaction continues, the application can present information
such as:

-   Contract or destination
-   Function
-   Parameters
-   Value
-   Estimated gas
-   Preparation status
-   Simulation information/errors where available

Approve a pending transaction:

``` csharp
PlayTradeXSdk.ApproveTransaction(
    transactionId);
```

Deny a pending transaction:

``` csharp
PlayTradeXSdk.DenyTransaction(
    transactionId);
```

A preparation or simulation failure is **not** the same as a user
denial.

If preparation fails, show the failure information and keep approval
unavailable rather than reporting that the user denied the transaction.

The included sample demonstrates the transaction-consent flow.

------------------------------------------------------------------------

## Handling Responses

Always inspect the response:

``` csharp
if (response.Success)
{
    // Continue with application-specific success handling.
}
else
{
    Debug.LogError(
        response.ErrorMessage);
}
```

Do not expose private keys, wallet passwords, or other sensitive wallet
data while diagnosing failures.

------------------------------------------------------------------------

## RPC Reliability and Failover

Networks can contain multiple RPC URLs.

In `0.3.0-alpha`, PlayTradeX validates RPC endpoints against the configured network Chain ID before accepting them for execution. An endpoint that reports the wrong chain is rejected.

Eligible transport or HTTP failures can trigger automatic failover to another configured endpoint. Successful validation results are cached, while temporarily unavailable endpoints can be placed into a cooldown state to avoid immediate repeated failures. Concurrent operations also synchronize validation of the same endpoint to reduce duplicate chain-validation requests.

Transaction preparation resolves and retains its selected RPC so that approval/submission continues with the prepared endpoint rather than unnecessarily resolving a different one after user consent.

> RPC failover is a reliability feature. `0.3.0-alpha` does not load-balance requests across healthy RPC endpoints.

RPC endpoints remain external infrastructure and can fail, rate-limit requests, or become unavailable independently of PlayTradeX.

When diagnosing an operation, verify:

-   Correct network ID
-   Correct Chain ID
-   RPC reachability
-   Wallet balance
-   Sufficient native currency for gas
-   Destination/contract address
-   Transaction preparation/simulation errors

------------------------------------------------------------------------

## Transaction Integration Guidelines

-   Wait for successful SDK initialization.
-   Use configured network IDs.
-   Use configured wallet IDs instead of exposing private keys in
    gameplay code.
-   Convert human-readable values with `PlayTradeXUnits`.
-   Avoid `float` and `double` for exact blockchain values.
-   Validate destination addresses and application input.
-   Present meaningful transaction information before approval.
-   Treat preparation/simulation failures separately from user denial.
-   Handle unsuccessful responses explicitly.
-   Use HTTPS RPC endpoints.
-   Never log private keys or wallet passwords.
-   Test with test networks and test assets before production use.

------------------------------------------------------------------------

## Smart Contract Transactions

Contract writes also create transactions.

Continue with [Smart Contracts](contracts.md).

------------------------------------------------------------------------

[← Getting Started](getting-started.md) · [Documentation Home](index.md)
· [Next: Smart Contracts →](contracts.md)
