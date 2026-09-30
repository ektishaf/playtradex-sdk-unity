using System;
using System.Collections.Generic;
using UnityEngine;


namespace PlayTradeX
{
    /// <summary>
    /// Manages the PlayTradeX SDK lifecycle inside Unity.
    /// </summary>
    /// <remarks>
    /// This component performs the required platform bootstrap,
    /// optionally initializes the SDK automatically, registers native
    /// logging, persists across scene changes, prevents duplicate
    /// lifecycle instances, and shuts down the SDK when the application
    /// or Play Mode exits.
    ///
    /// PlayTradeX supports multiple blockchain networks. Network
    /// configuration is read from PlayTradeX Project Settings and all
    /// configured networks are registered during SDK initialization.
    ///
    /// Wallet selection is intentionally not handled by this component.
    /// The SDK-managed identity wallet and application-managed external
    /// wallets are selected per transaction through the PlayTradeX
    /// transaction APIs.
    ///
    /// Add this component to a GameObject in the application's
    /// startup scene.
    /// </remarks>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayTradeXLifecycle : MonoBehaviour
    {
        // ========================================================
        // Initialization
        // ========================================================

        [Header("Initialization")]

        [Tooltip(
            "Automatically initialize PlayTradeX when the application starts.")]
        [SerializeField]
        private bool autoInitialize = true;

        // ========================================================
        // Events
        // ========================================================

        /// <summary>
        /// Raised after the PlayTradeX SDK has initialized
        /// successfully.
        /// </summary>
        /// <remarks>
        /// The supplied address is the SDK-managed identity wallet
        /// address.
        /// </remarks>
        public static event Action<string> Ready;


        // ========================================================
        // Public State
        // ========================================================

        /// <summary>
        /// Gets whether the PlayTradeX SDK is fully initialized.
        /// </summary>
        public static bool IsInitialized =>
            PlayTradeXSdk.IsInitialized();


        /// <summary>
        /// Gets whether automatic SDK initialization is currently
        /// in progress.
        /// </summary>
        public static bool IsInitializing =>
            _instance != null &&
            _instance._initializationStarted;


        // ========================================================
        // Internal State
        // ========================================================

        private static PlayTradeXLifecycle _instance;

        private bool _initializationStarted;

        private bool _shutdownRequested;


        // ========================================================
        // Platform Bootstrap
        // ========================================================

        /// <summary>
        /// Initializes platform-specific services required by
        /// PlayTradeX before native SDK initialization.
        /// </summary>
        /// <returns>
        /// True when the platform is ready for SDK initialization;
        /// otherwise false.
        /// </returns>
        private static bool InitializePlatform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            try
            {
                using (AndroidJavaClass unityPlayer =
                    new AndroidJavaClass(
                        "com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity =
                        unityPlayer.GetStatic<AndroidJavaObject>(
                            "currentActivity"))
                    {
                        if (activity == null)
                        {
                            Debug.LogError(
                                "[PlayTradeX Unity] Android activity is unavailable.");

                            return false;
                        }


                        using (AndroidJavaObject context =
                            activity.Call<AndroidJavaObject>(
                                "getApplicationContext"))
                        {
                            if (context == null)
                            {
                                Debug.LogError(
                                    "[PlayTradeX Unity] Android application context is unavailable.");

                                return false;
                            }


                            using (AndroidJavaClass playTradeXAndroid =
                                new AndroidJavaClass(
                                    "com.playtradex.PlayTradeXAndroid"))
                            {
                                bool initialized =
                                    playTradeXAndroid.CallStatic<bool>(
                                        "initialize",
                                        context);


                                if (!initialized)
                                {
                                    Debug.LogError(
                                        "[PlayTradeX Unity] Android platform initialization failed.");
                                }


                                return initialized;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[PlayTradeX Unity] Android platform initialization exception: " +
                    exception);

                return false;
            }

#else

            return true;

#endif
        }


        // ========================================================
        // Unity Lifecycle
        // ========================================================

        private void Awake()
        {
            // Only one lifecycle component may own the SDK.
            if (_instance != null &&
                _instance != this)
            {
                Destroy(gameObject);

                return;
            }


            _instance =
                this;


            // The SDK lifecycle must survive scene transitions.
            DontDestroyOnLoad(
                gameObject);


            /*
             * Register native logging before platform and SDK
             * initialization so initialization errors are captured.
             */
            PlayTradeXSdk.SetupLogging();


            if (!InitializePlatform())
            {
                return;
            }


            if (autoInitialize)
            {
                InitializeAutomatically();
            }
        }


        // ========================================================
        // Automatic Initialization
        // ========================================================

        /// <summary>
        /// Loads the PlayTradeX Project Settings, converts the configured
        /// Unity network settings into runtime NetworkConfig instances,
        /// validates them, and initializes the native SDK.
        /// </summary>
        private async void InitializeAutomatically()
        {
            if (_initializationStarted)
            {
                return;
            }


            if (PlayTradeXSdk.IsInitialized())
            {
                return;
            }


            _initializationStarted =
                true;


            try
            {
                // ------------------------------------------------
                // Resolve Project Settings
                // ------------------------------------------------

                PlayTradeXSettings settings =
                    PlayTradeXSettings.Instance;


                if (settings == null)
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] PlayTradeX Project Settings " +
                        "could not be loaded.");

                    return;
                }


                // ------------------------------------------------
                // Create Runtime Network Configuration
                // ------------------------------------------------

                NetworkConfig[] networks =
                    settings.CreateNetworkConfigs();


                /*
                 * Project Settings owns the network configuration.
                 *
                 * NetworkConfigSettings converts only the information
                 * required by the native SDK into NetworkConfig.
                 *
                 * Unity-only metadata such as Symbol and
                 * BlockExplorerUrl remains on the Unity side.
                 */
                if (!ValidateNetworks(
                        networks))
                {
                    return;
                }


                // ------------------------------------------------
                // Resolve Storage
                // ------------------------------------------------

                string resolvedStoragePath =
                    string.IsNullOrWhiteSpace(
                        settings.StoragePath)
                        ? Application.persistentDataPath
                        : settings.StoragePath;


                if (string.IsNullOrWhiteSpace(
                        resolvedStoragePath))
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] Unable to resolve the SDK storage path.");

                    return;
                }


