using AOT;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayTradeX
{
    // ============================================================
    // PlayTradeX SDK
    // ============================================================

    /// <summary>
    /// Main managed API for the PlayTradeX SDK.
    /// </summary>
    /// <remarks>
    /// Provides Task-based asynchronous APIs for blockchain
    /// and wallet operations.
    ///
    /// Native callbacks are dispatched to Unity's main thread before
    /// invoking application-facing callbacks or completing managed tasks.
    /// </remarks>
    public static class PlayTradeXSdk
    {
        /// <summary>
        /// Raised when the SDK initialization state changes.
        /// </summary>
        public static event Action<bool> InitializationChanged;

        private static readonly CPlayTradeXNative.LogCallback
    NativeLogCallback = OnNativeLog;

        private static readonly CPlayTradeXNative.TransactionConsentCallback
            NativeTransactionConsentCallback = OnNativeTransactionConsent;

        private static readonly CPlayTradeXNative.InitializeCallback
            NativeInitializeCallback = OnNativeInitialize;

        private static readonly CPlayTradeXNative.NativeBalanceCallback
            NativeBalanceCallback = OnNativeBalance;

        private static readonly CPlayTradeXNative.TransactionCallback
            NativeTransactionCallback = OnNativeTransaction;

        private static readonly CPlayTradeXNative.HumanReadableAbiCallback
            NativeHumanReadableAbiCallback = OnNativeHumanReadableAbi;

        private static readonly CPlayTradeXNative.ContractReadCallback
            NativeContractReadCallback = OnNativeContractRead;

        private static readonly CPlayTradeXNative.WalletExportCallback
            NativeWalletExportCallback = OnNativeWalletExport;

        private static readonly CPlayTradeXNative.WalletImportCallback
            NativeWalletImportCallback = OnNativeWalletImport;
        private static Action<PreparedTransaction> _transactionConsentCallback;

        private static readonly object UnityContextLock = new object();

        private static SynchronizationContext _unitySynchronizationContext;
        private static int _unityThreadId;
        private static bool _loggingRegistered;


        // ========================================================
        // Logging
        // ========================================================

        /// <summary>
        /// Registers PlayTradeX native logging with the Unity Console.
        /// </summary>
        /// <remarks>
        /// This should be called from Unity's main thread before using
        /// the SDK. PlayTradeXLifecycle performs this automatically.
        /// </remarks>
        public static void SetupLogging()
        {
            CaptureUnityContext();

            if (_loggingRegistered)
            {
                return;
            }

            CPlayTradeXNative.SetLogCallback(NativeLogCallback, IntPtr.Zero);

            _loggingRegistered = true;
        }

        [MonoPInvokeCallback(typeof(CPlayTradeXNative.LogCallback))]
        private static void OnNativeLog(IntPtr message, int type, IntPtr userData)
        {
            // Native char* is guaranteed only for the duration
            // of this callback. Copy it immediately.
            string text = CPlayTradeXNative.GetString(message);

            RunOnUnityThread(() =>
            {
                switch (type)
                {
                    case 1:
                        Debug.LogWarning(
                            $"[PlayTradeX Unity] {text}");
                        break;

                    case -1:
                        Debug.LogError(
                            $"[PlayTradeX Unity] {text}");
                        break;

                    default:
                        Debug.Log(
                            $"[PlayTradeX Unity] {text}");
                        break;
                }
            });
        }


        // ========================================================
        // Initialize
        // ========================================================

        /// <summary>
        /// Starts native PlayTradeX SDK initialization.
        /// </summary>
        private static void Initialize(
            string storagePath,
            string rpc,
            long chainId,
            Action<InitializeResult> callback,
            Action<Exception> errorCallback)
        {
            CaptureUnityContext();
            SetupLogging();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative.CPlayTradeX_Initialize(
                    storagePath ?? string.Empty,
                    rpc ?? string.Empty,
                    chainId,
                    NativeInitializeCallback,
                    userData);
            }
            catch
            {
                Debug.LogError("This is an error");
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(typeof(CPlayTradeXNative.InitializeCallback))]
        private static void OnNativeInitialize(CPlayTradeXNative.InitializeResult nativeResult, IntPtr userData)
        {
            try
            {
                RequestContext<InitializeResult> context =
                    GetRequestContext<InitializeResult>(userData);

                InitializeResult result =
                    new InitializeResult(
                        nativeResult.initialized != 0,
                        CPlayTradeXNative.GetString(
                            nativeResult.error),
                        CPlayTradeXNative.GetString(
                            nativeResult.walletAddress));

                RunOnUnityThread(() =>
                {
                    NotifyInitializationChanged(
                        result.Initialized);

                    SafeInvoke(() =>
                    {
                        context?.Callback?.Invoke(result);
                    });
                });
            }
            catch (Exception exception)
            {
                RequestContext<InitializeResult> context =
                    GetRequestContext<InitializeResult>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Initialize callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Initializes the PlayTradeX SDK.
        /// </summary>
        /// <param name="storagePath">
        /// Directory used by PlayTradeX for SDK storage.
        /// </param>
        /// <param name="rpc">
        /// Blockchain RPC endpoint used by the SDK.
        /// </param>
        /// <param name="chainId">
        /// Blockchain chain ID.
        /// </param>
        /// <returns>
        /// A task that completes when native SDK initialization
        /// finishes.
        /// </returns>
        public static Task<InitializeResult> InitializeAsync(
            string storagePath,
            string rpc,
            long chainId)
        {
            var completion =
                new TaskCompletionSource<InitializeResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                Initialize(
                    storagePath,
                    rpc,
                    chainId,
                    result =>
                    {
                        completion.TrySetResult(result);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    });
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Shutdown
        // ========================================================

        /// <summary>
        /// Gracefully shuts down the native PlayTradeX SDK.
        /// </summary>
        /// <remarks>
        /// Native shutdown drains operations according to PlayTradeX
        /// shutdown semantics and releases managed native callback
        /// references after native work has completed.
        /// </remarks>
        public static void Shutdown()
        {
            bool wasInitialized = false;

            try
            {
                try
                {
                    wasInitialized =
                        CPlayTradeXNative
                            .CPlayTradeX_IsInitialized() != 0;
                }
                catch
                {
                    wasInitialized = false;
                }

                // Native shutdown drains already queued/running
                // operations according to PlayTradeX semantics.
                CPlayTradeXNative.CPlayTradeX_Shutdown();

                if (wasInitialized)
                {
                    NotifyInitializationChanged(false);
                }
            }
            finally
            {
                _transactionConsentCallback = null;

                try
                {
                    CPlayTradeXNative
                        .CPlayTradeX_SetTransactionConsentCallback(
                            null,
                            IntPtr.Zero);
                }
                catch
                {
                    // Shutdown cleanup must remain safe.
                }

                try
                {
                    CPlayTradeXNative.SetLogCallback(
                        null,
                        IntPtr.Zero);
                }
                catch
                {
                    // Shutdown cleanup must remain safe.
                }

                _loggingRegistered = false;
            }
        }


        // ========================================================
        // Initialization State
        // ========================================================

        /// <summary>
        /// Gets whether the native PlayTradeX SDK is fully initialized.
        /// </summary>
        /// <returns>
        /// True when the SDK is initialized; otherwise false.
        /// </returns>
        public static bool IsInitialized()
        {
            try
            {
                return CPlayTradeXNative
                    .CPlayTradeX_IsInitialized() != 0;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[PlayTradeX Unity] IsInitialized failed: " +
                    exception);

                return false;
            }
        }


        // ========================================================
        // Transaction Consent
        // ========================================================

        /// <summary>
        /// Registers the callback invoked when a prepared transaction
        /// requires approval from the application.
        /// </summary>
        /// <param name="callback">
        /// Callback that receives the prepared transaction.
        /// Pass null to unregister the current callback.
        /// </param>
        public static void SetTransactionConsentCallback(
            Action<PreparedTransaction> callback)
        {
            CaptureUnityContext();

            _transactionConsentCallback = callback;

            if (callback == null)
            {
                CPlayTradeXNative
                    .CPlayTradeX_SetTransactionConsentCallback(
                        null, IntPtr.Zero);

                return;
            }

            CPlayTradeXNative
                .CPlayTradeX_SetTransactionConsentCallback(
                    NativeTransactionConsentCallback, IntPtr.Zero);
        }

        [MonoPInvokeCallback(typeof(CPlayTradeXNative.TransactionConsentCallback))]
        private static void OnNativeTransactionConsent(
            CPlayTradeXNative.PreparedTransaction nativeTransaction, IntPtr userData)
        {
            PreparedTransaction transaction =
                new PreparedTransaction(
                    CPlayTradeXNative.GetString(
                        nativeTransaction.id),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.rpc),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.contractAddress),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.abi),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.functionName),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.params_),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.value),
                    nativeTransaction.gasLimit,
                    CPlayTradeXNative.GetString(
                        nativeTransaction.baseFeePerGas),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.maxPriorityFeePerGas),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.maxFeePerGas),
                    CPlayTradeXNative.GetString(
                        nativeTransaction.estimatedMaxNetworkFee),
                    nativeTransaction.simulationSucceeded != 0,
                    CPlayTradeXNative.GetString(
                        nativeTransaction.simulationError),
                    nativeTransaction.canSubmit != 0,
                    CPlayTradeXNative.GetString(
                        nativeTransaction.preparationError));

            Action<PreparedTransaction> managedCallback =
                _transactionConsentCallback;

            InvokeOnUnityThread(
                managedCallback,
                transaction);
        }

        /// <summary>
        /// Approves a pending prepared transaction.
        /// </summary>
        /// <param name="transactionId">
        /// Identifier of the transaction to approve.
        /// </param>
        /// <returns>
        /// True when the approval request was accepted by the SDK;
        /// otherwise false.
        /// </returns>
        public static bool ApproveTransaction(
            string transactionId)
        {
            return CPlayTradeXNative
                .CPlayTradeX_ApproveTransaction(
                    transactionId ?? string.Empty) != 0;
        }

        /// <summary>
        /// Denies a pending prepared transaction.
        /// </summary>
        /// <param name="transactionId">
        /// Identifier of the transaction to deny.
        /// </param>
        /// <returns>
        /// True when the denial request was accepted by the SDK;
        /// otherwise false.
        /// </returns>
        public static bool DenyTransaction(
            string transactionId)
        {
            return CPlayTradeXNative
                .CPlayTradeX_DenyTransaction(
                    transactionId ?? string.Empty) != 0;
        }


        // ========================================================
        // Native Balance
        // ========================================================

        private static void GetNativeBalance(
            Action<NativeBalanceResponse> callback,
            Action<Exception> errorCallback,
            bool sequential = false)
        {
            CaptureUnityContext();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_GetNativeBalance(
                        NativeBalanceCallback,
                        userData,
                        sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(typeof(CPlayTradeXNative.NativeBalanceCallback))]
        private static void OnNativeBalance(CPlayTradeXNative.NativeBalanceResponse nativeResponse, IntPtr userData)
        {
            try
            {
                RequestContext<NativeBalanceResponse> context =
                    GetRequestContext<NativeBalanceResponse>(
                        userData);

                NativeBalanceResponse response =
                    new NativeBalanceResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        CPlayTradeXNative.GetString(
                            nativeResponse.body),
                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage),
                        CPlayTradeXNative.GetString(
                            nativeResponse.balance));

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<NativeBalanceResponse> context =
                    GetRequestContext<NativeBalanceResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Native balance callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Gets the native currency balance of the SDK wallet.
        /// </summary>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the native balance response.
        /// </returns>
        public static Task<NativeBalanceResponse>
            GetNativeBalanceAsync(
                bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<NativeBalanceResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                GetNativeBalance(
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Send Native Currency
        // ========================================================

        private static void SendEth(
            string to,
            string amount,
            Action<TransactionResponse> callback,
            Action<Exception> errorCallback,
            bool sequential = false)
        {
            CaptureUnityContext();


            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative.CPlayTradeX_SendEth(
                    to ?? string.Empty,
                    amount ?? string.Empty,
                    NativeTransactionCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(
    typeof(CPlayTradeXNative.TransactionCallback))]
        private static void OnNativeTransaction(
    CPlayTradeXNative.TransactionResponse nativeResponse,
    IntPtr userData)
        {
            try
            {
                RequestContext<TransactionResponse> context =
                    GetRequestContext<TransactionResponse>(
                        userData);

                TransactionResponse response =
                    CreateTransactionResponse(nativeResponse);

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<TransactionResponse> context =
                    GetRequestContext<TransactionResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Transaction callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Sends native blockchain currency from the SDK wallet.
        /// </summary>
        /// <param name="to">
        /// Destination wallet address.
        /// </param>
        /// <param name="amount">
        /// Amount of native currency to send.
        /// </param>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the transaction response.
        /// </returns>
        public static Task<TransactionResponse> SendEthAsync(
            string to,
            string amount,
            bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<TransactionResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                SendEth(
                    to,
                    amount,
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Human Readable ABI
        // ========================================================

        private static void HumanReadableAbi(
            string abi,
            bool minimal,
            Action<HumanReadableAbiResponse> callback,
            Action<Exception> errorCallback,
            bool sequential = false)
        {
            CaptureUnityContext();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_HumanReadableAbi(
                        abi ?? string.Empty,
                        minimal ? 1 : 0,
                        NativeHumanReadableAbiCallback,
                userData,
                        sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(
    typeof(CPlayTradeXNative.HumanReadableAbiCallback))]
        private static void OnNativeHumanReadableAbi(
    CPlayTradeXNative.HumanReadableAbiResponse nativeResponse,
    IntPtr userData)
        {
            try
            {
                RequestContext<HumanReadableAbiResponse> context =
                    GetRequestContext<HumanReadableAbiResponse>(
                        userData);

                HumanReadableAbiResponse response =
                    new HumanReadableAbiResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        CPlayTradeXNative.GetString(
                            nativeResponse.body),
                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage),
                        CPlayTradeXNative.GetString(
                            nativeResponse.abi));

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<HumanReadableAbiResponse> context =
                    GetRequestContext<HumanReadableAbiResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Human-readable ABI callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Converts a contract ABI into a human-readable ABI.
        /// </summary>
        /// <param name="abi">
        /// Contract ABI to convert.
        /// </param>
        /// <param name="minimal">
        /// Whether to return the minimal human-readable form.
        /// </param>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the human-readable ABI response.
        /// </returns>
        public static Task<HumanReadableAbiResponse>
            HumanReadableAbiAsync(
                string abi,
                bool minimal,
                bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<HumanReadableAbiResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                HumanReadableAbi(
                    abi,
                    minimal,
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Contract Write
        // ========================================================

        private static void Write(
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            Action<TransactionResponse> callback,
            Action<Exception> errorCallback,
            string value = "",
            bool sequential = false)
        {
            CaptureUnityContext();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);


            try
            {
                CPlayTradeXNative.CPlayTradeX_Write(
                    contractAddress ?? string.Empty,
                    abi ?? string.Empty,
                    functionName ?? string.Empty,
                    parameters ?? string.Empty,
                    NativeTransactionCallback, userData,
                    value ?? string.Empty,
                    sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        /// <summary>
        /// Executes a state-changing smart contract function.
        /// </summary>
        /// <param name="contractAddress">
        /// Target smart contract address.
        /// </param>
        /// <param name="abi">
        /// Contract ABI.
        /// </param>
        /// <param name="functionName">
        /// Contract function to execute.
        /// </param>
        /// <param name="parameters">
        /// Serialized function parameters.
        /// </param>
        /// <param name="value">
        /// Native currency value attached to the transaction.
        /// </param>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the transaction response.
        /// </returns>
        public static Task<TransactionResponse> WriteAsync(
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            string value = "",
            bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<TransactionResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                Write(
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    value,
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Contract Read
        // ========================================================

        private static void Read(
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            Action<ContractReadResponse> callback,
            Action<Exception> errorCallback,
            bool sequential = false)
        {
            CaptureUnityContext();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative.CPlayTradeX_Read(
                    contractAddress ?? string.Empty,
                    abi ?? string.Empty,
                    functionName ?? string.Empty,
                    parameters ?? string.Empty,
                    NativeContractReadCallback, userData,
                    sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(
    typeof(CPlayTradeXNative.ContractReadCallback))]
        private static void OnNativeContractRead(
    CPlayTradeXNative.ContractReadResponse nativeResponse,
    IntPtr userData)
        {
            try
            {
                RequestContext<ContractReadResponse> context =
                    GetRequestContext<ContractReadResponse>(
                        userData);

                ContractReadResponse response =
                    new ContractReadResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        CPlayTradeXNative.GetString(
                            nativeResponse.body),
                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage),
                        CPlayTradeXNative.GetString(
                            nativeResponse.data));

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<ContractReadResponse> context =
                    GetRequestContext<ContractReadResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Contract read callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Executes a read-only smart contract function.
        /// </summary>
        /// <param name="contractAddress">
        /// Target smart contract address.
        /// </param>
        /// <param name="abi">
        /// Contract ABI.
        /// </param>
        /// <param name="functionName">
        /// Contract function to execute.
        /// </param>
        /// <param name="parameters">
        /// Serialized function parameters.
        /// </param>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the contract read response.
        /// </returns>
        public static Task<ContractReadResponse> ReadAsync(
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<ContractReadResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                Read(
                    contractAddress,
                    abi,
                    functionName,
                    parameters,
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }


        // ========================================================
        // Wallet Export
        // ========================================================

        private static void ExportWallet(
            string password,
            string outputPath,
            Action<WalletExportResponse> callback,
            Action<Exception> errorCallback,
            bool sequential = false)
        {
            CaptureUnityContext();

            IntPtr userData =
                CreateRequestContext(
                    callback,
                    errorCallback);

            try
            {
                CPlayTradeXNative.CPlayTradeX_ExportWallet(
                    password ?? string.Empty,
                    outputPath ?? string.Empty,
                    NativeWalletExportCallback, userData,
                    sequential ? 1 : 0);
            }
            catch
            {
                FreeRequestContext(userData);
                throw;
            }
        }

        [MonoPInvokeCallback(
    typeof(CPlayTradeXNative.WalletExportCallback))]
        private static void OnNativeWalletExport(
    CPlayTradeXNative.WalletExportResponse nativeResponse,
    IntPtr userData)
        {
            try
            {
                RequestContext<WalletExportResponse> context =
                    GetRequestContext<WalletExportResponse>(
                        userData);

                WalletExportResponse response =
                    new WalletExportResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        CPlayTradeXNative.GetString(
                            nativeResponse.body),
                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage),
                        CPlayTradeXNative.GetString(
                            nativeResponse.walletAddress),
                        CPlayTradeXNative.GetString(
                            nativeResponse.filePath));

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<WalletExportResponse> context =
                    GetRequestContext<WalletExportResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Wallet export callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        /// <summary>
        /// Exports the current SDK wallet to an encrypted wallet file.
        /// </summary>
        /// <param name="password">
        /// Password used to protect the exported wallet.
        /// </param>
        /// <param name="outputPath">
        /// Destination file path or supported platform URI.
        /// </param>
        /// <param name="sequential">
        /// When true, the request participates in sequential
        /// request execution.
        /// </param>
        /// <returns>
        /// A task containing the wallet export response.
        /// </returns>
        public static Task<WalletExportResponse> ExportWalletAsync(
            string password,
            string outputPath,
            bool sequential = false)
        {
            var completion =
                new TaskCompletionSource<WalletExportResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                ExportWallet(
                    password,
                    outputPath,
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    },
                    sequential);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }

        private static Task<WalletImportResponse> ImportWalletAsyncInternal(
    string password,
    string inputPath)
        {
            CaptureUnityContext();

            var completion =
                new TaskCompletionSource<WalletImportResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            IntPtr userData =
                CreateRequestContext<WalletImportResponse>(
                    response =>
                    {
                        completion.TrySetResult(response);
                    },
                    exception =>
                    {
                        completion.TrySetException(exception);
                    });

            try
            {
                CPlayTradeXNative.CPlayTradeX_ImportWallet(
                    password ?? string.Empty,
                    inputPath ?? string.Empty,
                    NativeWalletImportCallback,
                    userData,
                    0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(userData);
                completion.TrySetException(exception);
            }

            return completion.Task;
        }

        [MonoPInvokeCallback(
    typeof(CPlayTradeXNative.WalletImportCallback))]
        private static void OnNativeWalletImport(
    CPlayTradeXNative.WalletImportResponse nativeResponse,
    IntPtr userData)
        {
            try
            {
                RequestContext<WalletImportResponse> context =
                    GetRequestContext<WalletImportResponse>(
                        userData);

                WalletImportResponse response =
                    new WalletImportResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        CPlayTradeXNative.GetString(
                            nativeResponse.body),
                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage),
                        CPlayTradeXNative.GetString(
                            nativeResponse.walletAddress));

                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<WalletImportResponse> context =
                    GetRequestContext<WalletImportResponse>(userData);

                context?.ErrorCallback?.Invoke(exception);

                SafeLogError(
                    "[PlayTradeX Unity] Wallet import callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(userData);
            }
        }

        public static Task<WalletImportResponse> ImportWalletAsync(
    string password,
    string inputPath)
        {
            return ImportWalletAsyncInternal(
                password,
                inputPath);
        }

        // ========================================================
        // Transaction Response Conversion
        // ========================================================

        private static TransactionResponse CreateTransactionResponse(
            CPlayTradeXNative.TransactionResponse nativeResponse)
        {
            return new TransactionResponse(
                nativeResponse.success != 0,
                nativeResponse.errorCode,
                CPlayTradeXNative.GetString(
                    nativeResponse.body),
                CPlayTradeXNative.GetString(
                    nativeResponse.errorMessage),
                CPlayTradeXNative.GetString(
                    nativeResponse.transactionHash),
                CPlayTradeXNative.GetString(
                    nativeResponse.receipt));
        }


        // ========================================================
        // Initialization Event
        // ========================================================

        private static void NotifyInitializationChanged(
            bool initialized)
        {
            SafeInvoke(() =>
            {
                InitializationChanged?.Invoke(initialized);
            });
        }

        // ========================================================
        // Unity Main Thread
        // ========================================================

        /// <summary>
        /// Captures Unity's SynchronizationContext once.
        /// </summary>
        /// <remarks>
        /// The first capture must occur on Unity's main thread.
        /// PlayTradeXLifecycle calls SetupLogging from Awake before
        /// SDK initialization, which establishes the Unity context.
        ///
        /// Once captured, the context is never replaced by subsequent
        /// SDK calls from worker threads.
        /// </remarks>
        private static void CaptureUnityContext()
        {
            if (_unitySynchronizationContext != null)
            {
                return;
            }

            SynchronizationContext context =
                SynchronizationContext.Current;

            if (context == null)
            {
                return;
            }

            lock (UnityContextLock)
            {
                if (_unitySynchronizationContext != null)
                {
                    return;
                }

                _unitySynchronizationContext = context;
                _unityThreadId =
                    Thread.CurrentThread.ManagedThreadId;
            }
        }

        /// <summary>
        /// Executes an action on Unity's main thread.
        /// </summary>
        /// <remarks>
        /// Native callbacks may originate from PlayTradeX worker
        /// threads. Unity-facing callbacks are therefore dispatched
        /// through the Unity SynchronizationContext.
        /// </remarks>
        private static void RunOnUnityThread(
            Action action)
        {
            if (action == null)
            {
                return;
            }

            SynchronizationContext context =
                _unitySynchronizationContext;

            if (context == null)
            {
                SafeLogError(
                    "[PlayTradeX Unity] Unity SynchronizationContext " +
                    "has not been captured. The callback was not " +
                    "executed.");

                return;
            }

            if (Thread.CurrentThread.ManagedThreadId ==
                _unityThreadId)
            {
                SafeInvoke(action);
                return;
            }

            context.Post(
                _ =>
                {
                    SafeInvoke(action);
                },
                null);
        }

        private static void InvokeOnUnityThread<T>(
            Action<T> callback,
            T value)
        {
            if (callback == null)
            {
                return;
            }

            RunOnUnityThread(() =>
            {
                callback(value);
            });
        }


        // ========================================================
        // Managed Exception Boundary
        // ========================================================

        /// <summary>
        /// Prevents exceptions thrown by managed callbacks from
        /// crossing a native callback boundary.
        /// </summary>
        private static void SafeInvoke(
            Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception exception)
            {
                SafeLogError(
                    "[PlayTradeX Unity] Managed callback exception: " +
                    exception);
            }
        }

        /// <summary>
        /// Logs an error without allowing Unity logging failures to
        /// escape SDK cleanup or native callback boundaries.
        /// </summary>
        private static void SafeLogError(
            string message)
        {
            try
            {
                Debug.LogError(message);
            }
            catch
            {
                // Never allow logging failures to escape through
                // SDK cleanup or a native callback boundary.
            }
        }

        private sealed class RequestContext<T>
        {
            internal readonly Action<T> Callback;
            internal readonly Action<Exception> ErrorCallback;

            internal RequestContext(
                Action<T> callback,
                Action<Exception> errorCallback)
            {
                Callback = callback;
                ErrorCallback = errorCallback;
            }
        }

        private static IntPtr CreateRequestContext<T>(
            Action<T> callback,
            Action<Exception> errorCallback = null)
        {
            GCHandle handle =
                GCHandle.Alloc(
                    new RequestContext<T>(
                        callback,
                        errorCallback));

            return GCHandle.ToIntPtr(handle);
        }

        private static RequestContext<T> GetRequestContext<T>(
            IntPtr userData)
        {
            if (userData == IntPtr.Zero)
            {
                return null;
            }

            GCHandle handle =
                GCHandle.FromIntPtr(userData);

            return handle.Target as RequestContext<T>;
        }

        private static void FreeRequestContext(
            IntPtr userData)
        {
            if (userData == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle =
                GCHandle.FromIntPtr(userData);

            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }
}