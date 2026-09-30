using AOT;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;


namespace PlayTradeX
{
    /// <summary>
    /// Main managed Unity API for the PlayTradeX SDK.
    /// </summary>
    /// <remarks>
    /// Provides Task-based managed wrappers around the native PlayTradeX
    /// C ABI.
    ///
    /// The managed API supports:
    ///
    /// - Multiple configured blockchain networks.
    /// - The SDK-managed PlayTradeX identity wallet.
    /// - Application-managed external wallets.
    /// - Standalone wallet generation.
    /// - Transaction consent.
    /// - Native currency transfers.
    /// - Smart contract reads and writes.
    /// - Identity-wallet import/export.
    ///
    /// Native callbacks are copied into managed objects before returning
    /// from the native callback and are dispatched to Unity's main thread
    /// before application-facing callbacks are executed.
    /// </remarks>
    public static class PlayTradeXSdk
    {
        // ========================================================
        // Events
        // ========================================================

        /// <summary>
        /// Raised whenever the SDK initialization state changes.
        /// </summary>
        public static event Action<bool> InitializationChanged;


        // ========================================================
        // Native Callback References
        // ========================================================

        /*
         * These delegates are stored statically so that the GC cannot
         * collect them while native code still holds their function
         * pointers.
         */

        private static readonly CPlayTradeXNative.LogCallback
            NativeLogCallback = OnNativeLog;

        private static readonly CPlayTradeXNative.GenerateWalletCallback
            NativeGenerateWalletCallback = OnNativeGenerateWallet;

        private static readonly CPlayTradeXNative.InitializeCallback
            NativeInitializeCallback = OnNativeInitialize;

        private static readonly CPlayTradeXNative.TransactionConsentCallback
            NativeTransactionConsentCallback = OnNativeTransactionConsent;

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


        // ========================================================
        // Transaction Consent
        // ========================================================

        private static Action<PreparedTransaction>
            _transactionConsentCallback;


        // ========================================================
        // Unity Main Thread
        // ========================================================

        private static readonly object UnityContextLock =
            new object();

        private static SynchronizationContext
            _unitySynchronizationContext;

        private static int
            _unityThreadId;


        // ========================================================
        // Logging State
        // ========================================================

        private static bool
            _loggingRegistered;


        // ========================================================
        // Logging
        // ========================================================

        /// <summary>
        /// Registers native PlayTradeX logging with the Unity Console.
        /// </summary>
        /// <remarks>
        /// This should initially be called from Unity's main thread so
        /// the Unity SynchronizationContext can be captured.
        /// </remarks>
        public static void SetupLogging()
        {
            CaptureUnityContext();

            if (_loggingRegistered)
            {
                return;
            }

            CPlayTradeXNative.SetLogCallback(
                NativeLogCallback,
                IntPtr.Zero);

            _loggingRegistered = true;
        }


