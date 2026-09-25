using System;
using System.Threading.Tasks;
using PlayTradeX;
using PlayTradeX.UI;
using UnityEngine;

/// <summary>
/// Unity-facing component for interacting with the PlayTradeX SDK.
/// </summary>
/// <remarks>
/// This component provides convenient Unity APIs for blockchain
/// operations, tracks SDK readiness, and manages transaction consent
/// UI.
///
/// The PlayTradeX SDK must be initialized before runtime blockchain
/// operations are performed.
///
/// Asynchronous operations return Tasks so calling scripts can await
/// completion and inspect the returned PlayTradeX response.
/// </remarks>
public sealed class PlayTradeXUnity : MonoBehaviour
{
    // ============================================================
    // State
    // ============================================================

    private bool _isReady;
    private PreparedTransaction _pendingTransaction;


    // ============================================================
    // Inspector
    // ============================================================

    [Header("Transaction Consent")]

    [Tooltip(
        "Popup displayed when a blockchain transaction requires " +
        "application approval.")]
    [SerializeField]
    private PlayTradeXTransactionPopup transactionPopup;


    // ============================================================
    // Public State
    // ============================================================

    /// <summary>
    /// Gets whether the PlayTradeX SDK is initialized and ready
    /// to accept runtime requests.
    /// </summary>
    public bool IsReady => _isReady;


    // ============================================================
    // Events
    // ============================================================

    /// <summary>
    /// Raised when PlayTradeX becomes initialized and ready.
    /// </summary>
    public event Action<string> Ready;

    /// <summary>
    /// Raised when PlayTradeX is no longer initialized.
    /// </summary>
    public event Action NotReady;

    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void OnEnable()
    {
        PlayTradeXSdk.InitializationChanged +=
            OnInitializationChanged;

        PlayTradeXLifecycle.Ready +=
       OnLifecycleReady;

        PlayTradeXSdk.SetTransactionConsentCallback(
            OnTransactionConsentRequested);

        SetReadyState(
            PlayTradeXSdk.IsInitialized());
    }

    private void OnDisable()
    {
        PlayTradeXSdk.InitializationChanged -=
            OnInitializationChanged;

        PlayTradeXLifecycle.Ready -=
        OnLifecycleReady;

        PlayTradeXSdk.SetTransactionConsentCallback(null);

        HideTransactionPopup();
    }

    private void OnLifecycleReady(
        string walletAddress)
    {
        _isReady = true;

        Debug.Log(
            "[PlayTradeX Unity] PlayTradeX SDK is ready.\n" +
            $"Wallet Address: {walletAddress}");

        Ready?.Invoke(walletAddress);
    }


    // ============================================================
    // SDK State
    // ============================================================

    private void OnInitializationChanged(
        bool initialized)
    {
        SetReadyState(initialized);
    }

    private void SetReadyState(
    bool initialized)
    {
        if (initialized)
        {
            return;
        }

        if (!_isReady)
        {
            return;
        }

        _isReady = false;

        Debug.Log(
            "[PlayTradeX Unity] PlayTradeX SDK is not ready.");

        NotReady?.Invoke();
    }

    /// <summary>
    /// Verifies that the PlayTradeX SDK is initialized before
    /// performing a runtime operation.
    /// </summary>
    private bool EnsureInitialized()
    {
        if (PlayTradeXSdk.IsInitialized())
        {
            return true;
        }

        Debug.LogWarning(
            "[PlayTradeX Unity] PlayTradeX SDK is not initialized.");

        return false;
    }


    // ============================================================
    // Transaction Consent
    // ============================================================

    private void OnTransactionConsentRequested(
        PreparedTransaction transaction)
    {
        if (transaction == null)
        {
            Debug.LogError(
                "[PlayTradeX Unity] Received an invalid " +
                "transaction consent request.");

            return;
        }

        _pendingTransaction = transaction;

        Debug.Log(
            "[PlayTradeX Unity] Transaction approval requested.\n" +
            $"ID: {transaction.Id}\n" +
            $"Contract: {transaction.ContractAddress}\n" +
            $"Function: {transaction.FunctionName}\n" +
            $"Parameters: {transaction.Params}\n" +
            $"Value: {transaction.Value}");

        ShowTransactionPopup(transaction);
    }

