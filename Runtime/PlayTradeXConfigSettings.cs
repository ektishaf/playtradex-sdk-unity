using System;

using UnityEngine;


namespace PlayTradeX
{
    // ============================================================
    // Network Configuration Settings
    // ============================================================

    /// <summary>
    /// Defines a blockchain network configured by the Unity developer
    /// in Project Settings > PlayTradeX.
    /// </summary>
    /// <remarks>
    /// This is the Unity-facing network configuration model.
    ///
    /// Only values required by the native PlayTradeX SDK are converted
    /// to NetworkConfig when the SDK is initialized.
    ///
    /// Unity-specific metadata such as the native currency symbol and
    /// block explorer URL remains on the managed side.
    /// </remarks>
    [Serializable]
    public sealed class NetworkConfigSettings
    {
        [Tooltip(
            "Unique identifier used to reference this network.")]
        [SerializeField]
        private string id =
            string.Empty;

            [Tooltip(
            "Name of the network.")]
        [SerializeField]
        private string networkName =
            string.Empty;


        [Tooltip(
            "RPC URLs available for this network.")]
        [SerializeField]
        private string[] rpcUrls =
            Array.Empty<string>();


        [Tooltip(
            "EVM chain ID for this network.")]
        [SerializeField]
        private ulong chainId;


        [Tooltip(
            "Native currency symbol, for example ETH, BNB, or POL.")]
        [SerializeField]
        private string symbol =
            string.Empty;


        [Tooltip(
            "Base URL of the network block explorer.")]
        [SerializeField]
        private string blockExplorerUrl =
            string.Empty;


        // ========================================================
        // Public Access
        // ========================================================

        /// <summary>
        /// Gets the unique network identifier.
        /// </summary>
        public string Id =>
            id;

        /// <summary>
        /// Gets the unique network identifier.
        /// </summary>
        public string NetworkName =>
            networkName;


        /// <summary>
        /// Gets the configured RPC URLs.
        /// </summary>
        public string[] RpcUrls =>
            rpcUrls;


        /// <summary>
        /// Gets the EVM chain ID.
        /// </summary>
        public ulong ChainId =>
            chainId;


        /// <summary>
        /// Gets the native currency symbol.
        /// </summary>
        public string Symbol =>
            symbol;


        /// <summary>
        /// Gets the base block explorer URL.
        /// </summary>
        public string BlockExplorerUrl =>
            blockExplorerUrl;


        // ========================================================
        // Native SDK Conversion
        // ========================================================

        /// <summary>
        /// Converts this Unity project configuration into the network
        /// configuration required by the PlayTradeX SDK.
        /// </summary>
        /// <remarks>
        /// Unity-only metadata such as Symbol and BlockExplorerUrl is
        /// intentionally not passed to the native SDK.
        /// </remarks>
        public NetworkConfig ToNetworkConfig()
        {
            return new NetworkConfig(
                id,
                chainId,
                rpcUrls ?? Array.Empty<string>());
        }
    }


    // ============================================================
    // Wallet Configuration Settings
    // ============================================================

    /// <summary>
    /// Defines an application-managed wallet configured by the Unity
    /// developer in Project Settings > PlayTradeX.
    /// </summary>
    /// <remarks>
    /// Wallets may be entered manually or populated through the
    /// Generate Wallet action in Project Settings.
    ///
    /// These wallets are separate from the SDK-managed PlayTradeX
    /// identity wallet.
    /// </remarks>
    [Serializable]
    public sealed class WalletConfigSettings
    {
        [Tooltip(
            "Developer-defined identifier used to reference this wallet.")]
        [SerializeField]
        private string id =
            string.Empty;


        [Tooltip(
            "EVM wallet address.")]
        [SerializeField]
        private string address =
            string.Empty;


        [Tooltip(
            "Private key used to sign transactions with this wallet.")]
        [SerializeField]
        private string privateKey =
            string.Empty;


        // ========================================================
        // Public Access
        // ========================================================

        /// <summary>
        /// Gets the developer-defined wallet identifier.
        /// </summary>
        public string Id =>
            id;


        /// <summary>
        /// Gets the EVM wallet address.
        /// </summary>
        public string Address =>
            address;


        /// <summary>
        /// Gets the wallet private key.
        /// </summary>
        public string PrivateKey =>
            privateKey;
    }
}