using System;
using System.Runtime.InteropServices;

namespace PlayTradeX
{
    /// <summary>
    /// Defines the native C ABI types and callback signatures used
    /// to communicate with the PlayTradeX native library.
    /// </summary>
    /// <remarks>
    /// The structures in this class must remain binary-compatible
    /// with their corresponding CPlayTradeX definitions.
    ///
    /// Do not reorder, remove, or change field types without making
    /// the corresponding change to the native C ABI.
    /// </remarks>
    internal static partial class CPlayTradeXNative
    {
        // ========================================================
        // Native Response Types
        // ========================================================

        [StructLayout(LayoutKind.Sequential)]
        internal struct InitializeResult
        {
            internal int initialized;
            internal IntPtr error;
            internal IntPtr walletAddress;
        }


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

        [StructLayout(LayoutKind.Sequential)]
        internal struct PreparedTransaction
        {
            internal IntPtr id;
            internal IntPtr rpc;
            internal IntPtr contractAddress;
            internal IntPtr abi;
            internal IntPtr functionName;
            internal IntPtr params_;
            internal IntPtr value;

            internal ulong gasLimit;

            internal IntPtr baseFeePerGas;
            internal IntPtr maxPriorityFeePerGas;
            internal IntPtr maxFeePerGas;
            internal IntPtr estimatedMaxNetworkFee;

            internal int simulationSucceeded;
            internal IntPtr simulationError;

            internal int canSubmit;
            internal IntPtr preparationError;
        }


        // ========================================================
        // Native Callback Types
        // ========================================================

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void InitializeCallback(
            InitializeResult result,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NativeBalanceCallback(
            NativeBalanceResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TransactionCallback(
            TransactionResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void HumanReadableAbiCallback(
            HumanReadableAbiResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void ContractReadCallback(
            ContractReadResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WalletExportCallback(
            WalletExportResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WalletImportCallback(
            WalletImportResponse response,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TransactionConsentCallback(
            PreparedTransaction transaction,
            IntPtr userData);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void LogCallback(
            IntPtr message,
            int type,
            IntPtr userData);
    }
}