    private void ShowTransactionPopup(
        PreparedTransaction transaction)
    {
        if (transactionPopup == null)
        {
            Debug.LogError(
                "[PlayTradeX Unity] Transaction popup is not assigned.");

            PlayTradeXSdk.DenyTransaction(transaction.Id);

            _pendingTransaction = null;

            return;
        }

        transactionPopup.Show(
            transaction,
            () => ApproveTransaction(transaction.Id),
            () => DenyTransaction(transaction.Id));
    }

    private void ApproveTransaction(
        string transactionId)
    {
        bool approved =
            PlayTradeXSdk.ApproveTransaction(transactionId);

        if (approved)
        {
            Debug.Log(
                "[PlayTradeX Unity] Transaction approved.");
        }
        else
        {
            Debug.LogError(
                "[PlayTradeX Unity] Failed to approve transaction.");
        }

        HideTransactionPopup();
    }

    private void DenyTransaction(
        string transactionId)
    {
        bool denied =
            PlayTradeXSdk.DenyTransaction(transactionId);

        if (denied)
        {
            Debug.Log(
                "[PlayTradeX Unity] Transaction denied.");
        }
        else
        {
            Debug.LogError(
                "[PlayTradeX Unity] Failed to deny transaction.");
        }

        HideTransactionPopup();
    }

    private void HideTransactionPopup()
    {
        if (transactionPopup != null)
        {
            transactionPopup.Hide();
        }

        _pendingTransaction = null;
    }


    // ============================================================
    // Native Balance
    // ============================================================