        [MonoPInvokeCallback(typeof(CPlayTradeXNative.LogCallback))]
        private static void OnNativeLog(
            IntPtr message,
            int type,
            IntPtr userData)
        {
            /*
             * Native string memory is valid only for the callback.
             * Copy before dispatching to the Unity thread.
             */

            string text =
                CPlayTradeXNative.GetString(message);

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
        // Network Configuration Marshalling
        // ========================================================

        /// <summary>
        /// Owns temporary unmanaged memory used while passing managed
        /// NetworkConfig objects to CPlayTradeX_Initialize.
        /// </summary>
        /// <remarks>
        /// The native SDK copies the supplied configuration during
        /// CPlayTradeX_Initialize, so these allocations may be released
        /// immediately after the native function returns.
        /// </remarks>
        private sealed class NativeNetworkAllocation : IDisposable
        {
            private readonly List<IntPtr> _strings =
                new List<IntPtr>();

            private readonly List<IntPtr> _rpcArrays =
                new List<IntPtr>();

            private IntPtr _networkArray;


            internal IntPtr NetworkArray =>
                _networkArray;


            internal uint NetworkCount { get; private set; }


            internal NativeNetworkAllocation(
                NetworkConfig[] networks)
            {
                if (networks == null)
                {
                    throw new ArgumentNullException(
                        nameof(networks));
                }

                if (networks.Length == 0)
                {
                    throw new ArgumentException(
                        "At least one network must be configured.",
                        nameof(networks));
                }

                ValidateNetworkConfigurations(
                    networks);

                NetworkCount =
                    checked((uint)networks.Length);

                int networkStructSize =
                    Marshal.SizeOf<CPlayTradeXNative.NetworkConfig>();

                _networkArray =
                    Marshal.AllocHGlobal(
                        checked(
                            networkStructSize *
                            networks.Length));

                try
                {
                    for (int i = 0;
                         i < networks.Length;
                         ++i)
                    {
                        NetworkConfig network =
                            networks[i];

                        IntPtr idPointer =
                            AllocateString(
                                network.Id);

                        IntPtr rpcArrayPointer =
                            AllocateRpcArray(
                                network.RpcEndpoints);

                        var nativeNetwork =
                            new CPlayTradeXNative.NetworkConfig
                            {
                                id =
                                    idPointer,

                                chainId =
                                    network.ChainId,

                                rpcEndpoints =
                                    rpcArrayPointer,

                                rpcEndpointCount =
                                    checked(
                                        (uint)
                                        network.RpcEndpoints.Length)
                            };

                        IntPtr destination =
                            IntPtr.Add(
                                _networkArray,
                                checked(
                                    i *
                                    networkStructSize));

                        Marshal.StructureToPtr(
                            nativeNetwork,
                            destination,
                            false);
                    }
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }


            private IntPtr AllocateString(
                string value)
            {
                IntPtr pointer =
                    Marshal.StringToHGlobalAnsi(
                        value);

                _strings.Add(
                    pointer);

                return pointer;
            }


            private IntPtr AllocateRpcArray(
                string[] rpcEndpoints)
            {
                int pointerSize =
                    IntPtr.Size;

                IntPtr arrayPointer =
                    Marshal.AllocHGlobal(
                        checked(
                            pointerSize *
                            rpcEndpoints.Length));

                _rpcArrays.Add(
                    arrayPointer);

                for (int i = 0;
                     i < rpcEndpoints.Length;
                     ++i)
                {
                    IntPtr rpcPointer =
                        AllocateString(
                            rpcEndpoints[i]);

                    Marshal.WriteIntPtr(
                        arrayPointer,
                        checked(
                            i *
                            pointerSize),
                        rpcPointer);
                }

                return arrayPointer;
            }


            public void Dispose()
            {
                for (int i = 0;
                     i < _strings.Count;
                     ++i)
                {
                    if (_strings[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(
                            _strings[i]);
                    }
                }

                _strings.Clear();


                for (int i = 0;
                     i < _rpcArrays.Count;
                     ++i)
                {
                    if (_rpcArrays[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(
                            _rpcArrays[i]);
                    }
                }

                _rpcArrays.Clear();


                if (_networkArray != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(
                        _networkArray);

                    _networkArray =
                        IntPtr.Zero;
                }

                NetworkCount = 0;
            }
        }


        private static void ValidateNetworkConfigurations(
            NetworkConfig[] networks)
        {
            var ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0;
                 i < networks.Length;
                 ++i)
            {
                NetworkConfig network =
                    networks[i];

                if (network == null)
                {
                    throw new ArgumentException(
                        $"Network configuration at index {i} is null.",
                        nameof(networks));
                }

                ValidateNetworkId(
                    network.Id);

                if (!ids.Add(network.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate network ID '{network.Id}'.",
                        nameof(networks));
                }

                if (network.ChainId == 0)
                {
                    throw new ArgumentException(
                        $"Network '{network.Id}' has an invalid chain ID.",
                        nameof(networks));
                }

                if (network.RpcEndpoints == null ||
                    network.RpcEndpoints.Length == 0)
                {
                    throw new ArgumentException(
                        $"Network '{network.Id}' has no RPC endpoints.",
                        nameof(networks));
                }

                for (int rpcIndex = 0;
                     rpcIndex < network.RpcEndpoints.Length;
                     ++rpcIndex)
                {
                    if (string.IsNullOrWhiteSpace(
                            network.RpcEndpoints[rpcIndex]))
                    {
                        throw new ArgumentException(
                            $"Network '{network.Id}' contains an empty " +
                            $"RPC endpoint at index {rpcIndex}.",
                            nameof(networks));
                    }
                }
            }
        }


        private static void ValidateNetworkId(
            string networkId)
        {
            if (string.IsNullOrWhiteSpace(
                    networkId))
            {
                throw new ArgumentException(
                    "Network ID is required.",
                    nameof(networkId));
            }
        }


        private static void ValidatePrivateKey(
            string privateKey)
        {
            if (string.IsNullOrWhiteSpace(
                    privateKey))
            {
                throw new ArgumentException(
                    "External wallet private key is required.",
                    nameof(privateKey));
            }
        }


        // ========================================================
        // Standalone Wallet Generation
        // ========================================================

        /// <summary>
        /// Generates a new standalone EVM wallet.
        /// </summary>
        /// <remarks>
        /// The generated wallet is not persisted by PlayTradeX and does
        /// not replace the SDK-managed identity wallet.
        ///
        /// The returned private key belongs to the calling application.
        /// </remarks>
        public static Task<GenerateWalletResponse> GenerateWalletAsync(
            bool sequential = false)
        {
            CaptureUnityContext();

            var completion =
                new TaskCompletionSource<GenerateWalletResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            IntPtr userData =
                CreateRequestContext<GenerateWalletResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });

            try
            {
                CPlayTradeXNative.CPlayTradeX_GenerateWallet(
                    NativeGenerateWalletCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }

            return completion.Task;
        }


        [MonoPInvokeCallback(
            typeof(CPlayTradeXNative.GenerateWalletCallback))]
        private static void OnNativeGenerateWallet(
            CPlayTradeXNative.GenerateWalletResponse nativeResponse,
            IntPtr userData)
        {
            try
            {
                RequestContext<GenerateWalletResponse> context =
                    GetRequestContext<GenerateWalletResponse>(
                        userData);

                GeneratedWallet wallet =
                    new GeneratedWallet(
                        CPlayTradeXNative.GetString(
                            nativeResponse.wallet.address),

                        CPlayTradeXNative.GetString(
                            nativeResponse.wallet.publicKey),

                        CPlayTradeXNative.GetString(
                            nativeResponse.wallet.privateKey));


                GenerateWalletResponse response =
                    new GenerateWalletResponse(
                        nativeResponse.success != 0,
                        nativeResponse.errorCode,
                        wallet,

                        CPlayTradeXNative.GetString(
                            nativeResponse.errorMessage));


                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<GenerateWalletResponse> context =
                    GetRequestContext<GenerateWalletResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Generate wallet callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Initialize
        // ========================================================

        /// <summary>
        /// Initializes PlayTradeX with all configured blockchain networks.
        /// </summary>
        public static Task<InitializeResult> InitializeAsync(
            string storagePath,
            NetworkConfig[] networks)
        {
            CaptureUnityContext();

            SetupLogging();


            var completion =
                new TaskCompletionSource<InitializeResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            if (string.IsNullOrWhiteSpace(
                    storagePath))
            {
                completion.TrySetException(
                    new ArgumentException(
                        "Storage path is required.",
                        nameof(storagePath)));

                return completion.Task;
            }


            if (networks == null)
            {
                completion.TrySetException(
                    new ArgumentNullException(
                        nameof(networks)));

                return completion.Task;
            }


            if (networks.Length == 0)
            {
                completion.TrySetException(
                    new ArgumentException(
                        "At least one network must be configured.",
                        nameof(networks)));

                return completion.Task;
            }


            IntPtr userData =
                IntPtr.Zero;

            try
            {
                /*
                 * Validate and marshal before allocating callback state.
                 * This prevents a GCHandle from being allocated when the
                 * configuration itself is invalid.
                 */

                using (var nativeNetworks =
                       new NativeNetworkAllocation(
                           networks))
                {
                    userData =
                        CreateRequestContext<InitializeResult>(
                            result =>
                            {
                                completion.TrySetResult(
                                    result);
                            },
                            exception =>
                            {
                                completion.TrySetException(
                                    exception);
                            });


                    CPlayTradeXNative.CPlayTradeX_Initialize(
                        storagePath,
                        nativeNetworks.NetworkArray,
                        nativeNetworks.NetworkCount,
                        NativeInitializeCallback,
                        userData);
                }
            }
            catch (Exception exception)
            {
                if (userData != IntPtr.Zero)
                {
                    FreeRequestContext(
                        userData);
                }

                completion.TrySetException(
                    exception);
            }

            return completion.Task;
        }


        [MonoPInvokeCallback(
            typeof(CPlayTradeXNative.InitializeCallback))]
        private static void OnNativeInitialize(
            CPlayTradeXNative.InitializeResult nativeResult,
            IntPtr userData)
        {
            try
            {
                RequestContext<InitializeResult> context =
                    GetRequestContext<InitializeResult>(
                        userData);


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
                        context?.Callback?.Invoke(
                            result);
                    });
                });
            }
            catch (Exception exception)
            {
                RequestContext<InitializeResult> context =
                    GetRequestContext<InitializeResult>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Initialize callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Shutdown
        // ========================================================

        /// <summary>
        /// Shuts down the native PlayTradeX SDK.
        /// </summary>
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


                CPlayTradeXNative
                    .CPlayTradeX_Shutdown();


                if (wasInitialized)
                {
                    NotifyInitializationChanged(
                        false);
                }
            }
            finally
            {
                _transactionConsentCallback =
                    null;


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


                _loggingRegistered =
                    false;
            }
        }


