using System;
using System.Collections.Generic;

using UnityEngine;


namespace PlayTradeX
{
    /// <summary>
    /// Stores the project-level PlayTradeX configuration used by the
    /// Unity integration.
    /// </summary>
    /// <remarks>
    /// This settings object is the single Unity-facing configuration
    /// source for PlayTradeX.
    ///
    /// Unity developers configure blockchain networks and
    /// application-managed wallets through:
    ///
    /// Project Settings > PlayTradeX
    ///
    /// NetworkConfigSettings contains both SDK configuration and
    /// Unity-specific metadata. Before SDK initialization, configured
    /// networks are converted into the native-facing NetworkConfig
    /// representation.
    ///
    /// WalletConfigSettings contains application-managed wallets that
    /// may either be entered manually or generated through PlayTradeX.
    ///
    /// Application-managed wallets are separate from the SDK-managed
    /// PlayTradeX identity wallet.
    /// </remarks>
    public sealed class PlayTradeXSettings : ScriptableObject
    {
        // ========================================================
        // Constants
        // ========================================================

        /// <summary>
        /// Resources path used to load the PlayTradeX settings asset.
        /// </summary>
        public const string ResourcesPath =
            "PlayTradeXSettings";

        // ========================================================
        // Storage
        // ========================================================

        [SerializeField]
        private string storagePath = "";


        /// <summary>
        /// Gets the custom PlayTradeX storage path configured for
        /// the project.
        /// </summary>
        /// <remarks>
        /// An empty value indicates that Unity's
        /// Application.persistentDataPath should be used.
        /// </remarks>
        public string StoragePath =>
            storagePath;


        // ========================================================
        // Network Configuration
        // ========================================================

        [Tooltip(
            "Blockchain networks available to the PlayTradeX SDK.")]
        [SerializeField]
        private List<NetworkConfigSettings> networks =
            new List<NetworkConfigSettings>();


        // ========================================================
        // Wallet Configuration
        // ========================================================

        [Tooltip(
            "Application-managed wallets available to PlayTradeX.")]
        [SerializeField]
        private List<WalletConfigSettings> wallets =
            new List<WalletConfigSettings>();


        // ========================================================
        // Singleton
        // ========================================================

        private static PlayTradeXSettings _instance;


        // ========================================================
        // Static Access
        // ========================================================

        /// <summary>
        /// Gets the PlayTradeX project settings asset.
        /// </summary>
        /// <remarks>
        /// The settings asset is loaded from Resources so that the
        /// configuration remains available to the Unity runtime.
        /// </remarks>
        public static PlayTradeXSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance =
                        Resources.Load<PlayTradeXSettings>(
                            ResourcesPath);
                }


