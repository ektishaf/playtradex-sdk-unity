#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace PlayTradeX.Editor
{
    /// <summary>
    /// Ensures that the required PlayTradeX project folders and
    /// settings asset exist after the package is imported.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayTradeXSettingsInitializer
    {
        // ============================================================
        // Paths
        // ============================================================

        private const string PlayTradeXFolder =
            "Assets/PlayTradeX";

        private const string ResourcesFolder =
            "Assets/PlayTradeX/Resources";

        private const string GeneratedFolder =
            "Assets/PlayTradeX/Generated";

        private const string ContractsFolder =
            "Assets/PlayTradeX/Generated/Contracts";

        private const string NetworksFolder =
            "Assets/PlayTradeX/Generated/Networks";

        private const string WalletsFolder =
            "Assets/PlayTradeX/Generated/Wallets";

        private const string SettingsPath =
            "Assets/PlayTradeX/Resources/PlayTradeXSettings.asset";


        // ============================================================
        // Initialization
        // ============================================================

        static PlayTradeXSettingsInitializer()
        {
            EditorApplication.delayCall += EnsureProjectStructure;
        }


        // ============================================================
        // Project Structure
        // ============================================================

        private static void EnsureProjectStructure()
        {
            bool changed = false;

            // Assets/PlayTradeX
            changed |= EnsureFolder(
                "Assets",
                "PlayTradeX");

            // Assets/PlayTradeX/Resources
            changed |= EnsureFolder(
                PlayTradeXFolder,
                "Resources");

            // Assets/PlayTradeX/Generated
            changed |= EnsureFolder(
                PlayTradeXFolder,
                "Generated");

            // Assets/PlayTradeX/Generated/Contracts
            changed |= EnsureFolder(
                GeneratedFolder,
                "Contracts");

            // Assets/PlayTradeX/Generated/Networks
            changed |= EnsureFolder(
                GeneratedFolder,
                "Networks");

            // Assets/PlayTradeX/Generated/Wallets
            changed |= EnsureFolder(
                GeneratedFolder,
                "Wallets");

            // Settings asset
            changed |= EnsureSettingsAsset();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log(
                    "[PlayTradeX] Project structure initialized.");
            }
        }


        // ============================================================
        // Folder Creation
        // ============================================================

        private static bool EnsureFolder(
            string parentFolder,
            string folderName)
        {
            string path =
                $"{parentFolder}/{folderName}";

            if (AssetDatabase.IsValidFolder(path))
            {
                return false;
            }

            string guid =
                AssetDatabase.CreateFolder(
                    parentFolder,
                    folderName);

            if (string.IsNullOrWhiteSpace(guid))
            {
                Debug.LogError(
                    $"[PlayTradeX] Failed to create folder '{path}'.");

                return false;
            }

            Debug.Log(
                $"[PlayTradeX] Created folder '{path}'.");

            return true;
        }


        // ============================================================
        // Settings Asset
        // ============================================================

        private static bool EnsureSettingsAsset()
        {
            PlayTradeXSettings existing =
                AssetDatabase.LoadAssetAtPath<PlayTradeXSettings>(
                    SettingsPath);

            if (existing != null)
            {
                return false;
            }

            PlayTradeXSettings settings =
                ScriptableObject.CreateInstance<PlayTradeXSettings>();

            AssetDatabase.CreateAsset(
                settings,
                SettingsPath);

            EditorUtility.SetDirty(settings);

            Debug.Log(
                $"[PlayTradeX] Created settings asset '{SettingsPath}'.");

            return true;
        }
    }
}

#endif