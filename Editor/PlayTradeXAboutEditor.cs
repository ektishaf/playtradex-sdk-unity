using UnityEditor;
using UnityEngine;

public class PlayTradeXAboutEditor : PlayTradeXEditorWindow
{
    private Vector2 _scrollPosition;

    [MenuItem("PlayTradeX/About", false, 4)]
    public static void ShowWindow()
    {
        PlayTradeXAboutEditor window =
            CreateInstance<PlayTradeXAboutEditor>();

        window.titleContent =
            new GUIContent("About PlayTradeX");

        window.minSize =
            new Vector2(380f, 500f);

        window.position =
            new Rect(
                Screen.width / 2f - 220f,
                Screen.height / 2f - 300f,
                440f,
                600f);

        window.ShowUtility();
    }

    private void OnGUI()
    {
        if (isLoading)
        {
            DrawLoading();
            return;
        }

        _scrollPosition =
            EditorGUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(28f);

        DrawHeader();

        GUILayout.Space(24f);

        DrawReleaseInformation();

        GUILayout.Space(16f);

        DrawPlatformInformation();

        GUILayout.Space(16f);

        DrawFeatures();

        GUILayout.Space(24f);

        DrawFooter();

        GUILayout.Space(24f);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        GUIStyle titleStyle =
            new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22
            };

        GUIStyle subtitleStyle =
            new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                wordWrap = true
            };

        GUILayout.Label(
            "PlayTradeX SDK",
            titleStyle);

        GUILayout.Space(8f);

        GUILayout.Label(
            "Cross-Platform Blockchain SDK for Games",
            subtitleStyle);

        GUILayout.Space(6f);

        GUILayout.Label(
            "Native EVM integration for Unity",
            subtitleStyle);
    }

    private void DrawReleaseInformation()
    {
        DrawSectionTitle("Release Information");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawInformationRow(
            "Version",
            "0.1.0-alpha");

        DrawInformationRow(
            "Release Channel",
            "Alpha");

        DrawInformationRow(
            "Unity",
            "6000.3+");

        EditorGUILayout.EndVertical();
    }

    private void DrawPlatformInformation()
    {
        DrawSectionTitle("Supported Platforms");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawInformationRow(
            "Windows",
            "x64");

        DrawInformationRow(
            "Android",
            "ARM64 (arm64-v8a)");

        DrawInformationRow(
            "macOS",
            "Planned");

        DrawInformationRow(
            "Linux",
            "Planned");

        DrawInformationRow(
            "iOS",
            "Planned");

        EditorGUILayout.EndVertical();
    }

    private void DrawFeatures()
    {
        DrawSectionTitle("SDK Features");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("• Native EVM wallet integration");
        GUILayout.Label("• Native currency transactions");
        GUILayout.Label("• Smart contract read and write");
        GUILayout.Label("• Human-readable ABI support");
        GUILayout.Label("• ABI Converter");
        GUILayout.Label("• Wallet import and export");
        GUILayout.Label("• Secure Android Keystore storage");
        GUILayout.Label("• Transaction approval flow");
        GUILayout.Label("• Exact blockchain unit conversion");
        GUILayout.Label("• Asynchronous Unity integration");

        EditorGUILayout.EndVertical();
    }

    private void DrawFooter()
    {
        GUIStyle footerStyle =
            new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

        GUIStyle alphaStyle =
            new GUIStyle(EditorStyles.wordWrappedMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

        GUILayout.Label(
            "PlayTradeX © 2026",
            footerStyle);

        GUILayout.Space(8f);

        GUILayout.Label(
            "Alpha software intended for development, integration and testing.",
            alphaStyle);
    }

    private void DrawSectionTitle(string title)
    {
        GUIStyle sectionStyle =
            new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13
            };

        GUILayout.Label(
            title,
            sectionStyle);

        GUILayout.Space(4f);
    }

    private void DrawInformationRow(
        string label,
        string value)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            label,
            EditorStyles.boldLabel,
            GUILayout.Width(130f));

        GUILayout.Label(value);

        EditorGUILayout.EndHorizontal();
    }
}