                return _instance;
            }
        }


        /// <summary>
        /// Gets whether a PlayTradeX project settings asset exists.
        /// </summary>
        public static bool Exists =>
            Instance != null;


        // ========================================================
        // Networks
        // ========================================================

        /// <summary>
        /// Gets the networks configured for this Unity project.
        /// </summary>
        public IReadOnlyList<NetworkConfigSettings> Networks =>
            networks;


        /// <summary>
        /// Gets the number of configured networks.
        /// </summary>
        public int NetworkCount =>
            networks?.Count ?? 0;


        /// <summary>
        /// Attempts to find a configured network by its identifier.
        /// </summary>
        /// <param name="networkId">
        /// Network identifier configured in Project Settings.
        /// </param>
        /// <param name="network">
        /// Receives the matching network configuration when found.
        /// </param>
        /// <returns>
        /// True when the network was found; otherwise false.
        /// </returns>
        public bool TryGetNetwork(
            string networkId,
            out NetworkConfigSettings network)
        {
            network =
                null;


            if (string.IsNullOrWhiteSpace(
                    networkId))
            {
                return false;
            }


            if (networks == null)
            {
                return false;
            }


            for (int i = 0;
                 i < networks.Count;
                 ++i)
            {
                NetworkConfigSettings candidate =
                    networks[i];


                if (candidate == null)
                {
                    continue;
                }


                if (string.Equals(
                        candidate.Id,
                        networkId,
                        StringComparison.Ordinal))
                {
                    network =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// Attempts to get a configured network by index.
        /// </summary>
        public bool TryGetNetwork(
            int index,
            out NetworkConfigSettings network)
        {
            network =
                null;


            if (networks == null ||
                index < 0 ||
                index >= networks.Count)
            {
                return false;
            }


            network =
                networks[index];


            return network != null;
        }


        // ========================================================
        // Network Conversion
        // ========================================================

        /// <summary>
        /// Creates the native-facing network configurations required
        /// by PlayTradeX SDK initialization.
        /// </summary>
        /// <remarks>
        /// Unity-only metadata such as Symbol and BlockExplorerUrl is
        /// intentionally excluded by NetworkConfigSettings.
        /// ToNetworkConfig().
        /// </remarks>
        public NetworkConfig[] CreateNetworkConfigs()
        {
            if (networks == null ||
                networks.Count == 0)
            {
                return Array.Empty<NetworkConfig>();
            }


            List<NetworkConfig> result =
                new List<NetworkConfig>(
                    networks.Count);


            for (int i = 0;
                 i < networks.Count;
                 ++i)
            {
                NetworkConfigSettings network =
                    networks[i];


                if (network == null)
                {
                    continue;
                }


                result.Add(
                    network.ToNetworkConfig());
            }


            return result.ToArray();
        }


        // ========================================================
        // Wallets
        // ========================================================

        /// <summary>
        /// Gets the application-managed wallets configured for this
        /// Unity project.
        /// </summary>
        public IReadOnlyList<WalletConfigSettings> Wallets =>
            wallets;


        /// <summary>
        /// Gets the number of configured application-managed wallets.
        /// </summary>
        public int WalletCount =>
            wallets?.Count ?? 0;


        /// <summary>
        /// Attempts to find an application-managed wallet by its
        /// developer-defined identifier.
        /// </summary>
        /// <param name="walletId">
        /// Wallet identifier configured in Project Settings.
        /// </param>
        /// <param name="wallet">
        /// Receives the matching wallet configuration when found.
        /// </param>
        /// <returns>
        /// True when the wallet was found; otherwise false.
        /// </returns>
        public bool TryGetWallet(
            string walletId,
            out WalletConfigSettings wallet)
        {
            wallet =
                null;


            if (string.IsNullOrWhiteSpace(
                    walletId))
            {
                return false;
            }


            if (wallets == null)
            {
                return false;
            }


            for (int i = 0;
                 i < wallets.Count;
                 ++i)
            {
                WalletConfigSettings candidate =
                    wallets[i];


                if (candidate == null)
                {
                    continue;
                }


                if (string.Equals(
                        candidate.Id,
                        walletId,
                        StringComparison.Ordinal))
                {
                    wallet =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// Attempts to find an application-managed wallet by its EVM
        /// address.
        /// </summary>
        public bool TryGetWalletByAddress(
            string address,
            out WalletConfigSettings wallet)
        {
            wallet =
                null;


            if (string.IsNullOrWhiteSpace(
                    address))
            {
                return false;
            }


            if (wallets == null)
            {
                return false;
            }


            for (int i = 0;
                 i < wallets.Count;
                 ++i)
            {
                WalletConfigSettings candidate =
                    wallets[i];


                if (candidate == null)
                {
                    continue;
                }


                if (string.Equals(
                        candidate.Address,
                        address,
                        StringComparison.OrdinalIgnoreCase))
                {
                    wallet =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// Attempts to get an application-managed wallet by index.
        /// </summary>
        public bool TryGetWallet(
            int index,
            out WalletConfigSettings wallet)
        {
            wallet =
                null;


            if (wallets == null ||
                index < 0 ||
                index >= wallets.Count)
            {
                return false;
            }


            wallet =
                wallets[index];


            return wallet != null;
        }
    }
}