                // ------------------------------------------------
                // Initialize Native SDK
                // ------------------------------------------------

                InitializeResult result =
                    await PlayTradeXSdk.InitializeAsync(
                        resolvedStoragePath,
                        networks);


                /*
                 * The lifecycle object may have been destroyed while
                 * native initialization was running.
                 */
                if (this == null)
                {
                    return;
                }


                /*
                 * Shutdown may have been requested while native
                 * initialization was running.
                 */
                if (_shutdownRequested)
                {
                    return;
                }


                if (!result.Initialized)
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] SDK initialization failed: " +
                        result.Error);

                    return;
                }


                Debug.Log(
                    "[PlayTradeX Unity] SDK initialized successfully.\n" +
                    $"Networks: {networks.Length}\n" +
                    $"Identity Wallet: {result.WalletAddress}");


                Ready?.Invoke(
                    result.WalletAddress);
            }
            catch (Exception exception)
            {
                if (this != null &&
                    !_shutdownRequested)
                {
                    Debug.LogError(
                        "[PlayTradeX Unity] SDK initialization exception: " +
                        exception);
                }
            }
            finally
            {
                if (this != null)
                {
                    _initializationStarted =
                        false;
                }
            }
        }


        // ========================================================
        // Network Validation
        // ========================================================

        /// <summary>
        /// Performs basic validation of the runtime network
        /// configuration before passing it to the native SDK.
        /// </summary>
        /// <param name="networks">
        /// Runtime network configurations created from PlayTradeX
        /// Project Settings.
        /// </param>
        /// <returns>
        /// True when all network configurations contain the minimum
        /// information required for initialization; otherwise false.
        /// </returns>
        /// <remarks>
        /// Native PlayTradeX remains responsible for authoritative
        /// network registration and validation. This validation exists
        /// only to report obvious Unity configuration errors early.
        /// </remarks>
        private static bool ValidateNetworks(
            NetworkConfig[] networks)
        {
            if (networks == null ||
                networks.Length == 0)
            {
                Debug.LogError(
                    "[PlayTradeX Unity] No blockchain networks are configured. " +
                    "Add at least one network in Project Settings > PlayTradeX.");

                return false;
            }


            HashSet<string> networkIds =
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
                    Debug.LogError(
                        $"[PlayTradeX Unity] Network configuration at index {i} is null.");

                    return false;
                }


                if (string.IsNullOrWhiteSpace(
                        network.Id))
                {
                    Debug.LogError(
                        $"[PlayTradeX Unity] Network at index {i} has no network ID.");

                    return false;
                }


                /*
                 * Network IDs identify registered networks throughout
                 * the SDK and therefore must be unique.
                 */
                if (!networkIds.Add(
                        network.Id))
                {
                    Debug.LogError(
                        $"[PlayTradeX Unity] Duplicate network ID '{network.Id}'.");

                    return false;
                }


                if (network.ChainId <= 0)
                {
                    Debug.LogError(
                        $"[PlayTradeX Unity] Network '{network.Id}' has an invalid chain ID.");

                    return false;
                }


                if (network.RpcEndpoints == null ||
                    network.RpcEndpoints.Length == 0)
                {
                    Debug.LogError(
                        $"[PlayTradeX Unity] Network '{network.Id}' has no RPC endpoints.");

                    return false;
                }


                bool hasRpc =
                    false;


                for (int rpcIndex = 0;
                     rpcIndex < network.RpcEndpoints.Length;
                     ++rpcIndex)
                {
                    if (!string.IsNullOrWhiteSpace(
                            network.RpcEndpoints[rpcIndex]))
                    {
                        hasRpc =
                            true;

                        break;
                    }
                }


                if (!hasRpc)
                {
                    Debug.LogError(
                        $"[PlayTradeX Unity] Network '{network.Id}' contains no valid RPC endpoint.");

                    return false;
                }
            }


            return true;
        }


        // ========================================================
        // Application Shutdown
        // ========================================================

        private void OnApplicationQuit()
        {
            ShutdownSdk();
        }


        private void OnDestroy()
        {
            /*
             * Destroying a duplicate lifecycle component must not
             * shut down the SDK owned by the active instance.
             */
            if (_instance != this)
            {
                return;
            }


            ShutdownSdk();


            _instance =
                null;
        }


        // ========================================================
        // SDK Shutdown
        // ========================================================

        /// <summary>
        /// Requests native SDK shutdown once for the active lifecycle.
        /// </summary>
        private void ShutdownSdk()
        {
            /*
             * Unity may invoke both OnApplicationQuit() and
             * OnDestroy(). Native shutdown must only be requested once.
             */
            if (_shutdownRequested)
            {
                return;
            }


            _shutdownRequested =
                true;


            try
            {
                /*
                 * Do not guard Shutdown() only with IsInitialized().
                 *
                 * Unity may exit Play Mode while InitializeAsync() is
                 * still performing native initialization. In that case,
                 * IsInitialized() can still be false even though native
                 * resources and work need to be shut down.
                 *
                 * Native Shutdown() is safe when the SDK is already
                 * uninitialized.
                 *
                 * Shutdown does not cancel blockchain transactions that
                 * have already been submitted. Those transactions remain
                 * subject to normal blockchain processing.
                 */
                PlayTradeXSdk.Shutdown();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[PlayTradeX Unity] SDK shutdown exception: " +
                    exception);
            }
        }
    }
}