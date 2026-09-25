using System;
using System.Runtime.InteropServices;

namespace PlayTradeX
{
    /// <summary>
    /// Provides the native PlayTradeX C ABI bindings used by the
    /// managed Unity SDK.
    /// </summary>
    /// <remarks>
    /// The signatures in this class must remain binary-compatible
    /// with the corresponding CPlayTradeX native API.
    /// </remarks>
    internal static partial class CPlayTradeXNative
    {
        // ========================================================
        // Native Library
        // ========================================================

#if UNITY_IOS && !UNITY_EDITOR
        private const string LibraryName = "__Internal";
#else
        private const string LibraryName = "PlayTradeXSDK";
#endif


        // ========================================================
        // SDK Lifecycle
        // ========================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Initialize(string storagePath, string rpc, long chainId, InitializeCallback callback, IntPtr userData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_Shutdown();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CPlayTradeX_IsInitialized();


        // ========================================================
        // Transaction Consent
        // ========================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_SetTransactionConsentCallback(TransactionConsentCallback callback, IntPtr userData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int CPlayTradeX_ApproveTransaction(string transactionId);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int CPlayTradeX_DenyTransaction(string transactionId);


        // ========================================================
        // Blockchain Operations
        // ========================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_GetNativeBalance(NativeBalanceCallback callback, IntPtr userData, int sequential);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_SendEth(string to, string amount, TransactionCallback callback, IntPtr userData, int sequential);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_HumanReadableAbi(string abi, int minimal, HumanReadableAbiCallback callback, IntPtr userData, int sequential);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Write(string contractAddress, string abi, string functionName, string parameters, TransactionCallback callback, IntPtr userData, string value, int sequential);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Read(string contractAddress, string abi, string functionName, string parameters, ContractReadCallback callback, IntPtr userData, int sequential);


        // ========================================================
        // Wallet Operations
        // ========================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_ExportWallet(string password, string outputPath, WalletExportCallback callback, IntPtr userData, int sequential);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_ImportWallet(string password, string inputPath, WalletImportCallback callback, IntPtr userData, int sequential);


        // ========================================================
        // Native Logging
        // ========================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetLogCallback(LogCallback callback, IntPtr userData);


        // ========================================================
        // Marshalling
        // ========================================================

        /// <summary>
        /// Copies a null-terminated native string into a managed
        /// string. A null native pointer is represented as an empty
        /// string.
        /// </summary>
        internal static string GetString(IntPtr pointer)
        {
            return pointer == IntPtr.Zero
                ? string.Empty
                : Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
        }
    }
}