    /// <summary>
    /// Gets the native blockchain balance of the PlayTradeX wallet.
    /// </summary>
    /// <returns>
    /// A task containing the native balance response.
    /// </returns>
    public async Task<NativeBalanceResponse> GetNativeBalance()
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            NativeBalanceResponse response =
                await PlayTradeXSdk.GetNativeBalanceAsync();

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Native Balance: " +
                    response.Balance);
            }
            else
            {
                LogError(
                    "Get Native Balance",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    // ============================================================
    // Read Contract
    // ============================================================

    /// <summary>
    /// Calls a read-only smart contract function.
    /// </summary>
    /// <remarks>
    /// The ABI must contain a human-readable function signature.
    ///
    /// Example:
    /// function totalSupply() view returns (uint256)
    /// </remarks>
    /// <param name="contractAddress">
    /// Address of the smart contract.
    /// </param>
    /// <param name="abi">
    /// Human-readable function ABI.
    /// </param>
    /// <param name="parameters">
    /// JSON array containing the function parameters.
    /// </param>
    /// <returns>
    /// A task containing the contract read response.
    /// </returns>
    public async Task<ContractReadResponse> Read(
        string contractAddress,
        string abi,
        string parameters = "[]")
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            string functionName =
                ExtractFunctionName(abi);

            ContractReadResponse response =
                await PlayTradeXSdk.ReadAsync(
                    contractAddress,
                    abi,
                    functionName,
                    parameters);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Read Result: " +
                    response.Data);
            }
            else
            {
                LogError(
                    "Read",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    // ============================================================
    // Send Native Currency
    // ============================================================

    /// <summary>
    /// Sends native blockchain currency to another address.
    /// </summary>
    /// <remarks>
    /// This creates a blockchain transaction and may require
    /// transaction consent before submission.
    /// </remarks>
    /// <param name="to">
    /// Destination blockchain address.
    /// </param>
    /// <param name="amount">
    /// Amount of native currency to send.
    /// </param>
    /// <returns>
    /// A task containing the transaction response.
    /// </returns>
    public async Task<TransactionResponse> SendEth(
        string to,
        string amount)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            TransactionResponse response =
                await PlayTradeXSdk.SendEthAsync(
                    to,
                    amount);

            if (response.Success)
            {
                LogTransaction(
                    "Send ETH",
                    response);
            }
            else
            {
                LogError(
                    "Send ETH",
                    response);
                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    // ============================================================
    // Write Contract
    // ============================================================

    /// <summary>
    /// Calls a state-changing smart contract function.
    /// </summary>
    /// <remarks>
    /// The ABI must contain a human-readable function signature.
    ///
    /// Example:
    /// function transfer(address to, uint256 amount) returns (bool)
    ///
    /// This creates a blockchain transaction and may require
    /// transaction consent before submission.
    /// </remarks>
    /// <param name="contractAddress">
    /// Address of the smart contract.
    /// </param>
    /// <param name="abi">
    /// Human-readable function ABI.
    /// </param>
    /// <param name="parameters">
    /// JSON array containing the function parameters.
    /// </param>
    /// <param name="value">
    /// Native currency value attached to the transaction.
    /// </param>
    /// <returns>
    /// A task containing the transaction response.
    /// </returns>
    public async Task<TransactionResponse> Write(
        string contractAddress,
        string abi,
        string parameters = "[]",
        string value = "0")
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            string functionName =
                ExtractFunctionName(abi);

            TransactionResponse response =
                await PlayTradeXSdk.WriteAsync(
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    value);

            if (response.Success)
            {
                LogTransaction(
                    "Write",
                    response);
            }
            else
            {
                LogError(
                    "Write",
                    response);

                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
        finally
        {
            HideTransactionPopup();
        }
    }

    // ============================================================
    // Wallet Export
    // ============================================================

    /// <summary>
    /// Exports the current PlayTradeX wallet to an encrypted
    /// wallet file.
    /// </summary>
    /// <param name="password">
    /// Password used to encrypt the exported wallet.
    /// </param>
    /// <param name="outputPath">
    /// Destination file path or supported platform URI.
    /// </param>
    /// <returns>
    /// A task containing the wallet export response.
    /// </returns>
    public async Task<WalletExportResponse> ExportWallet(
        string password,
        string outputPath)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            WalletExportResponse response =
                await PlayTradeXSdk.ExportWalletAsync(
                    password,
                    outputPath);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Wallet exported successfully.\n" +
                    $"Path: {outputPath}");
            }
            else
            {
                LogError(
                    "Export Wallet",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    // ============================================================
    // Wallet Import
    // ============================================================

    /// <summary>
    /// Imports an encrypted PlayTradeX wallet file.
    /// </summary>
    /// <remarks>
    /// For the Alpha SDK, PlayTradeX must already be initialized
    /// before importing a wallet.
    ///
    /// After a successful import, the application must be restarted
    /// before continuing normal SDK operations.
    /// </remarks>
    /// <param name="password">
    /// Password used to decrypt the wallet.
    /// </param>
    /// <param name="inputPath">
    /// Wallet file path or supported platform URI.
    /// </param>
    /// <returns>
    /// The wallet import response.
    /// </returns>
    public async Task<WalletImportResponse> ImportWallet(
        string password,
        string inputPath)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            WalletImportResponse response =
    await PlayTradeXSdk.ImportWalletAsync(
        password,
        inputPath);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Wallet imported successfully.\n" +
                    $"Wallet Address: {response.WalletAddress}\n" +
                    "Restart the application before continuing.");
            }
            else
            {
                LogError(
                    "Import Wallet",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    // ============================================================
    // Function Signature
    // ============================================================

    /// <summary>
    /// Extracts the function name from a human-readable Solidity
    /// function signature.
    /// </summary>
    /// <param name="functionSignature">
    /// Human-readable Solidity function signature.
    /// </param>
    /// <returns>
    /// The extracted function name.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the function signature is null, empty, or invalid.
    /// </exception>
    public static string ExtractFunctionName(
        string functionSignature)
    {
        if (string.IsNullOrWhiteSpace(functionSignature))
        {
            throw new ArgumentException(
                "Function signature cannot be null or empty.",
                nameof(functionSignature));
        }

        string signature =
            functionSignature.Trim();

        signature =
            signature.Trim('[', ']', '"').Trim();

        if (signature.StartsWith(
            "function ",
            StringComparison.Ordinal))
        {
            signature =
                signature.Substring("function ".Length).TrimStart();
        }

        int parenthesisIndex =
            signature.IndexOf('(');

        if (parenthesisIndex <= 0)
        {
            throw new ArgumentException(
                "Invalid human-readable function signature.",
                nameof(functionSignature));
        }

        string functionName =
            signature.Substring(
                0,
                parenthesisIndex).Trim();

        if (string.IsNullOrEmpty(functionName))
        {
            throw new ArgumentException(
                "Function name could not be extracted.",
                nameof(functionSignature));
        }

        return functionName;
    }


    // ============================================================
    // Logging
    // ============================================================

    private static void LogTransaction(
        string operation,
        TransactionResponse response)
    {
        Debug.Log(
            $"[PlayTradeX Unity] {operation} succeeded.\n" +
            $"Transaction Hash: {response.TransactionHash}\n" +
            $"Receipt: {response.Receipt}");
    }

    private static void LogError(
        string operation,
        BaseResponse response)
    {
        Debug.LogError(
            $"[PlayTradeX Unity] {operation} failed.\n" +
            $"Error Code: {response.ErrorCode}\n" +
            $"Message: {response.ErrorMessage}");
    }
}