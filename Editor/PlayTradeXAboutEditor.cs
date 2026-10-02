#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


public class PlayTradeXAboutEditor : PlayTradeXEditorWindow
{
    // ============================================================
    // Constants
    // ============================================================

    private const string UpdaterMenuPath =
        "Tools/PlayTradeX/SDK Updater";


    // ============================================================
    // State
    // ============================================================

    private Vector2 _scrollPosition;

    private string _packageVersion =
        "Unknown";

    private string _releaseChannel =
        "Unknown";

    private string _installationSource =
        "Unknown";

    private string _installedRevision =
        "Unknown";


    // ============================================================
    // Menu
    // ============================================================

    [MenuItem("PlayTradeX/About", false, 4)]
    public static void ShowWindow()
    {
        PlayTradeXAboutEditor window =
            CreateInstance<PlayTradeXAboutEditor>();


        window.titleContent =
            new GUIContent(
                "About PlayTradeX");


        window.minSize =
            new Vector2(
                420f,
                560f);


        window.position =
            new Rect(
                Screen.width / 2f - 240f,
                Screen.height / 2f - 320f,
                480f,
                640f);


        window.ShowUtility();
    }


    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void OnEnable()
    {
        LoadPackageInformation();
    }


    // ============================================================
    // Package Information
    // ============================================================

    private void LoadPackageInformation()
    {
        _packageVersion =
            PlayTradeX.Editor.PlayTradeXEditorInfo.GetVersion();


        _releaseChannel =
            PlayTradeX.Editor.PlayTradeXEditorInfo.GetReleaseChannel(
                _packageVersion);


        _installationSource =
            PlayTradeX.Editor.PlayTradeXEditorInfo.GetInstallationSource();


        _installedRevision =
            PlayTradeX.Editor.PlayTradeXEditorInfo.GetInstalledRevision();
    }


    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        if (isLoading)
        {
            DrawLoading();

            return;
        }


        _scrollPosition =
            EditorGUILayout.BeginScrollView(
                _scrollPosition);


        GUILayout.Space(
            28f);


        DrawHeader();


        GUILayout.Space(
            26f);


        DrawReleaseInformation();


        GUILayout.Space(
            18f);


        DrawPlatformInformation();


        GUILayout.Space(
            18f);


        DrawFeatures();


        GUILayout.Space(
            22f);


        DrawResources();


        GUILayout.Space(
            24f);


        DrawFooter();


        GUILayout.Space(
            24f);


