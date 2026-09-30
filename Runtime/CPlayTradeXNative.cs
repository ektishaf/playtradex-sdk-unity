using System;
using System.Runtime.InteropServices;

namespace PlayTradeX
{
    /// <summary>
    /// Provides the native PlayTradeX C ABI bindings used by the
    /// managed Unity SDK.
    /// </summary>
    /// <remarks>
    /// Every declaration in this class must remain binary-compatible
    /// with the corresponding function exported by CPlayTradeX.h.
    ///
    /// Do not change parameter order, parameter types, calling
    /// convention, or structure layouts independently of the native ABI.
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
        // Wallet Generation
        // ========================================================

        /// <summary>
        /// Generates a standalone EVM wallet.
        /// </summary>
        /// <remarks>
        /// The generated wallet is not persisted by the native SDK
        /// as the PlayTradeX identity wallet.
        /// </remarks>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_GenerateWallet(
            GenerateWalletCallback callback,
            IntPtr userData,
            int sequential);


        // ========================================================
        // SDK Lifecycle
        // ========================================================

        /// <summary>
        /// Initializes the native PlayTradeX SDK using one or more
        /// configured blockchain networks.
        /// </summary>
        /// <param name="storagePath">
        /// Writable persistent storage directory.
        /// </param>
        /// <param name="networks">
        /// Pointer to the first element of an unmanaged NetworkConfig array.
        /// </param>
        /// <param name="networkCount">
        /// Number of NetworkConfig entries.
        /// </param>
        /// <param name="callback">
        /// Initialization completion callback.
        /// </param>
        /// <param name="userData">
        /// Managed callback context.
        /// </param>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Initialize(
            string storagePath,
            IntPtr networks,
            uint networkCount,
            InitializeCallback callback,
            IntPtr userData);


        /// <summary>
        /// Shuts down the native PlayTradeX SDK.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_Shutdown();


        /// <summary>
        /// Returns non-zero when the native SDK is initialized.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CPlayTradeX_IsInitialized();


        // ========================================================
        // Transaction Consent
        // ========================================================

        /// <summary>
        /// Registers or unregisters the native transaction-consent callback.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CPlayTradeX_SetTransactionConsentCallback(
            TransactionConsentCallback callback,
            IntPtr userData);


        /// <summary>
        /// Approves a pending prepared transaction.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern int CPlayTradeX_ApproveTransaction(
            string transactionId);


        /// <summary>
        /// Denies a pending prepared transaction.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern int CPlayTradeX_DenyTransaction(
            string transactionId);


        // ========================================================
        // Native Balance
        // ========================================================

        /// <summary>
        /// Retrieves the native blockchain balance of the SDK-managed
        /// PlayTradeX identity wallet on the selected network.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_GetNativeBalance(
            string networkId,
            NativeBalanceCallback callback,
            IntPtr userData,
            int sequential);


        /// <summary>
        /// Retrieves the native blockchain balance of an arbitrary
        /// EVM address on the selected network.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_GetNativeBalanceForAddress(
            string networkId,
            string address,
            NativeBalanceCallback callback,
            IntPtr userData,
            int sequential);


        // ========================================================
        // Native Currency Transactions
        // ========================================================

        /// <summary>
        /// Sends native blockchain currency using the SDK-managed
        /// PlayTradeX identity wallet.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_SendEth(
            string networkId,
            string to,
            string amount,
            TransactionCallback callback,
            IntPtr userData,
            int sequential);


        /// <summary>
        /// Sends native blockchain currency using an
        /// application-supplied external wallet.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_SendEthWithWallet(
            string networkId,
            string privateKey,
            string to,
            string amount,
            TransactionCallback callback,
            IntPtr userData,
            int sequential);
        // ========================================================
        // Contract Write Operations
        // ========================================================

        /// <summary>
        /// Executes a state-changing contract operation using the
        /// SDK-managed PlayTradeX identity wallet.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Write(
            string networkId,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            TransactionCallback callback,
            IntPtr userData,
            string value,
            int sequential);


        /// <summary>
        /// Executes a state-changing contract operation using an
        /// application-supplied external wallet.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_WriteWithWallet(
            string networkId,
            string privateKey,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            TransactionCallback callback,
            IntPtr userData,
            string value,
            int sequential);


        // ========================================================
        // Contract Read Operations
        // ========================================================

        /// <summary>
        /// Executes a read-only contract call on the selected network.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_Read(
            string networkId,
            string contractAddress,
            string abi,
            string functionName,
            string parameters,
            ContractReadCallback callback,
            IntPtr userData,
            int sequential);

        // ========================================================
        // ABI Utilities
        // ========================================================

        /// <summary>
        /// Converts a contract ABI into the human-readable format
        /// supported by PlayTradeX.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_HumanReadableAbi(
            string abi,
            int minimal,
            HumanReadableAbiCallback callback,
            IntPtr userData);


        // ========================================================
        // Identity Wallet Backup Operations
        // ========================================================

        /// <summary>
        /// Exports the SDK-managed PlayTradeX identity wallet to an
        /// encrypted backup file.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_ExportWallet(
            string password,
            string outputPath,
            WalletExportCallback callback,
            IntPtr userData,
            int sequential);


        /// <summary>
        /// Imports an encrypted PlayTradeX identity-wallet backup.
        /// </summary>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        internal static extern void CPlayTradeX_ImportWallet(
            string password,
            string inputPath,
            WalletImportCallback callback,
            IntPtr userData,
            int sequential);


        // ========================================================
        // Native Logging
        // ========================================================

        /// <summary>
        /// Registers the native logging callback.
        /// </summary>
        /// <remarks>
        /// SetLogCallback is part of the existing native logging
        /// integration and remains separate from the CPlayTradeX
        /// operation surface.
        /// </remarks>
        [DllImport(
            LibraryName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetLogCallback(
            LogCallback callback,
            IntPtr userData);


        // ========================================================
        // Marshalling Helpers
        // ========================================================

        /// <summary>
        /// Copies a null-terminated native ANSI/UTF-8-compatible
        /// string into managed memory.
        /// </summary>
        /// <remarks>
        /// A null native pointer is represented as an empty string.
        ///
        /// This function performs the copy while the native callback
        /// data is still valid.
        /// </remarks>
        internal static string GetString(IntPtr pointer)
        {
            return pointer == IntPtr.Zero
                ? string.Empty
                : Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
        }
    }
}