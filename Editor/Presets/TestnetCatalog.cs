#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PlayTradeX.Editor
{
    /// <summary>
    /// Loads and provides access to the built-in PlayTradeX
    /// EVM testnet preset catalog.
    /// </summary>
    internal static class TestnetCatalog
    {
        // ============================================================
        // Constants
        // ============================================================

        private const string PackageName =
            "com.playtradex.sdk";

        private const string CatalogRelativePath =
            "Editor/Presets/evm-testnets.json";


        // ============================================================
        // Cached State
        // ============================================================

        private static TestnetCatalogData _catalog;

        private static string _catalogPath;


        // ============================================================
        // Public API
        // ============================================================

        /// <summary>
        /// Gets all testnet presets contained in the catalog.
        /// </summary>
        internal static IReadOnlyList<TestnetPreset> GetAll()
        {
            EnsureLoaded();

            if (_catalog == null ||
                _catalog.networks == null)
            {
                return Array.Empty<TestnetPreset>();
            }

            return _catalog.networks;
        }


        /// <summary>
        /// Finds a testnet preset by its unique PlayTradeX ID.
        /// </summary>
        internal static TestnetPreset GetById(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            IReadOnlyList<TestnetPreset> networks =
                GetAll();

            for (int i = 0;
                 i < networks.Count;
                 ++i)
            {
                TestnetPreset preset =
                    networks[i];

                if (preset == null)
                {
                    continue;
                }

                if (string.Equals(
                        preset.id,
                        id,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return preset;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns true when the catalog contains a preset
        /// with the supplied ID.
        /// </summary>
        internal static bool Contains(
            string id)
        {
            return GetById(id) != null;
        }


        /// <summary>
        /// Forces the catalog to be loaded again from disk.
        /// Useful while developing/editing the preset JSON.
        /// </summary>
        internal static void Reload()
        {
            _catalog = null;
            _catalogPath = null;

            EnsureLoaded();
        }


        /// <summary>
        /// Gets the loaded catalog version.
        /// </summary>
        internal static string GetCatalogVersion()
        {
            EnsureLoaded();

            if (_catalog == null ||
                string.IsNullOrWhiteSpace(
                    _catalog.catalogVersion))
            {
                return "Unknown";
            }

            return _catalog.catalogVersion;
        }


        /// <summary>
        /// Gets the schema version of the loaded catalog.
        /// </summary>
        internal static int GetSchemaVersion()
        {
            EnsureLoaded();

            return _catalog != null
                ? _catalog.schemaVersion
                : 0;
        }


        // ============================================================
        // Loading
        // ============================================================

        private static void EnsureLoaded()
        {
            if (_catalog != null)
            {
                return;
            }

            string path =
                ResolveCatalogPath();

            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError(
                    "[PlayTradeX] Unable to locate " +
                    "evm-testnets.json.");

                _catalog =
                    CreateEmptyCatalog();

                return;
            }

            if (!File.Exists(path))
            {
                Debug.LogError(
                    $"[PlayTradeX] Testnet catalog does not exist: '{path}'.");

                _catalog =
                    CreateEmptyCatalog();

                return;
            }

            try
            {
                string json =
                    File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogError(
                        "[PlayTradeX] Testnet catalog is empty.");

                    _catalog =
                        CreateEmptyCatalog();

                    return;
                }

                TestnetCatalogData catalog =
                    JsonUtility.FromJson<TestnetCatalogData>(
                        json);

                if (catalog == null)
                {
                    Debug.LogError(
                        "[PlayTradeX] Failed to deserialize " +
                        "the testnet catalog.");

                    _catalog =
                        CreateEmptyCatalog();

                    return;
                }

                if (catalog.networks == null)
                {
                    catalog.networks =
                        new List<TestnetPreset>();
                }

                _catalog =
                    catalog;

                _catalogPath =
                    path;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[PlayTradeX] Failed to load testnet catalog.\n" +
                    exception);

                _catalog =
                    CreateEmptyCatalog();
            }
        }


        // ============================================================
        // Package Path Resolution
        // ============================================================

        private static string ResolveCatalogPath()
        {
            if (!string.IsNullOrWhiteSpace(
                    _catalogPath))
            {
                return _catalogPath;
            }

            UnityEditor.PackageManager.PackageInfo[] packages =
                UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

            if (packages == null)
            {
                return null;
            }

            for (int i = 0;
                 i < packages.Length;
                 ++i)
            {
                UnityEditor.PackageManager.PackageInfo package =
                    packages[i];

                if (package == null)
                {
                    continue;
                }

                if (!string.Equals(
                        package.name,
                        PackageName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        package.resolvedPath))
                {
                    return null;
                }

                _catalogPath =
                    Path.Combine(
                        package.resolvedPath,
                        CatalogRelativePath);

                return _catalogPath;
            }

            return null;
        }


        // ============================================================
        // Helpers
        // ============================================================

        private static TestnetCatalogData CreateEmptyCatalog()
        {
            return new TestnetCatalogData
            {
                schemaVersion = 0,
                catalogVersion = "Unknown",
                networks =
                    new List<TestnetPreset>()
            };
        }
    }


    // ================================================================
    // JSON Models
    // ================================================================

    [Serializable]
    internal sealed class TestnetCatalogData
    {
        public int schemaVersion;

        public string catalogVersion;

        public List<TestnetPreset> networks =
            new List<TestnetPreset>();
    }


    [Serializable]
    internal sealed class TestnetPreset
    {
        public string id;

        public string networkName;

        public string ecosystem;

        public ulong chainId;

        public string symbol;

        public List<string> rpcUrls =
            new List<string>();

        public string blockExplorerUrl;

        public bool isTestnet;
    }
}

#endif