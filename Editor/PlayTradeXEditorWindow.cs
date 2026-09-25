using System;
using UnityEngine;
using UnityEditor;
using PlayTradeX;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER && UNITY_EDITOR
using System.Reflection;
#endif

public class PlayTradeXEditorWindow : EditorWindow
{
    protected string GeneratedDirectory = $"{Application.dataPath}/PlayTradeX/Generated";
    //protected static BlockchainSettings Config;
    protected bool isLoading;
    protected string LoadingText = "Please Wait ...";

    protected virtual void DrawLoading()
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.fontSize = 18;
        Vector2 size = style.CalcSize(new GUIContent(LoadingText));
        float x = (position.width - size.x) / 2;
        float y = (position.height - size.y) / 2;
        GUI.Label(new Rect(x, y, size.x, size.y), LoadingText, style);
    }

    /*protected static void LoadConfig()
    {
        Config = BlockchainSettings.GetOrCreateSettings();
        if (!Config)
        {
            Debug.LogError("Failed to load configuration file.");
        }
    }*/

    protected async void GetHumanReadableAbiAsync(string abi, Action<HumanReadableAbiResponse> callback)
    {
        HumanReadableAbiResponse response = await PlayTradeXSdk.HumanReadableAbiAsync(abi, false);
        callback?.Invoke(response);
    }

    protected void Compile()
    {
#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        CompilationPipeline.RequestScriptCompilation();
#elif UNITY_2017_1_OR_NEWER && UNITY_EDITOR
                var editorAssembly = Assembly.GetAssembly(typeof(Editor));
                var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif

#if UNITY_EDITOR
        //AssetDatabase.Refresh();
#endif
    }
}