        // ========================================================
        // Initialization State
        // ========================================================

        /// <summary>
        /// Returns whether the native SDK is initialized.
        /// </summary>
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
        /// Registers the application callback that receives prepared
        /// transactions requiring user consent.
        /// </summary>
        public static void SetTransactionConsentCallback(
            Action<PreparedTransaction> callback)
        {
            CaptureUnityContext();

            _transactionConsentCallback =
                callback;


            if (callback == null)
            {
                CPlayTradeXNative
                    .CPlayTradeX_SetTransactionConsentCallback(
                        null,
                        IntPtr.Zero);

                return;
            }


            CPlayTradeXNative
                .CPlayTradeX_SetTransactionConsentCallback(
                    NativeTransactionConsentCallback,
                    IntPtr.Zero);
        }


        [MonoPInvokeCallback(
            typeof(CPlayTradeXNative.TransactionConsentCallback))]
        private static void OnNativeTransactionConsent(
            CPlayTradeXNative.PreparedTransaction nativeTransaction,
            IntPtr userData)
        {
            /*
             * Every native pointer must be copied before returning from
             * this callback.
             */

            PreparedTransaction transaction =
                new PreparedTransaction(
                    CPlayTradeXNative.GetString(
                        nativeTransaction.id),

                    CPlayTradeXNative.GetString(
                        nativeTransaction.networkId),

                    nativeTransaction.chainId,

                    CPlayTradeXNative.GetString(
                        nativeTransaction.fromAddress),

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


            Action<PreparedTransaction> callback =
                _transactionConsentCallback;


            InvokeOnUnityThread(
                callback,
                transaction);
        }


        /// <summary>
        /// Approves a pending prepared transaction.
        /// </summary>
        public static bool ApproveTransaction(
            string transactionId)
        {
            if (string.IsNullOrWhiteSpace(
                    transactionId))
            {
                return false;
            }

            return CPlayTradeXNative
                .CPlayTradeX_ApproveTransaction(
                    transactionId) != 0;
        }


        /// <summary>
        /// Denies a pending prepared transaction.
        /// </summary>
        public static bool DenyTransaction(
            string transactionId)
        {
            if (string.IsNullOrWhiteSpace(
                    transactionId))
            {
                return false;
            }

            return CPlayTradeXNative
                .CPlayTradeX_DenyTransaction(
                    transactionId) != 0;
        }


        // ========================================================
        // Native Balance - Identity Wallet
        // ========================================================

        /// <summary>
        /// Gets the native currency balance of the PlayTradeX identity
        /// wallet on the selected configured network.
        /// </summary>
        public static Task<NativeBalanceResponse>
            GetNativeBalanceAsync(
                string networkId,
                bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<NativeBalanceResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<NativeBalanceResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_GetNativeBalance(
                        networkId,
                        NativeBalanceCallback,
                        userData,
                        sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        // ========================================================
        // Native Balance - Arbitrary Address
        // ========================================================

        /// <summary>
        /// Gets the native currency balance of an arbitrary EVM address
        /// on the selected configured network.
        /// </summary>
        /// <remarks>
        /// No private key is required because balance lookup is a
        /// read-only blockchain operation.
        /// </remarks>
        public static Task<NativeBalanceResponse>
            GetNativeBalanceForAddressAsync(
                string networkId,
                string address,
                bool sequential = false)
        {
            ValidateNetworkId(
                networkId);


            if (string.IsNullOrWhiteSpace(
                    address))
            {
                throw new ArgumentException(
                    "Wallet address is required.",
                    nameof(address));
            }


            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<NativeBalanceResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<NativeBalanceResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_GetNativeBalanceForAddress(
                        networkId,
                        address,
                        NativeBalanceCallback,
                        userData,
                        sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        [MonoPInvokeCallback(
            typeof(CPlayTradeXNative.NativeBalanceCallback))]
        private static void OnNativeBalance(
            CPlayTradeXNative.NativeBalanceResponse nativeResponse,
            IntPtr userData)
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
                    GetRequestContext<NativeBalanceResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Native balance callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Send Native Currency - Identity Wallet
        // ========================================================

        /// <summary>
        /// Sends native blockchain currency using the SDK-managed
        /// PlayTradeX identity wallet.
        /// </summary>
        public static Task<TransactionResponse> SendEthAsync(
            string networkId,
            string to,
            string amount,
            bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            ValidateSendParameters(
                to,
                amount);

            CaptureUnityContext();


            var completion =
                CreateTransactionCompletion();


            IntPtr userData =
                CreateTransactionRequestContext(
                    completion);


            try
            {
                CPlayTradeXNative.CPlayTradeX_SendEth(
                    networkId,
                    to,
                    amount,
                    NativeTransactionCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        // ========================================================
        // Send Native Currency - External Wallet
        // ========================================================

        /// <summary>
        /// Sends native blockchain currency using an application-managed
        /// external wallet.
        /// </summary>
        /// <remarks>
        /// The private key is supplied only for this execution.
        /// The native SDK does not persist it as the PlayTradeX
        /// identity wallet.
        /// </remarks>
        public static Task<TransactionResponse> SendEthWithWalletAsync(
            string networkId,
            string privateKey,
            string to,
            string amount,
            bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            ValidatePrivateKey(
                privateKey);

            ValidateSendParameters(
                to,
                amount);

            CaptureUnityContext();


            var completion =
                CreateTransactionCompletion();


            IntPtr userData =
                CreateTransactionRequestContext(
                    completion);


            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_SendEthWithWallet(
                        networkId,
                        privateKey,
                        to,
                        amount,
                        NativeTransactionCallback,
                        userData,
                        sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        private static void ValidateSendParameters(
            string to,
            string amount)
        {
            if (string.IsNullOrWhiteSpace(
                    to))
            {
                throw new ArgumentException(
                    "Recipient address is required.",
                    nameof(to));
            }

            if (string.IsNullOrWhiteSpace(
                    amount))
            {
                throw new ArgumentException(
                    "Amount is required.",
                    nameof(amount));
            }
        }


        // ========================================================
        // Transaction Callback
        // ========================================================

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
                    CreateTransactionResponse(
                        nativeResponse);


                InvokeOnUnityThread(
                    context?.Callback,
                    response);
            }
            catch (Exception exception)
            {
                RequestContext<TransactionResponse> context =
                    GetRequestContext<TransactionResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Transaction callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Human Readable ABI
        // ========================================================

        /// <summary>
        /// Converts a contract ABI into PlayTradeX human-readable ABI
        /// format.
        /// </summary>
        public static Task<HumanReadableAbiResponse>
            HumanReadableAbiAsync(
                string abi,
                bool minimal,
                bool sequential = false)
        {
            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<HumanReadableAbiResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<HumanReadableAbiResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_HumanReadableAbi(
                        abi ?? string.Empty,
                        minimal ? 1 : 0,
                        NativeHumanReadableAbiCallback,
                        userData);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
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
                    GetRequestContext<HumanReadableAbiResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Human-readable ABI callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Contract Write - Identity Wallet
        // ========================================================

        /// <summary>
        /// Executes a state-changing contract operation using the
        /// SDK-managed PlayTradeX identity wallet.
        /// </summary>
        public static Task<TransactionResponse> WriteAsync(
            string networkId,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            string value = "0",
            bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            ValidateWriteParameters(
                contractAddress,
                abi,
                functionName);


            CaptureUnityContext();


            var completion =
                CreateTransactionCompletion();


            IntPtr userData =
                CreateTransactionRequestContext(
                    completion);


            try
            {
                CPlayTradeXNative.CPlayTradeX_Write(
                    networkId,
                    contractAddress,
                    abi,
                    functionName,
                    parameters ?? string.Empty,
                    NativeTransactionCallback,
                    userData,
                    string.IsNullOrEmpty(value) ? "0" : value,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        // ========================================================
        // Contract Write - External Wallet
        // ========================================================

        /// <summary>
        /// Executes a state-changing contract operation using an
        /// application-managed external wallet.
        /// </summary>
        public static Task<TransactionResponse> WriteWithWalletAsync(
            string networkId,
            string privateKey,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            string value = "0",
            bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            ValidatePrivateKey(
                privateKey);

            ValidateWriteParameters(
                contractAddress,
                abi,
                functionName);


            CaptureUnityContext();


            var completion =
                CreateTransactionCompletion();


            IntPtr userData =
                CreateTransactionRequestContext(
                    completion);


            try
            {
                CPlayTradeXNative
                    .CPlayTradeX_WriteWithWallet(
                        networkId,
                        privateKey,
                        contractAddress,
                        abi,
                        functionName,
                        parameters ?? string.Empty,
                        NativeTransactionCallback,
                        userData,
                        string.IsNullOrEmpty(value) ? "0" : value,
                        sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
        }


        private static void ValidateWriteParameters(
            string contractAddress,
            string abi,
            string functionName)
        {
            if (string.IsNullOrWhiteSpace(
                    contractAddress))
            {
                throw new ArgumentException(
                    "Contract address is required.",
                    nameof(contractAddress));
            }

            if (string.IsNullOrWhiteSpace(
                    abi))
            {
                throw new ArgumentException(
                    "Contract ABI is required.",
                    nameof(abi));
            }

            if (string.IsNullOrWhiteSpace(
                    functionName))
            {
                throw new ArgumentException(
                    "Contract function name is required.",
                    nameof(functionName));
            }
        }


        // ========================================================
        // Contract Read
        // ========================================================

        /// <summary>
        /// Executes a read-only smart contract call on the selected
        /// configured network.
        /// </summary>
        /// <remarks>
        /// Read does not require an identity or external wallet because
        /// no transaction is signed.
        /// </remarks>
        public static Task<ContractReadResponse> ReadAsync(
            string networkId,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            bool sequential = false)
        {
            ValidateNetworkId(
                networkId);

            ValidateWriteParameters(
                contractAddress,
                abi,
                functionName);


            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<ContractReadResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<ContractReadResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative.CPlayTradeX_Read(
                    networkId,
                    contractAddress,
                    abi,
                    functionName,
                    parameters ?? string.Empty,
                    NativeContractReadCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
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
                    GetRequestContext<ContractReadResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Contract read callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Identity Wallet Export
        // ========================================================

        /// <summary>
        /// Exports the SDK-managed PlayTradeX identity wallet to an
        /// encrypted wallet backup.
        /// </summary>
        /// <remarks>
        /// This operation applies only to the PlayTradeX identity wallet.
        /// Standalone wallets generated by GenerateWalletAsync are owned
        /// and managed by the application.
        /// </remarks>
        public static Task<WalletExportResponse> ExportWalletAsync(
            string password,
            string outputPath,
            bool sequential = false)
        {
            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<WalletExportResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<WalletExportResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative.CPlayTradeX_ExportWallet(
                    password ?? string.Empty,
                    outputPath ?? string.Empty,
                    NativeWalletExportCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
            }


            return completion.Task;
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
                    GetRequestContext<WalletExportResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Wallet export callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Identity Wallet Import
        // ========================================================

        /// <summary>
        /// Imports an encrypted PlayTradeX identity-wallet backup.
        /// </summary>
        public static Task<WalletImportResponse> ImportWalletAsync(
            string password,
            string inputPath,
            bool sequential = false)
        {
            CaptureUnityContext();


            var completion =
                new TaskCompletionSource<WalletImportResponse>(
                    TaskCreationOptions.RunContinuationsAsynchronously);


            IntPtr userData =
                CreateRequestContext<WalletImportResponse>(
                    response =>
                    {
                        completion.TrySetResult(
                            response);
                    },
                    exception =>
                    {
                        completion.TrySetException(
                            exception);
                    });


            try
            {
                CPlayTradeXNative.CPlayTradeX_ImportWallet(
                    password ?? string.Empty,
                    inputPath ?? string.Empty,
                    NativeWalletImportCallback,
                    userData,
                    sequential ? 1 : 0);
            }
            catch (Exception exception)
            {
                FreeRequestContext(
                    userData);

                completion.TrySetException(
                    exception);
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
                    GetRequestContext<WalletImportResponse>(
                        userData);

                context?.ErrorCallback?.Invoke(
                    exception);

                SafeLogError(
                    "[PlayTradeX Unity] Wallet import callback failed: " +
                    exception);
            }
            finally
            {
                FreeRequestContext(
                    userData);
            }
        }


        // ========================================================
        // Transaction Helpers
        // ========================================================

        private static TaskCompletionSource<TransactionResponse>
            CreateTransactionCompletion()
        {
            return new TaskCompletionSource<TransactionResponse>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }


        private static IntPtr CreateTransactionRequestContext(
            TaskCompletionSource<TransactionResponse> completion)
        {
            return CreateRequestContext<TransactionResponse>(
                response =>
                {
                    completion.TrySetResult(
                        response);
                },
                exception =>
                {
                    completion.TrySetException(
                        exception);
                });
        }


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
                InitializationChanged?.Invoke(
                    initialized);
            });
        }


        // ========================================================
        // Unity Main Thread Dispatch
        // ========================================================

        /// <summary>
        /// Captures Unity's SynchronizationContext.
        /// </summary>
        /// <remarks>
        /// The first successful capture must occur on Unity's main
        /// thread. Once captured, the context is not replaced by calls
        /// originating from native or worker threads.
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


                _unitySynchronizationContext =
                    context;

                _unityThreadId =
                    Thread.CurrentThread.ManagedThreadId;
            }
        }


        /// <summary>
        /// Executes an action on Unity's main thread.
        /// </summary>
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
                    "has not been captured. The callback was not executed.");

                return;
            }


            if (Thread.CurrentThread.ManagedThreadId ==
                _unityThreadId)
            {
                SafeInvoke(
                    action);

                return;
            }


            context.Post(
                _ =>
                {
                    SafeInvoke(
                        action);
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
                callback(
                    value);
            });
        }


        // ========================================================
        // Managed Exception Boundary
        // ========================================================

        /// <summary>
        /// Prevents managed callback exceptions from escaping through
        /// native callback boundaries.
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


        private static void SafeLogError(
            string message)
        {
            try
            {
                Debug.LogError(
                    message);
            }
            catch
            {
                /*
                 * Logging must never cause native callback cleanup
                 * to fail.
                 */
            }
        }


        // ========================================================
        // Managed Request Context
        // ========================================================

        /// <summary>
        /// Stores the managed callbacks associated with one native
        /// asynchronous request.
        /// </summary>
        private sealed class RequestContext<T>
        {
            internal readonly Action<T>
                Callback;

            internal readonly Action<Exception>
                ErrorCallback;


            internal RequestContext(
                Action<T> callback,
                Action<Exception> errorCallback)
            {
                Callback =
                    callback;

                ErrorCallback =
                    errorCallback;
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


            return GCHandle.ToIntPtr(
                handle);
        }


        private static RequestContext<T>
            GetRequestContext<T>(
                IntPtr userData)
        {
            if (userData == IntPtr.Zero)
            {
                return null;
            }


            GCHandle handle =
                GCHandle.FromIntPtr(
                    userData);


            return handle.Target
                as RequestContext<T>;
        }


        private static void FreeRequestContext(
            IntPtr userData)
        {
            if (userData == IntPtr.Zero)
            {
                return;
            }


            GCHandle handle =
                GCHandle.FromIntPtr(
                    userData);


            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }
}