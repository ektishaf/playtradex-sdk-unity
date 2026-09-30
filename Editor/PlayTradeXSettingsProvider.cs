#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace PlayTradeX.Editor
{
    /// <summary>
    /// Provides the PlayTradeX configuration page under
    /// Project Settings > PlayTradeX.
    /// </summary>
    /// <remarks>
    /// This provider is the central Unity Editor configuration surface
    /// for PlayTradeX networks and application-managed wallets.
    ///
    /// Network and wallet identifiers can be generated into strongly
    /// discoverable C# constant classes for use throughout the game.
    /// </remarks>
    internal sealed class PlayTradeXSettingsProvider :
        SettingsProvider
    {
        // ========================================================
        // Constants
        // ========================================================

        private const string SettingsPath =
            "Project/PlayTradeX";

        private const string SettingsAssetFolder =
            "Assets/PlayTradeX/Resources";

        private const string SettingsAssetPath =
            SettingsAssetFolder + "/PlayTradeXSettings.asset";


        private const string GeneratedFolder =
            "Assets/PlayTradeX/Generated";

        private const string GeneratedNetworksPath =
            GeneratedFolder + "/Networks/GeneratedNetworks.cs";

        private const string GeneratedWalletsPath =
            GeneratedFolder + "/Wallets/GeneratedWallets.cs";


        // ========================================================
        // State
        // ========================================================

        private PlayTradeXSettings _settings;

        private SerializedObject _serializedSettings;

        private SerializedProperty _networksProperty;

        private SerializedProperty _walletsProperty;

        private Vector2 _scrollPosition;

        private bool _generatingWallet;

        private string _statusMessage =
            string.Empty;

        private MessageType _statusMessageType =
            MessageType.Info;


        // ========================================================
        // Construction
        // ========================================================

        private PlayTradeXSettingsProvider(
            string path,
            SettingsScope scope)
            : base(
                path,
                scope)
        {
        }


        // ========================================================
        // Registration
        // ========================================================

        /// <summary>
        /// Registers PlayTradeX under Unity Project Settings.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            PlayTradeXSettingsProvider provider =
                new PlayTradeXSettingsProvider(
                    SettingsPath,
                    SettingsScope.Project);


            provider.label =
                "PlayTradeX";


            provider.keywords =
                new HashSet<string>
                {
                    "PlayTradeX",
                    "Network",
                    "Networks",
                    "RPC",
                    "Chain",
                    "Chain ID",
                    "Symbol",
                    "Explorer",
                    "Block Explorer",
                    "Wallet",
                    "Wallets",
                    "Address",
                    "Private Key",
                    "Ethereum",
                    "EVM",
                    "GeneratedNetworks",
                    "GeneratedWallets"
                };


            return provider;
        }


        // ========================================================
        // Activation
        // ========================================================

        public override void OnActivate(
            string searchContext,
            VisualElement rootElement)
        {
            LoadSettings();
        }


        public override void OnDeactivate()
        {
            _networksProperty =
                null;

            _walletsProperty =
                null;

            _serializedSettings =
                null;

            _settings =
                null;
        }


        // ========================================================
        // GUI
        // ========================================================

        public override void OnGUI(
            string searchContext)
        {
            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null)
            {
                EditorGUILayout.HelpBox(
                    "Unable to load PlayTradeX Project Settings.",
                    MessageType.Error);

                return;
            }


            _serializedSettings.Update();


            DrawHeader();


            EditorGUILayout.Space(
                8);


            DrawStatus();


            _scrollPosition =
                EditorGUILayout.BeginScrollView(
                    _scrollPosition);

            DrawStorage();


            EditorGUILayout.Space(
                20);


            DrawNetworks();


            EditorGUILayout.Space(
                20);


            DrawWallets();


            EditorGUILayout.EndScrollView();


            if (_serializedSettings.ApplyModifiedProperties())
            {
                SaveSettings();
            }
        }


        // ========================================================
        // Header
        // ========================================================

        private static void DrawHeader()
        {
            EditorGUILayout.LabelField(
                "PlayTradeX",
                EditorStyles.boldLabel);


            EditorGUILayout.HelpBox(
                "Configure blockchain networks and application-managed " +
                "wallets used by PlayTradeX.",
                MessageType.Info);
        }


        // ========================================================
        // Status
        // ========================================================

        private void DrawStatus()
        {
            if (string.IsNullOrWhiteSpace(
                    _statusMessage))
            {
                return;
            }


            EditorGUILayout.HelpBox(
                _statusMessage,
                _statusMessageType);


            EditorGUILayout.Space(
                6);
        }


        // ========================================================
        // Networks
        // ========================================================

        private void DrawNetworks()
        {
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.LabelField(
                "Networks",
                EditorStyles.boldLabel);


            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
        "Add Testnet ▼",
        GUILayout.Width(125),
        GUILayout.Height(22)))
            {
                ShowTestnetMenu();
            }


            if (GUILayout.Button(
                    "Add All Testnets",
                    GUILayout.Width(125),
                    GUILayout.Height(22)))
            {
                AddAllTestnets();
            }


            if (GUILayout.Button(
                    "Remove Testnets",
                    GUILayout.Width(125),
                    GUILayout.Height(22)))
            {
                RemoveAllTestnets();
            }
            EditorGUILayout.Space(4);


            // Existing Generate Class button — KEEP

            if (GUILayout.Button(
                    "Generate Class",
                    GUILayout.Width(130),
                    GUILayout.Height(22)))
            {
                GenerateNetworksClass();
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.HelpBox(
                "Configure the blockchain networks available to PlayTradeX. " +
                "Generate Class creates GeneratedNetworks.cs containing " +
                "constants for the configured network IDs.",
                MessageType.None);


            EditorGUILayout.Space(
                4);


            if (_networksProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Unable to locate the serialized networks collection.",
                    MessageType.Error);

                return;
            }


            EditorGUILayout.PropertyField(
                _networksProperty,
                new GUIContent(
                    "Networks"),
                true);
        }

        // ========================================================
        // Testnet Presets
        // ========================================================

        private void ShowTestnetMenu()
        {
            IReadOnlyList<TestnetPreset> presets =
                TestnetCatalog.GetAll();


            if (presets == null ||
                presets.Count == 0)
            {
                SetStatus(
                    "No PlayTradeX testnet presets are available.",
                    MessageType.Warning);

                return;
            }


            GenericMenu menu =
                new GenericMenu();


            for (int i = 0;
                 i < presets.Count;
                 ++i)
            {
                TestnetPreset preset =
                    presets[i];


                if (preset == null ||
                    string.IsNullOrWhiteSpace(
                        preset.id))
                {
                    continue;
                }


                string ecosystem =
                    string.IsNullOrWhiteSpace(
                        preset.ecosystem)
                        ? "Other"
                        : preset.ecosystem.Trim();


                string displayName =
                    string.IsNullOrWhiteSpace(
                        preset.networkName)
                        ? preset.id
                        : preset.networkName.Trim();


                string menuPath =
                    ecosystem +
                    "/" +
                    displayName;


                bool alreadyAdded =
                    ContainsNetwork(
                        preset.id,
                        preset.chainId);


                if (alreadyAdded)
                {
                    menu.AddDisabledItem(
                        new GUIContent(
                            menuPath +
                            "  ✓"));
                }
                else
                {
                    TestnetPreset capturedPreset =
                        preset;


                    menu.AddItem(
                        new GUIContent(
                            menuPath),
                        false,
                        () =>
                        {
                            AddTestnet(
                                capturedPreset);
                        });
                }
            }


            menu.ShowAsContext();
        }


        private void AddTestnet(
            TestnetPreset preset)
        {
            if (preset == null)
            {
                return;
            }


            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _networksProperty == null)
            {
                SetStatus(
                    "PlayTradeX network settings are unavailable.",
                    MessageType.Error);

                return;
            }


            _serializedSettings.Update();


            if (ContainsNetwork(
                    preset.id,
                    preset.chainId))
            {
                SetStatus(
                    $"Testnet '{preset.networkName}' is already configured.",
                    MessageType.Warning);

                return;
            }


            Undo.RecordObject(
                _settings,
                "Add PlayTradeX Testnet");


            int newIndex =
                _networksProperty.arraySize;


            _networksProperty.InsertArrayElementAtIndex(
                newIndex);


            SerializedProperty network =
                _networksProperty.GetArrayElementAtIndex(
                    newIndex);


            if (!PopulateNetworkFromPreset(
                    network,
                    preset))
            {
                _networksProperty.DeleteArrayElementAtIndex(
                    newIndex);


                _serializedSettings.ApplyModifiedProperties();


                SetStatus(
                    $"Unable to add testnet '{preset.networkName}'. " +
                    "NetworkConfigSettings does not contain the expected fields.",
                    MessageType.Error);

                return;
            }


            _serializedSettings.ApplyModifiedProperties();


            SaveSettings();


            SetStatus(
                $"Added testnet: {preset.networkName}",
                MessageType.Info);
        }


        private void AddAllTestnets()
        {
            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _networksProperty == null)
            {
                SetStatus(
                    "PlayTradeX network settings are unavailable.",
                    MessageType.Error);

                return;
            }


            IReadOnlyList<TestnetPreset> presets =
                TestnetCatalog.GetAll();


            if (presets == null ||
                presets.Count == 0)
            {
                SetStatus(
                    "No PlayTradeX testnet presets are available.",
                    MessageType.Warning);

                return;
            }


            _serializedSettings.Update();


            Undo.RecordObject(
                _settings,
                "Add All PlayTradeX Testnets");


            int added =
                0;

            int skipped =
                0;


            for (int i = 0;
                 i < presets.Count;
                 ++i)
            {
                TestnetPreset preset =
                    presets[i];


                if (preset == null ||
                    string.IsNullOrWhiteSpace(
                        preset.id))
                {
                    continue;
                }


                if (ContainsNetwork(
                        preset.id,
                        preset.chainId))
                {
                    ++skipped;

                    continue;
                }


                int newIndex =
                    _networksProperty.arraySize;


                _networksProperty.InsertArrayElementAtIndex(
                    newIndex);


                SerializedProperty network =
                    _networksProperty.GetArrayElementAtIndex(
                        newIndex);


                if (!PopulateNetworkFromPreset(
                        network,
                        preset))
                {
                    _networksProperty.DeleteArrayElementAtIndex(
                        newIndex);

                    continue;
                }


                ++added;
            }


            _serializedSettings.ApplyModifiedProperties();


            SaveSettings();


            SetStatus(
                $"Added {added} testnet(s). " +
                $"{skipped} already configured testnet(s) skipped.",
                MessageType.Info);
        }

        private void RemoveAllTestnets()
        {
            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _networksProperty == null)
            {
                SetStatus(
                    "PlayTradeX network settings are unavailable.",
                    MessageType.Error);

                return;
            }


            _serializedSettings.Update();


            IReadOnlyList<TestnetPreset> presets =
                TestnetCatalog.GetAll();


            if (presets == null ||
                presets.Count == 0)
            {
                SetStatus(
                    "No PlayTradeX testnet presets are available.",
                    MessageType.Warning);

                return;
            }


            HashSet<string> presetIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            HashSet<ulong> presetChainIds =
                new HashSet<ulong>();


            for (int i = 0;
                 i < presets.Count;
                 ++i)
            {
                TestnetPreset preset =
                    presets[i];


                if (preset == null)
                {
                    continue;
                }


                if (!string.IsNullOrWhiteSpace(
                        preset.id))
                {
                    presetIds.Add(
                        preset.id);
                }


                if (preset.chainId > 0)
                {
                    presetChainIds.Add(
                        preset.chainId);
                }
            }


            int removableCount =
                0;


            for (int i = 0;
                 i < _networksProperty.arraySize;
                 ++i)
            {
                SerializedProperty network =
                    _networksProperty.GetArrayElementAtIndex(
                        i);


                if (IsTestnetNetwork(
                        network,
                        presetIds,
                        presetChainIds))
                {
                    ++removableCount;
                }
            }


            if (removableCount == 0)
            {
                SetStatus(
                    "No PlayTradeX testnets are currently configured.",
                    MessageType.Info);

                return;
            }


            bool confirmed =
                EditorUtility.DisplayDialog(
                    "Remove PlayTradeX Testnets",
                    $"Remove {removableCount} configured testnet(s)?\n\n" +
                    "Custom/mainnet network entries will not be removed.",
                    "Remove Testnets",
                    "Cancel");


            if (!confirmed)
            {
                return;
            }


            Undo.RecordObject(
                _settings,
                "Remove PlayTradeX Testnets");


            int removed =
                0;


            /*
             * Iterate backwards because deleting array entries changes
             * the indexes of everything after the deleted element.
             */
            for (int i =
                     _networksProperty.arraySize - 1;
                 i >= 0;
                 --i)
            {
                SerializedProperty network =
                    _networksProperty.GetArrayElementAtIndex(
                        i);


                if (!IsTestnetNetwork(
                        network,
                        presetIds,
                        presetChainIds))
                {
                    continue;
                }


                _networksProperty.DeleteArrayElementAtIndex(
                    i);


                ++removed;
            }


            _serializedSettings.ApplyModifiedProperties();


            SaveSettings();


            SetStatus(
                $"Removed {removed} testnet(s).",
                MessageType.Info);
        }

        private static bool IsTestnetNetwork(
    SerializedProperty network,
    HashSet<string> presetIds,
    HashSet<ulong> presetChainIds)
        {
            if (network == null)
            {
                return false;
            }


            /*
             * First prefer the explicit isTestnet field when the
             * NetworkConfigSettings model contains it.
             */
            SerializedProperty isTestnetProperty =
                network.FindPropertyRelative(
                    "isTestnet");


            if (isTestnetProperty != null &&
                isTestnetProperty.boolValue)
            {
                return true;
            }


            /*
             * Next check presetId when available.
             */
            SerializedProperty presetIdProperty =
                network.FindPropertyRelative(
                    "presetId");


            if (presetIdProperty != null &&
                !string.IsNullOrWhiteSpace(
                    presetIdProperty.stringValue) &&
                presetIds.Contains(
                    presetIdProperty.stringValue))
            {
                return true;
            }


            /*
             * Compatibility with NetworkConfigSettings that does not
             * yet contain isTestnet/presetId.
             */
            SerializedProperty idProperty =
                network.FindPropertyRelative(
                    "id");


            if (idProperty != null &&
                !string.IsNullOrWhiteSpace(
                    idProperty.stringValue) &&
                presetIds.Contains(
                    idProperty.stringValue))
            {
                return true;
            }


            SerializedProperty chainIdProperty =
                network.FindPropertyRelative(
                    "chainId");


            if (chainIdProperty != null &&
                chainIdProperty.longValue > 0 &&
                presetChainIds.Contains(
                    (ulong)chainIdProperty.longValue))
            {
                return true;
            }


            return false;
        }

        private bool ContainsNetwork(
            string id,
            ulong chainId)
        {
            if (_networksProperty == null)
            {
                return false;
            }


            for (int i = 0;
                 i < _networksProperty.arraySize;
                 ++i)
            {
                SerializedProperty network =
                    _networksProperty.GetArrayElementAtIndex(
                        i);


                SerializedProperty idProperty =
                    network.FindPropertyRelative(
                        "id");


                SerializedProperty chainIdProperty =
                    network.FindPropertyRelative(
                        "chainId");


                if (idProperty != null &&
                    !string.IsNullOrWhiteSpace(
                        id) &&
                    string.Equals(
                        idProperty.stringValue,
                        id,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }


                if (chainIdProperty != null &&
                    chainId > 0 &&
                    (ulong)chainIdProperty.longValue ==
                    chainId)
                {
                    return true;
                }
            }


            return false;
        }


        private static bool PopulateNetworkFromPreset(
            SerializedProperty network,
            TestnetPreset preset)
        {
            if (network == null ||
                preset == null)
            {
                return false;
            }


            SerializedProperty idProperty =
                network.FindPropertyRelative(
                    "id");

            SerializedProperty networkNameProperty =
                network.FindPropertyRelative(
                    "networkName");

            SerializedProperty rpcUrlsProperty =
                network.FindPropertyRelative(
                    "rpcUrls");

            SerializedProperty chainIdProperty =
                network.FindPropertyRelative(
                    "chainId");

            SerializedProperty symbolProperty =
                network.FindPropertyRelative(
                    "symbol");

            SerializedProperty blockExplorerProperty =
                network.FindPropertyRelative(
                    "blockExplorerUrl");


            if (idProperty == null ||
                rpcUrlsProperty == null ||
                chainIdProperty == null)
            {
                return false;
            }


            idProperty.stringValue =
                preset.id ?? string.Empty;

            if (networkNameProperty != null)
            {
                networkNameProperty.stringValue =
                    preset.networkName ?? string.Empty;
            }


            chainIdProperty.longValue =
                checked((long)preset.chainId);


            rpcUrlsProperty.ClearArray();


            if (preset.rpcUrls != null)
            {
                for (int i = 0;
                     i < preset.rpcUrls.Count;
                     ++i)
                {
                    string rpc =
                        preset.rpcUrls[i];


                    if (string.IsNullOrWhiteSpace(
                            rpc))
                    {
                        continue;
                    }


                    int rpcIndex =
                        rpcUrlsProperty.arraySize;


                    rpcUrlsProperty.InsertArrayElementAtIndex(
                        rpcIndex);


                    SerializedProperty rpcProperty =
                        rpcUrlsProperty.GetArrayElementAtIndex(
                            rpcIndex);


                    rpcProperty.stringValue =
                        rpc.Trim();
                }
            }


            if (symbolProperty != null)
            {
                symbolProperty.stringValue =
                    preset.symbol ?? string.Empty;
            }


            if (blockExplorerProperty != null)
            {
                blockExplorerProperty.stringValue =
                    preset.blockExplorerUrl ??
                    string.Empty;
            }


            /*
             * Optional fields.
             *
             * These are populated automatically if they exist in
             * NetworkConfigSettings, but their absence does not prevent
             * older PlayTradeX settings models from using the presets.
             */

            SerializedProperty isTestnetProperty =
                network.FindPropertyRelative(
                    "isTestnet");


            if (isTestnetProperty != null)
            {
                isTestnetProperty.boolValue =
                    true;
            }


            SerializedProperty presetIdProperty =
                network.FindPropertyRelative(
                    "presetId");


            if (presetIdProperty != null)
            {
                presetIdProperty.stringValue =
                    preset.id ?? string.Empty;
            }


            return true;
        }

        // ========================================================
        // Storage
        // ========================================================

        private void DrawStorage()
        {
            EditorGUILayout.LabelField(
                "Storage",
                EditorStyles.boldLabel);


            EditorGUILayout.Space(
                4);


            SerializedProperty storagePathProperty =
                _serializedSettings.FindProperty(
                    "storagePath");


            if (storagePathProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Unable to locate the PlayTradeX storage path setting.",
                    MessageType.Error);

                return;
            }


            EditorGUILayout.PropertyField(
                storagePathProperty,
                new GUIContent(
                    "Storage Path"));


            EditorGUILayout.HelpBox(
                "Leave empty to use Application.persistentDataPath.\n\n" +

                "Windows:\n" +
                "You may specify an absolute writable directory, for example:\n" +
                @"C:\MyGame\PlayTradeX" +

                "\n\nAndroid / iOS:\n" +
                "Leaving this empty is recommended. PlayTradeX will use " +
                "Application.persistentDataPath, which resolves to the " +
                "application's platform-specific persistent storage directory.",
                MessageType.Info);
        }


        // ========================================================
        // Network Class Generation
        // ========================================================

        /// <summary>
        /// Generates the global GeneratedNetworks class from the
        /// currently configured network identifiers.
        /// </summary>
        private void GenerateNetworksClass()
        {
            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _networksProperty == null)
            {
                SetStatus(
                    "PlayTradeX network settings are unavailable.",
                    MessageType.Error);

                return;
            }


            /*
             * Commit any pending Project Settings changes before
             * generating source code.
             */
            _serializedSettings.ApplyModifiedProperties();


            if (!ValidateNetworks(
                    out string validationError))
            {
                SetStatus(
                    validationError,
                    MessageType.Error);

                return;
            }


            try
            {
                EnsureFolderExists(
                    GeneratedFolder);


                StringBuilder builder =
                    CreateGeneratedFileHeader();


                builder.AppendLine(
                    "public static class GeneratedNetworks");

                builder.AppendLine(
                    "{");


                HashSet<string> generatedNames =
                    new HashSet<string>(
                        StringComparer.Ordinal);


                int generatedCount =
                    0;


                for (int i = 0;
                     i < _networksProperty.arraySize;
                     ++i)
                {
                    SerializedProperty network =
                        _networksProperty.GetArrayElementAtIndex(
                            i);


                    SerializedProperty idProperty =
                        network.FindPropertyRelative(
                            "id");


                    if (idProperty == null)
                    {
                        continue;
                    }


                    string networkId =
                        idProperty.stringValue?.Trim();


                    if (string.IsNullOrWhiteSpace(
                            networkId))
                    {
                        continue;
                    }


                    string constantName =
                        ToPascalCaseIdentifier(
                            networkId);


                    if (!generatedNames.Add(
                            constantName))
                    {
                        SetStatus(
                            $"Cannot generate GeneratedNetworks.cs. " +
                            $"Multiple network IDs resolve to the C# " +
                            $"identifier '{constantName}'.",
                            MessageType.Error);

                        return;
                    }


                    builder.Append(
                        "    public const string ");

                    builder.Append(
                        constantName);

                    builder.Append(
                        " = \"");

                    builder.Append(
                        EscapeStringLiteral(
                            networkId));

                    builder.AppendLine(
                        "\";");


                    ++generatedCount;
                }


                builder.AppendLine(
                    "}");


                RecreateGeneratedFile(
                    GeneratedNetworksPath,
                    builder.ToString());


                SetStatus(
                    $"Generated GeneratedNetworks.cs with " +
                    $"{generatedCount} network constant(s).",
                    MessageType.Info);


                Debug.Log(
                    "[PlayTradeX] Generated network constants:\n" +
                    GeneratedNetworksPath);
            }
            catch (Exception exception)
            {
                SetStatus(
                    "Failed to generate GeneratedNetworks.cs: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
        }


        // ========================================================
        // Network Validation
        // ========================================================

        /// <summary>
        /// Validates network configuration before source generation.
        /// </summary>
        private bool ValidateNetworks(
            out string error)
        {
            error =
                string.Empty;


            if (_networksProperty.arraySize == 0)
            {
                error =
                    "No networks are configured.";

                return false;
            }


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);


            HashSet<string> generatedNames =
                new HashSet<string>(
                    StringComparer.Ordinal);


            for (int i = 0;
                 i < _networksProperty.arraySize;
                 ++i)
            {
                SerializedProperty network =
                    _networksProperty.GetArrayElementAtIndex(
                        i);


                SerializedProperty idProperty =
                    network.FindPropertyRelative(
                        "id");

                SerializedProperty rpcUrlsProperty =
                    network.FindPropertyRelative(
                        "rpcUrls");

                SerializedProperty chainIdProperty =
                    network.FindPropertyRelative(
                        "chainId");


                if (idProperty == null ||
                    rpcUrlsProperty == null ||
                    chainIdProperty == null)
                {
                    error =
                        $"Network at index {i} does not contain the " +
                        "expected id, rpcUrls and chainId fields.";

                    return false;
                }


                string networkId =
                    idProperty.stringValue?.Trim();


                if (string.IsNullOrWhiteSpace(
                        networkId))
                {
                    error =
                        $"Network at index {i} has no ID.";

                    return false;
                }


                if (!ids.Add(
                        networkId))
                {
                    error =
                        $"Duplicate Network ID: {networkId}";

                    return false;
                }


                string generatedName =
                    ToPascalCaseIdentifier(
                        networkId);


                if (!generatedNames.Add(
                        generatedName))
                {
                    error =
                        $"Network ID '{networkId}' produces duplicate " +
                        $"generated identifier '{generatedName}'.";

                    return false;
                }


                if (chainIdProperty.longValue <= 0)
                {
                    error =
                        $"Network '{networkId}' has an invalid Chain ID.";

                    return false;
                }


                if (!rpcUrlsProperty.isArray ||
                    rpcUrlsProperty.arraySize == 0)
                {
                    error =
                        $"Network '{networkId}' requires at least one RPC URL.";

                    return false;
                }


                for (int rpcIndex = 0;
                     rpcIndex < rpcUrlsProperty.arraySize;
                     ++rpcIndex)
                {
                    SerializedProperty rpcProperty =
                        rpcUrlsProperty.GetArrayElementAtIndex(
                            rpcIndex);


                    if (string.IsNullOrWhiteSpace(
                            rpcProperty.stringValue))
                    {
                        error =
                            $"Network '{networkId}' contains an empty " +
                            $"RPC URL at index {rpcIndex}.";

                        return false;
                    }
                }
            }


            return true;
        }


        // ========================================================
        // Wallets
        // ========================================================

        private void DrawWallets()
        {
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.LabelField(
                "Wallets",
                EditorStyles.boldLabel);


            GUILayout.FlexibleSpace();


            if (GUILayout.Button(
                    "Generate Class",
                    GUILayout.Width(130),
                    GUILayout.Height(22)))
            {
                GenerateWalletsClass();
            }


            EditorGUILayout.Space(
                4);


            using (new EditorGUI.DisabledScope(
                       _generatingWallet))
            {
                string buttonText =
                    _generatingWallet
                        ? "Generating..."
                        : "Generate Wallet";


                if (GUILayout.Button(
                        buttonText,
                        GUILayout.Width(130),
                        GUILayout.Height(22)))
                {
                    _ = GenerateWalletAsync();
                }
            }


            EditorGUILayout.EndHorizontal();


            EditorGUILayout.HelpBox(
                "Wallets may be entered manually or generated by PlayTradeX. " +
                "Generate Class creates GeneratedWallets.cs containing only " +
                "the configured wallet IDs.",
                MessageType.None);


            EditorGUILayout.Space(
                4);


            if (_walletsProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Unable to locate the serialized wallets collection.",
                    MessageType.Error);

                return;
            }


            EditorGUILayout.PropertyField(
                _walletsProperty,
                new GUIContent(
                    "Wallets"),
                true);
        }


        // ========================================================
        // Wallet Class Generation
        // ========================================================

        /// <summary>
        /// Generates the global GeneratedWallets class containing
        /// constants for configured wallet identifiers.
        /// </summary>
        /// <remarks>
        /// Only wallet identifiers are generated.
        ///
        /// Wallet addresses and private keys are intentionally never
        /// written to generated source code.
        /// </remarks>
        private void GenerateWalletsClass()
        {
            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _walletsProperty == null)
            {
                SetStatus(
                    "PlayTradeX wallet settings are unavailable.",
                    MessageType.Error);

                return;
            }


            _serializedSettings.ApplyModifiedProperties();


            if (!ValidateWalletIds(
                    out string validationError))
            {
                SetStatus(
                    validationError,
                    MessageType.Error);

                return;
            }


            try
            {
                EnsureFolderExists(
                    GeneratedFolder);


                StringBuilder builder =
                    CreateGeneratedFileHeader();


                builder.AppendLine(
                    "public static class GeneratedWallets");

                builder.AppendLine(
                    "{");


                for (int i = 0;
                     i < _walletsProperty.arraySize;
                     ++i)
                {
                    SerializedProperty wallet =
                        _walletsProperty.GetArrayElementAtIndex(
                            i);


                    SerializedProperty idProperty =
                        wallet.FindPropertyRelative(
                            "id");


                    string walletId =
                        idProperty.stringValue.Trim();


                    string constantName =
                        ToPascalCaseIdentifier(
                            walletId);


                    builder.Append(
                        "    public const string ");

                    builder.Append(
                        constantName);

                    builder.Append(
                        " = \"");

                    builder.Append(
                        EscapeStringLiteral(
                            walletId));

                    builder.AppendLine(
                        "\";");
                }


                builder.AppendLine(
                    "}");


                RecreateGeneratedFile(
                    GeneratedWalletsPath,
                    builder.ToString());


                SetStatus(
                    $"Generated GeneratedWallets.cs with " +
                    $"{_walletsProperty.arraySize} wallet constant(s).",
                    MessageType.Info);


                Debug.Log(
                    "[PlayTradeX] Generated wallet constants:\n" +
                    GeneratedWalletsPath);
            }
            catch (Exception exception)
            {
                SetStatus(
                    "Failed to generate GeneratedWallets.cs: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
        }


        // ========================================================
        // Wallet Validation
        // ========================================================

        /// <summary>
        /// Validates wallet identifiers before source generation.
        /// </summary>
        private bool ValidateWalletIds(
            out string error)
        {
            error =
                string.Empty;


            if (_walletsProperty.arraySize == 0)
            {
                error =
                    "No wallets are configured.";

                return false;
            }


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);


            HashSet<string> generatedNames =
                new HashSet<string>(
                    StringComparer.Ordinal);


            for (int i = 0;
                 i < _walletsProperty.arraySize;
                 ++i)
            {
                SerializedProperty wallet =
                    _walletsProperty.GetArrayElementAtIndex(
                        i);


                SerializedProperty idProperty =
                    wallet.FindPropertyRelative(
                        "id");


                if (idProperty == null)
                {
                    error =
                        $"Wallet at index {i} does not contain an ID field.";

                    return false;
                }


                string walletId =
                    idProperty.stringValue?.Trim();


                if (string.IsNullOrWhiteSpace(
                        walletId))
                {
                    error =
                        $"Wallet at index {i} has no ID.";

                    return false;
                }


                if (!ids.Add(
                        walletId))
                {
                    error =
                        $"Duplicate Wallet ID: {walletId}";

                    return false;
                }


                string generatedName =
                    ToPascalCaseIdentifier(
                        walletId);


                if (!generatedNames.Add(
                        generatedName))
                {
                    error =
                        $"Wallet ID '{walletId}' produces duplicate " +
                        $"generated identifier '{generatedName}'.";

                    return false;
                }
            }


            return true;
        }


        // ========================================================
        // Generate Wallet
        // ========================================================

        /// <summary>
        /// Generates a standalone Ethereum wallet and appends it to
        /// the Project Settings wallet array.
        /// </summary>
        private async Task GenerateWalletAsync()
        {
            if (_generatingWallet)
            {
                return;
            }


            EnsureSettingsLoaded();


            if (_settings == null ||
                _serializedSettings == null ||
                _walletsProperty == null)
            {
                SetStatus(
                    "PlayTradeX Project Settings are unavailable.",
                    MessageType.Error);

                return;
            }


            _generatingWallet =
                true;


            SetStatus(
                "Generating wallet...",
                MessageType.Info);


            Repaint();


            try
            {
                GenerateWalletResponse response =
                    await PlayTradeXSdk.GenerateWalletAsync();


                if (response == null)
                {
                    SetStatus(
                        "Wallet generation returned no response.",
                        MessageType.Error);

                    return;
                }


                if (!response.Success)
                {
                    string error =
                        string.IsNullOrWhiteSpace(
                            response.ErrorMessage)
                            ? "Wallet generation failed."
                            : response.ErrorMessage;


                    SetStatus(
                        error,
                        MessageType.Error);

                    return;
                }


                if (response.Wallet == null)
                {
                    SetStatus(
                        "Wallet generation succeeded but no wallet was returned.",
                        MessageType.Error);

                    return;
                }


                if (string.IsNullOrWhiteSpace(
                        response.Wallet.Address))
                {
                    SetStatus(
                        "Generated wallet does not contain an address.",
                        MessageType.Error);

                    return;
                }


                if (string.IsNullOrWhiteSpace(
                        response.Wallet.PrivateKey))
                {
                    SetStatus(
                        "Generated wallet does not contain a private key.",
                        MessageType.Error);

                    return;
                }


                _serializedSettings.Update();


                if (ContainsWalletAddress(
                        response.Wallet.Address))
                {
                    SetStatus(
                        "A wallet with the generated address already exists.",
                        MessageType.Warning);

                    return;
                }


                Undo.RecordObject(
                    _settings,
                    "Generate PlayTradeX Wallet");


                int newIndex =
                    _walletsProperty.arraySize;


                _walletsProperty.InsertArrayElementAtIndex(
                    newIndex);


                SerializedProperty walletProperty =
                    _walletsProperty.GetArrayElementAtIndex(
                        newIndex);


                SerializedProperty idProperty =
                    walletProperty.FindPropertyRelative(
                        "id");

                SerializedProperty addressProperty =
                    walletProperty.FindPropertyRelative(
                        "address");

                SerializedProperty privateKeyProperty =
                    walletProperty.FindPropertyRelative(
                        "privateKey");


                if (idProperty == null ||
                    addressProperty == null ||
                    privateKeyProperty == null)
                {
                    _walletsProperty.DeleteArrayElementAtIndex(
                        newIndex);


                    _serializedSettings.ApplyModifiedProperties();


                    SetStatus(
                        "WalletConfigSettings does not contain the expected " +
                        "id, address and privateKey fields.",
                        MessageType.Error);

                    return;
                }


                idProperty.stringValue =
                    CreateUniqueWalletId();


                addressProperty.stringValue =
                    response.Wallet.Address;


                privateKeyProperty.stringValue =
                    response.Wallet.PrivateKey;


                _serializedSettings.ApplyModifiedProperties();


                SaveSettings();


                SetStatus(
                    $"Wallet generated successfully: " +
                    $"{ShortenAddress(response.Wallet.Address)}",
                    MessageType.Info);
            }
            catch (Exception exception)
            {
                SetStatus(
                    string.IsNullOrWhiteSpace(
                        exception.Message)
                        ? "Wallet generation failed."
                        : exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
            finally
            {
                _generatingWallet =
                    false;


                Repaint();
            }
        }


        // ========================================================
        // Generated Source Helpers
        // ========================================================

        /// <summary>
        /// Creates the common header used by PlayTradeX-generated
        /// source files.
        /// </summary>
        private static StringBuilder CreateGeneratedFileHeader()
        {
            StringBuilder builder =
                new StringBuilder();


            builder.AppendLine(
                "// <auto-generated>");

            builder.AppendLine(
                "// Generated by PlayTradeX Project Settings.");

            builder.AppendLine(
                "// Do not edit this file manually.");

            builder.AppendLine(
                "// Regenerate from Project Settings > PlayTradeX.");

            builder.AppendLine(
                "// </auto-generated>");

            builder.AppendLine();


            return builder;
        }


        /// <summary>
        /// Deletes an existing generated source file and recreates it
        /// with the supplied contents.
        /// </summary>
        private static void RecreateGeneratedFile(
            string path,
            string contents)
        {
            if (File.Exists(
                    path))
            {
                AssetDatabase.DeleteAsset(
                    path);
            }


            File.WriteAllText(
                path,
                contents,
                new UTF8Encoding(false));


            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceUpdate);


            AssetDatabase.Refresh();
        }


        /// <summary>
        /// Converts an arbitrary configured ID into a PascalCase C#
        /// identifier.
        /// </summary>
        /// <remarks>
        /// The original ID is never modified. Only the generated
        /// constant name is transformed.
        ///
        /// Examples:
        ///
        /// bsc-testnet -> BscTestnet
        /// game-wallet -> GameWallet
        /// polygon_amoy -> PolygonAmoy
        /// </remarks>
        private static string ToPascalCaseIdentifier(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return "_";
            }


            StringBuilder builder =
                new StringBuilder(
                    value.Length + 1);


            bool uppercaseNext =
                true;


            for (int i = 0;
                 i < value.Length;
                 ++i)
            {
                char character =
                    value[i];


                if (!char.IsLetterOrDigit(
                        character))
                {
                    uppercaseNext =
                        true;

                    continue;
                }


                if (builder.Length == 0 &&
                    char.IsDigit(
                        character))
                {
                    builder.Append(
                        '_');
                }


                if (uppercaseNext &&
                    char.IsLetter(
                        character))
                {
                    builder.Append(
                        char.ToUpperInvariant(
                            character));

                    uppercaseNext =
                        false;

                    continue;
                }


                builder.Append(
                    character);


                uppercaseNext =
                    false;
            }


            if (builder.Length == 0)
            {
                return "_";
            }


            /*
             * Protect against C# keywords after transformation.
             */
            string identifier =
                builder.ToString();


            if (IsCSharpKeyword(
                    identifier))
            {
                return "_" + identifier;
            }


            return identifier;
        }


        /// <summary>
        /// Determines whether a generated identifier conflicts with
        /// a reserved C# keyword.
        /// </summary>
        private static bool IsCSharpKeyword(
            string value)
        {
            switch (value)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":

                    return true;

                default:

                    return false;
            }
        }


        /// <summary>
        /// Escapes a configured ID for use inside a generated C#
        /// string literal.
        /// </summary>
        private static string EscapeStringLiteral(
            string value)
        {
            if (string.IsNullOrEmpty(
                    value))
            {
                return string.Empty;
            }


            return value
                .Replace(
                    "\\",
                    "\\\\")
                .Replace(
                    "\"",
                    "\\\"");
        }


        // ========================================================
        // Wallet Helpers
        // ========================================================

        private bool ContainsWalletAddress(
            string address)
        {
            if (_walletsProperty == null ||
                string.IsNullOrWhiteSpace(
                    address))
            {
                return false;
            }


            for (int i = 0;
                 i < _walletsProperty.arraySize;
                 ++i)
            {
                SerializedProperty wallet =
                    _walletsProperty.GetArrayElementAtIndex(
                        i);


                SerializedProperty walletAddress =
                    wallet.FindPropertyRelative(
                        "address");


                if (walletAddress == null)
                {
                    continue;
                }


                if (string.Equals(
                        walletAddress.stringValue,
                        address,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// Creates a unique default ID for a newly generated wallet.
        /// </summary>
        private string CreateUniqueWalletId()
        {
            int number =
                1;


            while (true)
            {
                string candidate =
                    $"wallet-{number}";


                if (!ContainsWalletId(
                        candidate))
                {
                    return candidate;
                }


                ++number;
            }
        }


        private bool ContainsWalletId(
            string id)
        {
            if (_walletsProperty == null ||
                string.IsNullOrWhiteSpace(
                    id))
            {
                return false;
            }


            for (int i = 0;
                 i < _walletsProperty.arraySize;
                 ++i)
            {
                SerializedProperty wallet =
                    _walletsProperty.GetArrayElementAtIndex(
                        i);


                SerializedProperty walletId =
                    wallet.FindPropertyRelative(
                        "id");


                if (walletId == null)
                {
                    continue;
                }


                if (string.Equals(
                        walletId.stringValue,
                        id,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }


            return false;
        }


        // ========================================================
        // Settings Loading
        // ========================================================

        private void EnsureSettingsLoaded()
        {
            if (_settings != null &&
                _serializedSettings != null &&
                _networksProperty != null &&
                _walletsProperty != null)
            {
                return;
            }


            LoadSettings();
        }


        private void LoadSettings()
        {
            _settings =
                GetOrCreateSettings();


            if (_settings == null)
            {
                _serializedSettings =
                    null;

                _networksProperty =
                    null;

                _walletsProperty =
                    null;

                return;
            }


            _serializedSettings =
                new SerializedObject(
                    _settings);


            _networksProperty =
                _serializedSettings.FindProperty(
                    "networks");


            _walletsProperty =
                _serializedSettings.FindProperty(
                    "wallets");
        }


        // ========================================================
        // Settings Asset
        // ========================================================

        /// <summary>
        /// Loads the PlayTradeX settings asset or creates it when it
        /// does not yet exist.
        /// </summary>
        private static PlayTradeXSettings GetOrCreateSettings()
        {
            PlayTradeXSettings settings =
                AssetDatabase.LoadAssetAtPath<PlayTradeXSettings>(
                    SettingsAssetPath);


            if (settings != null)
            {
                return settings;
            }


            EnsureFolderExists(
                SettingsAssetFolder);


            settings =
                ScriptableObject.CreateInstance<PlayTradeXSettings>();


            settings.name =
                "PlayTradeXSettings";


            AssetDatabase.CreateAsset(
                settings,
                SettingsAssetPath);


            EditorUtility.SetDirty(
                settings);


            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();


            Debug.Log(
                "[PlayTradeX] Created Project Settings asset at:\n" +
                SettingsAssetPath);


            return settings;
        }


        /// <summary>
        /// Creates an Assets-relative folder hierarchy when it does
        /// not already exist.
        /// </summary>
        private static void EnsureFolderExists(
            string folderPath)
        {
            if (AssetDatabase.IsValidFolder(
                    folderPath))
            {
                return;
            }


            string[] parts =
                folderPath.Split('/');


            if (parts.Length == 0 ||
                !string.Equals(
                    parts[0],
                    "Assets",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Invalid Unity asset folder path: {folderPath}");
            }


            string currentPath =
                "Assets";


            for (int i = 1;
                 i < parts.Length;
                 ++i)
            {
                string nextPath =
                    currentPath +
                    "/" +
                    parts[i];


                if (!AssetDatabase.IsValidFolder(
                        nextPath))
                {
                    AssetDatabase.CreateFolder(
                        currentPath,
                        parts[i]);
                }


                currentPath =
                    nextPath;
            }
        }


        // ========================================================
        // Persistence
        // ========================================================

        private void SaveSettings()
        {
            if (_settings == null)
            {
                return;
            }


            EditorUtility.SetDirty(
                _settings);


            AssetDatabase.SaveAssets();
        }


        // ========================================================
        // Status Helpers
        // ========================================================

        private void SetStatus(
            string message,
            MessageType messageType)
        {
            _statusMessage =
                message ?? string.Empty;


            _statusMessageType =
                messageType;


            Repaint();
        }


        // ========================================================
        // Display Helpers
        // ========================================================

        private static string ShortenAddress(
            string address)
        {
            if (string.IsNullOrWhiteSpace(
                    address))
            {
                return string.Empty;
            }


            if (address.Length <= 10)
            {
                return address;
            }


            return
                address.Substring(
                    0,
                    6) +
                "..." +
                address.Substring(
                    address.Length - 4);
        }
    }
}

#endif