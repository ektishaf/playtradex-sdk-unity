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
/// operations, tracks SDK readiness, and manages transaction consent UI.
///
/// PlayTradeX supports multiple configured blockchain networks.
/// Every blockchain operation therefore explicitly receives a network ID.
///
/// Transaction-signing operations can use either:
///
/// - The PlayTradeX identity wallet managed by the SDK.
/// - An application-managed external wallet supplied by private key.
///
/// Read-only operations do not require a private key.
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

    private string _walletAddress = string.Empty;


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


    /// <summary>
    /// Gets the PlayTradeX identity wallet address reported when
    /// the SDK becomes ready.
    /// </summary>
    public string WalletAddress => _walletAddress;


    /// <summary>
    /// Gets the transaction currently awaiting application consent.
    /// </summary>
    public PreparedTransaction PendingTransaction =>
        _pendingTransaction;


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

    /// <summary>
    /// Raised when a transaction submitted through PlayTradeX
    /// reaches a terminal mined state.
    ///
    /// The event is raised for both confirmed and reverted
    /// transactions and includes the blockchain receipt.
    /// </summary>
    public event Action<TransactionEvent> TransactionMined;

    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void OnEnable()
    {
        PlayTradeXSdk.InitializationChanged +=
            OnInitializationChanged;

        PlayTradeXSdk.TransactionMined +=
            OnTransactionMined;

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

        PlayTradeXSdk.TransactionMined -=
            OnTransactionMined;

        PlayTradeXLifecycle.Ready -=
            OnLifecycleReady;

        /*
         * Only unregister the consent callback if this component owns
         * the application's consent UI.
         */
        PlayTradeXSdk.SetTransactionConsentCallback(
            null);

        HideTransactionPopup();
    }


    // ============================================================
    // Lifecycle Ready
    // ============================================================

    private void OnLifecycleReady(
        string walletAddress)
    {
        _walletAddress =
            walletAddress ?? string.Empty;

        bool wasReady =
            _isReady;

        _isReady =
            true;

        Debug.Log(
            "[PlayTradeX Unity] PlayTradeX SDK is ready.\n" +
            $"Wallet Address: {_walletAddress}");

        /*
         * LifecycleReady contains the identity wallet address, so it
         * remains the authoritative ready notification exposed by this
         * component.
         */
        Ready?.Invoke(
            _walletAddress);

        if (!wasReady)
        {
            return;
        }
    }


    // ============================================================
    // SDK State
    // ============================================================

    private void OnInitializationChanged(
        bool initialized)
    {
        SetReadyState(
            initialized);
    }


    private void SetReadyState(
        bool initialized)
    {
        if (initialized)
        {
            _isReady =
                true;

            return;
        }

        if (!_isReady)
        {
            return;
        }

        _isReady =
            false;

        _walletAddress =
            string.Empty;

        HideTransactionPopup();

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


    /// <summary>
    /// Validates a configured network identifier before forwarding
    /// an operation to the SDK.
    /// </summary>
    private static void ValidateNetworkId(
        string networkId)
    {
        if (string.IsNullOrWhiteSpace(
                networkId))
        {
            throw new ArgumentException(
                "Network ID cannot be null or empty.",
                nameof(networkId));
        }
    }


    /// <summary>
    /// Validates an application-managed external private key.
    /// </summary>
    private static void ValidatePrivateKey(
        string privateKey)
    {
        if (string.IsNullOrWhiteSpace(
                privateKey))
        {
            throw new ArgumentException(
                "External wallet private key cannot be null or empty.",
                nameof(privateKey));
        }
    }

    // ============================================================
    // Transaction Events
    // ============================================================

    private void OnTransactionMined(
        TransactionEvent transactionEvent)
    {
        if (transactionEvent == null)
        {
            Debug.LogError(
                "[PlayTradeX Unity] Received null transaction mined event.");

            return;
        }


        Debug.Log(
            "[PlayTradeX Unity] Transaction mined.\n" +
            $"Transaction ID: {transactionEvent.TransactionId}\n" +
            $"Network: {transactionEvent.NetworkId}\n" +
            $"Chain ID: {transactionEvent.ChainId}\n" +
            $"Transaction Hash: {transactionEvent.TransactionHash}\n" +
            $"Status: {transactionEvent.Status}\n" +
            $"Receipt: {transactionEvent.Receipt}");


        TransactionMined?.Invoke(
            transactionEvent);
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
            "[PlayTradeX Unity] Received null transaction consent request.");

        return;
    }


        // ============================================================
        // Diagnostics
        // ============================================================

        if (!transaction.CanSubmit)
        {
            Debug.LogError(
                "[PlayTradeX Unity] Prepared transaction cannot be submitted.\n" +
                $"Reason: {transaction.PreparationError}\n" +
                $"Simulation: {transaction.SimulationError}");
        }


        // ============================================================
        // Show consent / preparation result
        //
        // Even when CanSubmit == false, show the transaction so the
        // developer/user can see why submission is unavailable.
        //
        // The popup itself disables the Approve button when the
        // transaction cannot be submitted.
        // ============================================================

        /*transactionPopup.Show(
            transaction,

            () =>
            {
                if (!transaction.CanSubmit)
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] Attempted to approve a transaction " +
                        "that cannot be submitted.");

                    return;
                }


                PlayTradeXSdk.ApproveTransaction(
                    transaction.Id);
            },

            () =>
            {
                PlayTradeXSdk.DenyTransaction(
                    transaction.Id);
            });*/
        ShowTransactionPopup(transaction);
}


    private void ShowTransactionPopup(
        PreparedTransaction transaction)
    {
        if (transactionPopup == null)
        {
            Debug.LogError(
                "[PlayTradeX Unity] Transaction popup is not assigned.");

            PlayTradeXSdk.DenyTransaction(
                transaction.Id);

            _pendingTransaction =
                null;

            return;
        }
        
        // ============================================================
        // Show consent / preparation result
        //
        // Even when CanSubmit == false, show the transaction so the
        // developer/user can see why submission is unavailable.
        //
        // The popup itself disables the Approve button when the
        // transaction cannot be submitted.
        // ============================================================
        transactionPopup.Show(
            transaction,
            () => {
                if (!transaction.CanSubmit)
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] Attempted to approve a transaction " +
                        "that cannot be submitted.");

                    return;
                }

                ApproveTransaction(
                    transaction.Id);
            },
            () => DenyTransaction(transaction.Id));
    }


    private void ApproveTransaction(
        string transactionId)
    {
        bool approved =
            PlayTradeXSdk.ApproveTransaction(
                transactionId);

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
            PlayTradeXSdk.DenyTransaction(
                transactionId);

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

        _pendingTransaction =
            null;
    }


    // ============================================================
    // Standalone Wallet Generation
    // ============================================================

    /// <summary>
    /// Generates a new standalone EVM wallet.
    /// </summary>
    /// <remarks>
    /// The generated wallet is application-managed and is not stored
    /// as the PlayTradeX identity wallet.
    ///
    /// The returned private key is sensitive wallet material.
    /// Applications are responsible for protecting it.
    /// </remarks>
    public async Task<GenerateWalletResponse> GenerateWallet(
        bool sequential = false)
    {
        try
        {
            GenerateWalletResponse response =
                await PlayTradeXSdk.GenerateWalletAsync(
                    sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Standalone wallet generated.\n" +
                    $"Address: {response.Wallet.Address}");
            }
            else
            {
                Debug.LogError(
                    "[PlayTradeX Unity] Generate Wallet failed.\n" +
                    $"Error Code: {response.ErrorCode}\n" +
                    $"Message: {response.ErrorMessage}");
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Native Balance - Identity Wallet
    // ============================================================

    /// <summary>
    /// Gets the native blockchain balance of the PlayTradeX identity
    /// wallet on the selected network.
    /// </summary>
    /// <param name="networkId">
    /// ID of a network configured during SDK initialization.
    /// </param>
    /// <param name="sequential">
    /// When true, queues the request for sequential execution.
    /// </param>
    public async Task<NativeBalanceResponse> GetNativeBalance(
        string networkId,
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        try
        {
            NativeBalanceResponse response =
                await PlayTradeXSdk.GetNativeBalanceAsync(
                    networkId,
                    sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Native Balance.\n" +
                    $"Network: {networkId}\n" +
                    $"Address: {_walletAddress}\n" +
                    $"Balance: {response.Balance}");
            }
            else
            {
                LogError(
                    $"Get Native Balance [{networkId}]",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Native Balance - Arbitrary Address
    // ============================================================

    /// <summary>
    /// Gets the native blockchain balance of an arbitrary EVM address.
    /// </summary>
    /// <remarks>
    /// This is a read-only operation and therefore does not require
    /// the private key of the supplied address.
    /// </remarks>
    public async Task<NativeBalanceResponse>
        GetNativeBalanceForAddress(
            string networkId,
            string address,
            bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        if (string.IsNullOrWhiteSpace(
                address))
        {
            throw new ArgumentException(
                "Wallet address cannot be null or empty.",
                nameof(address));
        }

        try
        {
            NativeBalanceResponse response =
                await PlayTradeXSdk
                    .GetNativeBalanceForAddressAsync(
                        networkId,
                        address,
                        sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Address Native Balance.\n" +
                    $"Network: {networkId}\n" +
                    $"Address: {address}\n" +
                    $"Balance: {response.Balance}");
            }
            else
            {
                LogError(
                    $"Get Address Native Balance [{networkId}]",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Read Contract
    // ============================================================

    /// <summary>
    /// Calls a read-only smart contract function on the selected
    /// blockchain network.
    /// </summary>
    /// <remarks>
    /// The ABI must contain a human-readable function signature.
    ///
    /// Example:
    ///
    /// function totalSupply() view returns (uint256)
    ///
    /// Read operations do not require transaction signing and therefore
    /// do not require either the identity wallet or an external wallet.
    /// </remarks>
    /// <param name="networkId">
    /// ID of a network configured during SDK initialization.
    /// </param>
    /// <param name="contractAddress">
    /// Address of the smart contract.
    /// </param>
    /// <param name="abi">
    /// Human-readable function ABI.
    /// </param>
    /// <param name="parameters">
    /// JSON array containing the function parameters.
    /// </param>
    /// <param name="sequential">
    /// When true, queues the request for sequential execution.
    /// </param>
    public async Task<ContractReadResponse> Read(
        string networkId,
        string contractAddress,
        string abi,
        string parameters = "[]",
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        try
        {
            string functionName =
                ExtractFunctionName(
                    abi);

            ContractReadResponse response =
                await PlayTradeXSdk.ReadAsync(
                    networkId,
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Read succeeded.\n" +
                    $"Network: {networkId}\n" +
                    $"Contract: {contractAddress}\n" +
                    $"Function: {functionName}\n" +
                    $"Result: {response.Data}");
            }
            else
            {
                LogError(
                    $"Read [{networkId}]",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Send Native Currency - Identity Wallet
    // ============================================================

    /// <summary>
    /// Sends native blockchain currency using the SDK-managed
    /// PlayTradeX identity wallet.
    /// </summary>
    /// <remarks>
    /// This creates a blockchain transaction and may require
    /// transaction consent before submission.
    /// </remarks>
    public async Task<TransactionResponse> SendEth(
        string networkId,
        string to,
        string amount,
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        try
        {
            TransactionResponse response =
                await PlayTradeXSdk.SendEthAsync(
                    networkId,
                    to,
                    amount,
                    sequential);

            if (response.Success)
            {
                LogTransaction(
                    $"Send Native Currency [{networkId}]",
                    response);
            }
            else
            {
                LogError(
                    $"Send Native Currency [{networkId}]",
                    response);

                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Send Native Currency - External Wallet
    // ============================================================

    /// <summary>
    /// Sends native blockchain currency using an application-managed
    /// external wallet.
    /// </summary>
    /// <remarks>
    /// The supplied private key is used only for this operation and is
    /// not stored as the PlayTradeX identity wallet.
    ///
    /// Never log or display the private key.
    /// </remarks>
    public async Task<TransactionResponse> SendEthWithWallet(
        string networkId,
        string privateKey,
        string to,
        string amount,
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        ValidatePrivateKey(
            privateKey);

        try
        {
            TransactionResponse response =
                await PlayTradeXSdk.SendEthWithWalletAsync(
                    networkId,
                    privateKey,
                    to,
                    amount,
                    sequential);

            if (response.Success)
            {
                LogTransaction(
                    $"Send Native Currency - External Wallet [{networkId}]",
                    response);
            }
            else
            {
                LogError(
                    $"Send Native Currency - External Wallet [{networkId}]",
                    response);

                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Write Contract - Identity Wallet
    // ============================================================

    /// <summary>
    /// Calls a state-changing smart contract function using the
    /// SDK-managed PlayTradeX identity wallet.
    /// </summary>
    /// <remarks>
    /// The ABI must contain a human-readable function signature.
    ///
    /// Example:
    ///
    /// function transfer(address to, uint256 amount) returns (bool)
    ///
    /// This creates a blockchain transaction and may require
    /// transaction consent before submission.
    /// </remarks>
    public async Task<TransactionResponse> Write(
        string networkId,
        string contractAddress,
        string abi,
        string parameters = "[]",
        string value = "0",
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        try
        {
            string functionName =
                ExtractFunctionName(
                    abi);

            TransactionResponse response =
                await PlayTradeXSdk.WriteAsync(
                    networkId,
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    value,
                    sequential);

            if (response.Success)
            {
                LogTransaction(
                    $"Write [{networkId}]",
                    response);
            }
            else
            {
                LogError(
                    $"Write [{networkId}]",
                    response);

                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
        finally
        {
            HideTransactionPopup();
        }
    }


    // ============================================================
    // Write Contract - External Wallet
    // ============================================================

    /// <summary>
    /// Calls a state-changing smart contract function using an
    /// application-managed external wallet.
    /// </summary>
    /// <remarks>
    /// The private key is supplied only for this execution.
    /// PlayTradeX does not persist it as the identity wallet.
    ///
    /// This transaction may require application consent before
    /// submission.
    /// </remarks>
    public async Task<TransactionResponse> WriteWithWallet(
        string networkId,
        string privateKey,
        string contractAddress,
        string abi,
        string parameters = "[]",
        string value = "0",
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        ValidateNetworkId(
            networkId);

        ValidatePrivateKey(
            privateKey);

        try
        {
            string functionName =
                ExtractFunctionName(
                    abi);

            TransactionResponse response =
                await PlayTradeXSdk.WriteWithWalletAsync(
                    networkId,
                    privateKey,
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    value,
                    sequential);

            if (response.Success)
            {
                LogTransaction(
                    $"Write - External Wallet [{networkId}]",
                    response);
            }
            else
            {
                LogError(
                    $"Write - External Wallet [{networkId}]",
                    response);

                HideTransactionPopup();
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
        finally
        {
            HideTransactionPopup();
        }
    }


    // ============================================================
    // Human-Readable ABI
    // ============================================================

    /// <summary>
    /// Converts a contract ABI into the human-readable ABI format
    /// supported by PlayTradeX.
    /// </summary>
    public async Task<HumanReadableAbiResponse> HumanReadableAbi(
        string abi,
        bool minimal = false,
        bool sequential = false)
    {
        if (!EnsureInitialized())
        {
            return null;
        }

        try
        {
            HumanReadableAbiResponse response =
                await PlayTradeXSdk.HumanReadableAbiAsync(
                    abi,
                    minimal,
                    sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Human-readable ABI generated.\n" +
                    response.Abi);
            }
            else
            {
                LogError(
                    "Human Readable ABI",
                    response);
            }

            return response;
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Wallet Export
    // ============================================================

    /// <summary>
    /// Exports the current PlayTradeX identity wallet to an encrypted
    /// wallet file.
    /// </summary>
    /// <remarks>
    /// This exports only the SDK-managed identity wallet.
    /// Application-managed standalone/external wallets remain the
    /// application's responsibility.
    /// </remarks>
    public async Task<WalletExportResponse> ExportWallet(
        string password,
        string outputPath,
        bool sequential = false)
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
                    outputPath,
                    sequential);

            if (response.Success)
            {
                Debug.Log(
                    "[PlayTradeX Unity] Wallet exported successfully.\n" +
                    $"Wallet Address: {response.WalletAddress}\n" +
                    $"Path: {response.FilePath}");
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
            Debug.LogException(
                exception);

            throw;
        }
    }


    // ============================================================
    // Wallet Import
    // ============================================================

    /// <summary>
    /// Imports an encrypted PlayTradeX identity-wallet backup.
    /// </summary>
    /// <remarks>
    /// For the Alpha SDK, PlayTradeX must already be initialized
    /// before importing a wallet.
    ///
    /// After a successful import, the application should restart
    /// before continuing normal SDK operations.
    /// </remarks>
    public async Task<WalletImportResponse> ImportWallet(
        string password,
        string inputPath,
        bool sequential = false)
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
                    inputPath,
                    sequential);

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
            Debug.LogException(
                exception);

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
        if (string.IsNullOrWhiteSpace(
                functionSignature))
        {
            throw new ArgumentException(
                "Function signature cannot be null or empty.",
                nameof(functionSignature));
        }

        string signature =
            functionSignature.Trim();

        signature =
            signature
                .Trim('[', ']', '"')
                .Trim();

        if (signature.StartsWith(
                "function ",
                StringComparison.Ordinal))
        {
            signature =
                signature
                    .Substring(
                        "function ".Length)
                    .TrimStart();
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
            signature
                .Substring(
                    0,
                    parenthesisIndex)
                .Trim();

        if (string.IsNullOrEmpty(
                functionName))
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
        if (response == null)
        {
            Debug.LogError(
                $"[PlayTradeX Unity] {operation} failed.\n" +
                "No response was returned.");

            return;
        }

        Debug.LogError(
            $"[PlayTradeX Unity] {operation} failed.\n" +
            $"Error Code: {response.ErrorCode}\n" +
            $"Message: {response.ErrorMessage}\n" +
            $"Body: {response.Body}");
    }
}