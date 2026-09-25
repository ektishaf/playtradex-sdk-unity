using System;
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
    /// Add this component to a GameObject in the application's
    /// startup scene.
    /// </remarks>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayTradeXLifecycle : MonoBehaviour
    {
        // ========================================================
        // Inspector
        // ========================================================

        [Header("Initialization")]

        [Tooltip(
            "Automatically initialize PlayTradeX when the application starts.")]
        [SerializeField]
        private bool autoInitialize = true;


        [Tooltip("Blockchain RPC endpoint used by PlayTradeX.")]
        [SerializeField]
        private string rpc;


        [Tooltip("Blockchain chain ID.")]
        [SerializeField]
        private long chainId;


        [Header("Storage")]

        [Tooltip(
            "Leave empty to use Application.persistentDataPath.")]
        [SerializeField]
        private string storagePath = "";

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

            _instance = this;

            // The SDK lifecycle must survive scene transitions.
            DontDestroyOnLoad(gameObject);

            // Register native logging before platform and SDK
            // initialization so initialization errors are captured.
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

            _initializationStarted = true;

            try
            {
                string resolvedStoragePath =
                    string.IsNullOrWhiteSpace(storagePath)
                        ? Application.persistentDataPath
                        : storagePath;

                InitializeResult result =
                    await PlayTradeXSdk.InitializeAsync(
                        resolvedStoragePath,
                        rpc,
                        chainId);

                // The lifecycle object may have been destroyed while
                // native initialization was running.
                if (this == null)
                {
                    return;
                }

                // Shutdown may have been requested while native
                // initialization was running.
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
                    "[PlayTradeX Unity] SDK initialized successfully " +
                    "with wallet address: " +
                    result.WalletAddress);
                
                Ready?.Invoke(result.WalletAddress);
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
                    _initializationStarted = false;
                }
            }
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
            // Destroying a duplicate lifecycle component must not
            // shut down the SDK owned by the active instance.
            if (_instance != this)
            {
                return;
            }

            ShutdownSdk();

            _instance = null;
        }


        // ========================================================
        // SDK Shutdown
        // ========================================================

        private void ShutdownSdk()
        {
            // Unity may invoke both OnApplicationQuit() and
            // OnDestroy(). Native shutdown must only be requested once.
            if (_shutdownRequested)
            {
                return;
            }

            _shutdownRequested = true;

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