        EditorGUILayout.EndScrollView();
    }


    // ============================================================
    // Header
    // ============================================================

    private void DrawHeader()
    {
        GUIStyle titleStyle =
            new GUIStyle(
                EditorStyles.boldLabel)
            {
                alignment =
                    TextAnchor.MiddleCenter,

                fontSize =
                    22
            };


        GUIStyle subtitleStyle =
            new GUIStyle(
                EditorStyles.label)
            {
                alignment =
                    TextAnchor.MiddleCenter,

                fontSize =
                    13,

                wordWrap =
                    true
            };


        GUIStyle versionStyle =
            new GUIStyle(
                EditorStyles.miniLabel)
            {
                alignment =
                    TextAnchor.MiddleCenter,

                fontSize =
                    11
            };


        GUILayout.Label(
            "PlayTradeX SDK",
            titleStyle);


        GUILayout.Space(
            8f);


        GUILayout.Label(
            "Cross-Platform Blockchain SDK for Games",
            subtitleStyle);


        GUILayout.Space(
            5f);


        GUILayout.Label(
            "Native EVM integration for Unity",
            subtitleStyle);


        GUILayout.Space(
            10f);


        GUILayout.Label(
            $"Version {_packageVersion}",
            versionStyle);
    }


    // ============================================================
    // Release Information
    // ============================================================

    private void DrawReleaseInformation()
    {
        DrawSectionTitle(
            "Release Information");


        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox);


        GUILayout.Space(
            4f);


        DrawInformationRow(
            "Version",
            _packageVersion);


        DrawInformationRow(
            "Release Channel",
            _releaseChannel);


        DrawInformationRow(
            "Unity",
            Application.unityVersion);


        DrawInformationRow(
            "Installation",
            _installationSource);


        DrawInformationRow(
            "Revision",
            _installedRevision);


        GUILayout.Space(
            4f);


        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Platforms
    // ============================================================

    private void DrawPlatformInformation()
    {
        DrawSectionTitle(
            "Supported Platforms");


        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox);


        GUILayout.Space(
            4f);


        DrawPlatformRow(
            "Windows",
            "x64",
            true);


        DrawPlatformRow(
            "Android",
            "ARM64 (arm64-v8a)",
            true);


        DrawPlatformRow(
            "macOS",
            "Planned",
            false);


        DrawPlatformRow(
            "Linux",
            "Planned",
            false);


        DrawPlatformRow(
            "iOS",
            "Planned",
            false);


        GUILayout.Space(
            4f);


        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Features
    // ============================================================

    private void DrawFeatures()
    {
        DrawSectionTitle(
            "SDK Features");


        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox);


        GUILayout.Space(
            6f);


        DrawFeature(
            "Multi network EVM integration");


        DrawFeature(
            "SDK managed and external wallets");


        DrawFeature(
            "Native currency balances and transactions");


        DrawFeature(
            "Smart contract read and write operations");


        DrawFeature(
            "Transaction simulation and fee estimation");


        DrawFeature(
            "Transaction consent and approval flow");


        DrawFeature(
            "Human readable ABI support");


        DrawFeature(
            "ABI Converter and generated configuration IDs");


        DrawFeature(
            "Encrypted wallet import and export");


        DrawFeature(
            "Secure platform wallet storage");


        DrawFeature(
            "Exact blockchain unit conversion");


        DrawFeature(
            "Sequential and concurrent request execution");


        DrawFeature(
            "Asynchronous Unity integration");


        GUILayout.Space(
            6f);


        EditorGUILayout.EndVertical();
    }


    private static void DrawFeature(
        string feature)
    {
        EditorGUILayout.BeginHorizontal();


        GUILayout.Space(
            6f);


        GUILayout.Label(
            "•",
            GUILayout.Width(
                14f));


        GUILayout.Label(
            feature,
            EditorStyles.wordWrappedLabel);


        EditorGUILayout.EndHorizontal();


        GUILayout.Space(
            2f);
    }


    // ============================================================
    // Resources
    // ============================================================

    private void DrawResources()
    {
        DrawSectionTitle(
            "Resources");


        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox);


        GUILayout.Space(
            8f);


        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button(
                "Documentation",
                GUILayout.Height(28f)))
        {
            Application.OpenURL(
                PlayTradeX.Editor.PlayTradeXEditorInfo.DocumentationUrl);
        }


        GUILayout.Space(
            4f);


        if (GUILayout.Button(
                "GitHub",
                GUILayout.Height(28f)))
        {
            Application.OpenURL(
                PlayTradeX.Editor.PlayTradeXEditorInfo.RepositoryUrl);
        }


        EditorGUILayout.EndHorizontal();


        GUILayout.Space(
            6f);


        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button(
                "Check for Updates",
                GUILayout.Height(28f)))
        {
            OpenUpdater();
        }


        GUILayout.Space(
            4f);


        if (GUILayout.Button(
                "Report Issue",
                GUILayout.Height(28f)))
        {
            Application.OpenURL(
                PlayTradeX.Editor.PlayTradeXEditorInfo.IssuesUrl);
        }


        EditorGUILayout.EndHorizontal();


        GUILayout.Space(
            8f);


        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Footer
    // ============================================================

    private void DrawFooter()
    {
        GUIStyle footerStyle =
            new GUIStyle(
                EditorStyles.miniLabel)
            {
                alignment =
                    TextAnchor.MiddleCenter
            };


        GUIStyle channelStyle =
            new GUIStyle(
                EditorStyles.wordWrappedMiniLabel)
            {
                alignment =
                    TextAnchor.MiddleCenter
            };


        GUILayout.Label(
            "PlayTradeX © 2026",
            footerStyle);


        GUILayout.Space(
            8f);


        string message =
            GetReleaseDisclaimer();


        if (!string.IsNullOrWhiteSpace(
                message))
        {
            GUILayout.Label(
                message,
                channelStyle);
        }
    }


    // ============================================================
    // Shared GUI
    // ============================================================

    private static void DrawSectionTitle(
        string title)
    {
        GUIStyle sectionStyle =
            new GUIStyle(
                EditorStyles.boldLabel)
            {
                fontSize =
                    13
            };


        GUILayout.Label(
            title,
            sectionStyle);


        GUILayout.Space(
            4f);
    }

    private static void DrawInformationRow(
    string label,
    string value)
    {
        string displayValue =
            string.IsNullOrWhiteSpace(value)
                ? "Unknown"
                : value;

        Rect rowRect =
            EditorGUILayout.GetControlRect(
                false,
                20f);

        Rect labelRect =
            new Rect(
                rowRect.x,
                rowRect.y,
                140f,
                rowRect.height);

        Rect valueRect =
            new Rect(
                rowRect.x + 145f,
                rowRect.y,
                Mathf.Max(50f, rowRect.width - 145f),
                rowRect.height);

        EditorGUI.LabelField(
            labelRect,
            label,
            EditorStyles.boldLabel);

        EditorGUI.SelectableLabel(
            valueRect,
            displayValue,
            EditorStyles.label);
    }


    private static void DrawPlatformRow(
        string platform,
        string architecture,
        bool supported)
    {
        EditorGUILayout.BeginHorizontal();


        GUILayout.Label(
            supported
                ? "✓"
                : "○",
            GUILayout.Width(20f));


        GUILayout.Label(
            platform,
            EditorStyles.boldLabel,
            GUILayout.Width(120f));


        GUILayout.Label(
            architecture);


        EditorGUILayout.EndHorizontal();


        GUILayout.Space(
            2f);
    }


    // ============================================================
    // Release Disclaimer
    // ============================================================

    private string GetReleaseDisclaimer()
    {
        switch (_releaseChannel)
        {
            case "Alpha":

                return
                    "Alpha software intended for development, " +
                    "integration and testing.";


            case "Beta":

                return
                    "Beta software intended for development, " +
                    "integration and pre-release testing.";


            case "Release Candidate":

                return
                    "Release candidate intended for final integration " +
                    "and validation before stable release.";


            case "Stable":

                return
                    "Stable PlayTradeX SDK release.";


            default:

                return string.Empty;
        }
    }


    // ============================================================
    // Updater
    // ============================================================

    private static void OpenUpdater()
    {
        bool opened =
            EditorApplication.ExecuteMenuItem(
                UpdaterMenuPath);


        if (!opened)
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX SDK Updater",
                "The PlayTradeX SDK Updater could not be opened.",
                "OK");
        }
    }
}

#endif