using System;
using System.Runtime.InteropServices;

namespace PlayTradeX
{
    /// <summary>
    /// Defines the native C ABI types and callback signatures used
    /// to communicate with the PlayTradeX native library.
    /// </summary>
    /// <remarks>
    /// These structures must remain binary-compatible with their
    /// corresponding definitions in CPlayTradeXTypes.h.
    ///
    /// Do not reorder, remove, or change field types without making
    /// the corresponding change to the native C ABI.
    ///
    /// Native strings exposed through callback structures are only
    /// valid for the duration of the native callback. Managed code
    /// must copy them before returning from the callback.
    /// </remarks>
    internal static partial class CPlayTradeXNative
    {
        // ========================================================
        // Network Configuration
        // ========================================================

        /// <summary>
        /// Native C ABI representation of one configured blockchain network.
        /// </summary>
        /// <remarks>
        /// rpcEndpoints points to an unmanaged array of pointers to
        /// null-terminated strings.
        ///
        /// This structure is used only while calling Initialize.
        /// The native SDK copies the supplied network configuration.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct NetworkConfig
        {
            internal IntPtr id;
            internal ulong chainId;
            internal IntPtr rpcEndpoints;
            internal uint rpcEndpointCount;
        }


        // ========================================================
        // Wallet Types
        // ========================================================

        /// <summary>
        /// Native representation of a standalone generated EVM wallet.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct GeneratedWallet
        {
            internal IntPtr address;
            internal IntPtr publicKey;
            internal IntPtr privateKey;
        }


        /// <summary>
        /// Native response returned by CPlayTradeX_GenerateWallet.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct GenerateWalletResponse
        {
            internal int success;
            internal int errorCode;

            internal GeneratedWallet wallet;

            internal IntPtr errorMessage;
        }


        // ========================================================
        // Initialization
        // ========================================================

        /// <summary>
        /// Native result returned when SDK initialization completes.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct InitializeResult
        {
            internal int initialized;
            internal IntPtr error;
            internal IntPtr walletAddress;
        }


        // ========================================================
        // Native Response Types
        // ========================================================

        /// <summary>
        /// Native base response layout shared by PlayTradeX responses.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct BaseResponse
        {
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;
        }


        /// <summary>
        /// Native response returned by a native blockchain balance query.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeBalanceResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr balance;
        }


        /// <summary>
        /// Native response returned by a blockchain transaction operation.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct TransactionResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr transactionHash;
            internal IntPtr receipt;
        }

        // ========================================================
        // Native Transaction Event Types
        // ========================================================

        /// <summary>
        /// Terminal mined state reported by the native SDK.
        /// Values must match CPlayTradeXTransactionEventStatus.
        /// </summary>
        internal enum TransactionEventStatus
        {
            Confirmed = 0,
            Reverted = 1
        }


        /// <summary>
        /// Native mined transaction event.
        ///
        /// Field order must exactly match
        /// CPlayTradeXTransactionEvent.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct TransactionEvent
        {
            internal IntPtr transactionId;
            internal IntPtr networkId;

            internal ulong chainId;

            internal IntPtr transactionHash;

            internal TransactionEventStatus status;

            internal IntPtr receipt;
        }


        /// <summary>
        /// Native response returned by human-readable ABI conversion.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HumanReadableAbiResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr abi;
        }


        /// <summary>
        /// Native response returned by a read-only smart contract call.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct ContractReadResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr data;
        }


        /// <summary>
        /// Native response returned when exporting the PlayTradeX
        /// identity wallet.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WalletExportResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr walletAddress;
            internal IntPtr filePath;
        }


        /// <summary>
        /// Native response returned when importing a PlayTradeX
        /// identity wallet.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WalletImportResponse
        {
            // BaseResponse layout.
            internal int success;
            internal int errorCode;
            internal IntPtr body;
            internal IntPtr errorMessage;

            internal IntPtr walletAddress;
        }


        // ========================================================
        // Native Transaction Types
        // ========================================================

        /// <summary>
        /// Native prepared transaction supplied through the
        /// transaction-consent callback.
        /// </summary>
        /// <remarks>
        /// Field order must exactly match CPlayTradeXPreparedTransaction.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct PreparedTransaction
        {
            internal IntPtr id;

            // Network + signer context.
            internal IntPtr networkId;
            internal ulong chainId;
            internal IntPtr fromAddress;

            // RPC selected during preparation.
            internal IntPtr rpc;

            // Transaction target / contract information.
            internal IntPtr contractAddress;
            internal IntPtr abi;
            internal IntPtr functionName;
            internal IntPtr params_;
            internal IntPtr value;

            // Gas information.
            internal ulong gasLimit;

            internal IntPtr baseFeePerGas;
            internal IntPtr maxPriorityFeePerGas;
            internal IntPtr maxFeePerGas;
            internal IntPtr estimatedMaxNetworkFee;

            // Simulation information.
            internal int simulationSucceeded;
            internal IntPtr simulationError;

            // Submission readiness.
            internal int canSubmit;
            internal IntPtr preparationError;
        }


        // ========================================================
        // Native Callback Types
        // ========================================================

        /// <summary>
        /// Callback invoked when standalone wallet generation completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void GenerateWalletCallback(
            GenerateWalletResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when SDK initialization completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void InitializeCallback(
            InitializeResult result,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when a native balance query completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NativeBalanceCallback(
            NativeBalanceResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when a blockchain transaction operation completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TransactionCallback(
            TransactionResponse response,
            IntPtr userData);

        /// <summary>
        /// Callback invoked when a submitted transaction reaches
        /// a terminal mined state.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TransactionEventCallback(
            TransactionEvent transactionEvent,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when ABI conversion completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void HumanReadableAbiCallback(
            HumanReadableAbiResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when a read-only contract call completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void ContractReadCallback(
            ContractReadResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when identity-wallet export completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WalletExportCallback(
            WalletExportResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when identity-wallet import completes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WalletImportCallback(
            WalletImportResponse response,
            IntPtr userData);


        /// <summary>
        /// Callback invoked when a prepared transaction requires
        /// application/user consent.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TransactionConsentCallback(
            PreparedTransaction transaction,
            IntPtr userData);


        /// <summary>
        /// Callback used by the native logging bridge.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void LogCallback(
            IntPtr message,
            int type,
            IntPtr userData);
    }
}