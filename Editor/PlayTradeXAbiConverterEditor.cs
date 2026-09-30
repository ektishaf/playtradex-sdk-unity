using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PlayTradeXAbiConverterEditor : PlayTradeXEditorWindow
{
    private string FileName;
    private string Address;
    private string Abi;
    private string Hbi;

    private bool SaveAbi;
    private bool SaveHumanReadableAbi;
    private bool IsGenerating;

    private Vector2 AbiScroll;
    private Vector2 GlobalScroll;

    private const float MinAbiHeight = 300f;

    [MenuItem("PlayTradeX/ABI Converter", false, 1)]
    public static void ShowWindow()
    {
        PlayTradeXAbiConverterEditor window =
            CreateInstance<PlayTradeXAbiConverterEditor>();

        window.titleContent =
            new GUIContent(
                "ABI Converter",
                "Converts a contract ABI and generates a C# contract interface.");

        window.minSize =
            new Vector2(600f, 650f);

        window.position =
            new Rect(
                Screen.width / 2f - 400f,
                Screen.height / 2f - 400f,
                800f,
                800f);

        window.FileName =
            $"ContractName{UnityEngine.Random.Range(1000, 9999)}";

        window.Address =
            "0x0000000000000000000000000000000000000000";

        window.Abi = string.Empty;
        window.Hbi = string.Empty;

        window.SaveAbi = false;
        window.SaveHumanReadableAbi = false;

        window.ShowUtility();
    }

    private void OnGUI()
    {
        GlobalScroll =
            EditorGUILayout.BeginScrollView(GlobalScroll);

        GUILayout.Space(15f);

        DrawHeader();

        GUILayout.Space(15f);

        DrawAbiInput();

        GUILayout.Space(15f);

        DrawSettings();

        GUILayout.Space(20f);

        DrawActions();

        GUILayout.Space(15f);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        GUIStyle titleStyle =
            new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

        GUIStyle descriptionStyle =
            new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

        GUILayout.Label(
            "Contract ABI Converter",
            titleStyle);

        GUILayout.Space(5f);

        GUILayout.Label(
            "Paste the complete JSON ABI of your smart contract below. " +
            "PlayTradeX will convert it to a human-readable ABI and " +
            "generate a C# contract interface.",
            descriptionStyle);
    }

    private void DrawAbiInput()
    {
        EditorGUILayout.LabelField(
            "Contract ABI",
            EditorStyles.boldLabel);

        GUILayout.Space(5f);

        float availableHeight =
            Mathf.Max(
                300f,
                position.height - 390f);

        GUIStyle abiStyle =
            new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };

        AbiScroll =
            EditorGUILayout.BeginScrollView(
                AbiScroll,
                false,
                true,
                GUILayout.Height(availableHeight));

        Abi =
            EditorGUILayout.TextArea(
                Abi ?? string.Empty,
                abiStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();

        if (string.IsNullOrWhiteSpace(Abi))
        {
            EditorGUILayout.HelpBox(
                "Paste the complete contract JSON ABI into the field above.",
                MessageType.Info);
        }
    }

    private void DrawSettings()
    {
        float previousLabelWidth =
            EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 200f;

        Address =
            EditorGUILayout.TextField(
                "Contract Address",
                Address);

        GUILayout.Space(8f);

        FileName =
            EditorGUILayout.TextField(
                "Save Filename",
                FileName);

        GUILayout.Space(8f);

        SaveAbi =
            EditorGUILayout.Toggle(
                "Save ABI",
                SaveAbi);

        GUILayout.Space(5f);

        SaveHumanReadableAbi =
            EditorGUILayout.Toggle(
                "Save Human Readable ABI",
                SaveHumanReadableAbi);

        EditorGUIUtility.labelWidth =
            previousLabelWidth;
    }

    private void DrawActions()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(IsGenerating);

        if (GUILayout.Button(
            IsGenerating ? "Generating..." : "Generate",
            GUILayout.Height(32f)))
        {
            Generate();
        }

        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button(
            "Cancel",
            GUILayout.Height(32f)))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Generate()
    {
        if (!ValidateInput())
        {
            return;
        }

        try
        {
            if (!Directory.Exists(GeneratedDirectory))
            {
                Directory.CreateDirectory(GeneratedDirectory);
            }

            IsGenerating = true;

            GetHumanReadableAbiAsync(
                Abi,
                response =>
                {
                    IsGenerating = false;

                    if (!response.Success)
                    {
                        Debug.LogError(
                            "[PlayTradeX] Failed to generate human-readable ABI.\n" +
                            $"Error Code: {response.ErrorCode}\n" +
                            $"Message: {response.ErrorMessage}");

                        Repaint();
                        return;
                    }

                    try
                    {
                        string[] humanReadableAbi =
                            JsonConvert.DeserializeObject<string[]>(
                                response.Abi);

                        if (humanReadableAbi == null ||
                            humanReadableAbi.Length == 0)
                        {
                            Debug.LogError(
                                "[PlayTradeX] Human-readable ABI response is empty.");

                            Repaint();
                            return;
                        }

                        Hbi =
                            JsonConvert.SerializeObject(
                                humanReadableAbi,
                                Formatting.Indented);

                        object parsedAbi =
                            JsonConvert.DeserializeObject<object>(Abi);

                        if (parsedAbi == null)
                        {
                            Debug.LogError(
                                "[PlayTradeX] Contract ABI could not be parsed.");

                            Repaint();
                            return;
                        }

                        Abi =
                            JsonConvert.SerializeObject(
                                parsedAbi,
                                Formatting.None);

                        GenerateABI(
                            Hbi,
                            Abi,
                            Address,
                            FileName);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            "[PlayTradeX] Failed to process ABI response.");

                        Debug.LogException(exception);
                    }

                    Repaint();
                });
        }
        catch (Exception exception)
        {
            IsGenerating = false;

            Debug.LogException(exception);

            Repaint();
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Abi))
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX ABI Converter",
                "Please paste a contract ABI.",
                "OK");

            return false;
        }

        try
        {
            object parsed =
                JsonConvert.DeserializeObject<object>(Abi);

            if (parsed == null)
            {
                throw new JsonException();
            }
        }
        catch
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX ABI Converter",
                "The contract ABI is not valid JSON.",
                "OK");

            return false;
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX ABI Converter",
                "Please enter a filename.",
                "OK");

            return false;
        }

        if (!IsValidIdentifier(FileName))
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX ABI Converter",
                "The filename must also be a valid C# class name.",
                "OK");

            return false;
        }

        if (!IsValidEthereumAddress(Address))
        {
            EditorUtility.DisplayDialog(
                "PlayTradeX ABI Converter",
                "Please enter a valid EVM contract address.",
                "OK");

            return false;
        }

        return true;
    }

    /// <summary>
    /// Generates a C# contract interface containing human-readable
    /// Solidity function signatures and optional ABI constants.
    /// </summary>
    /// <param name="hbi">
    /// Human-readable ABI represented as a JSON string array.
    /// </param>
    /// <param name="abi">
    /// Original contract ABI serialized as compact JSON.
    /// </param>
    /// <param name="address">
    /// Contract address.
    /// </param>
    /// <param name="fileName">
    /// Generated C# class and filename.
    /// </param>
    public void GenerateABI(
        string hbi,
        string abi,
        string address,
        string fileName)
    {
        string[] valueArray =
            JsonConvert.DeserializeObject<string[]>(hbi);

        if (valueArray == null ||
            valueArray.Length == 0)
        {
            Debug.LogError(
                "[PlayTradeX] Human-readable ABI is empty.");

            return;
        }

        Dictionary<string, string> dictionary =
            new Dictionary<string, string>();

        for (int i = 0; i < valueArray.Length; i++)
        {
            string raw =
                valueArray[i];

            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            if (!TryCreateFunctionEntry(
                raw,
                out string identifier))
            {
                continue;
            }

            if (dictionary.ContainsKey(identifier))
            {
                Debug.LogWarning(
                    $"[PlayTradeX] Duplicate generated ABI identifier " +
                    $"'{identifier}' was skipped.");

                continue;
            }

            dictionary.Add(
                identifier,
                raw);
        }

        string outputPath =
            Path.Combine(
                $"{GeneratedDirectory}/Contracts",
                fileName + ".cs");

        using (StreamWriter writer =
            new StreamWriter(outputPath))
        {
            writer.WriteLine(
                $"public static class {fileName}");

            writer.WriteLine("{");

            foreach (KeyValuePair<string, string> entry
                in dictionary)
            {
                string escapedValue =
                    entry.Value.Replace(
                        "\"",
                        "\\\"");

                writer.WriteLine(
                    $"\tpublic const string {entry.Key} = " +
                    $"\"{escapedValue}\";");
            }

            writer.WriteLine();

            writer.WriteLine(
                $"\tpublic const string Address = " +
                $"\"{address}\";");

            if (SaveAbi)
            {
                writer.WriteLine();

                writer.WriteLine(
                    $"\tpublic const string ABI = " +
                    $"\"{abi.Replace("\"", "\\\"")}\";");
            }

            if (SaveHumanReadableAbi)
            {
                writer.WriteLine();

                writer.WriteLine(
                    $"\tpublic const string HBI = " +
                    $"@\"{hbi.Replace("\"", "\"\"")}\";");
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();

        Debug.Log(
            $"[PlayTradeX] Contract interface generated: {outputPath}");

        Compile();
    }

    private static bool TryCreateFunctionEntry(
        string raw,
        out string identifier)
    {
        identifier = null;

        string trimmed =
            raw.Trim();

        if (!trimmed.StartsWith(
            "function ",
            StringComparison.Ordinal))
        {
            return false;
        }

        int argumentStartIndex =
            trimmed.IndexOf('(');

        int argumentEndIndex =
            trimmed.IndexOf(
                ')',
                argumentStartIndex + 1);

        if (argumentStartIndex < 0 ||
            argumentEndIndex < argumentStartIndex)
        {
            return false;
        }

        string functionName =
            trimmed.Substring(
                "function ".Length,
                argumentStartIndex - "function ".Length)
            .Trim();

        if (string.IsNullOrWhiteSpace(functionName))
        {
            return false;
        }

        string rawArguments =
            trimmed.Substring(
                argumentStartIndex + 1,
                argumentEndIndex - argumentStartIndex - 1);

        string argumentSuffix = string.Empty;
        int argumentCount = 0;

        if (!string.IsNullOrWhiteSpace(rawArguments))
        {
            string[] arguments =
                rawArguments.Split(',');

            argumentCount =
                arguments.Length;

            for (int i = 0; i < arguments.Length; i++)
            {
                string argument =
                    arguments[i].Trim();

                if (string.IsNullOrWhiteSpace(argument))
                {
                    continue;
                }

                string[] argumentParts =
                    argument.Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

                if (argumentParts.Length == 0)
                {
                    continue;
                }

                string typeName =
                    argumentParts[0]
                        .Replace("[]", "Array")
                        .Replace("[", "_")
                        .Replace("]", string.Empty);

                typeName =
                    CultureInfo.InvariantCulture.TextInfo
                        .ToTitleCase(typeName);

                if (argumentSuffix.Length > 0)
                {
                    argumentSuffix += "_";
                }

                argumentSuffix += typeName;
            }
        }

        identifier =
            $"{functionName}_{argumentCount}";

        if (!string.IsNullOrEmpty(argumentSuffix))
        {
            identifier += "_" + argumentSuffix;
        }

        return true;
    }

    private static bool IsValidEthereumAddress(
        string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        if (address.Length != 42 ||
            !address.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        for (int i = 2; i < address.Length; i++)
        {
            char c = address[i];

            bool isHex =
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F');

            if (!isHex)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidIdentifier(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!(char.IsLetter(value[0]) ||
              value[0] == '_'))
        {
            return false;
        }

        for (int i = 1; i < value.Length; i++)
        {
            if (!(char.IsLetterOrDigit(value[i]) ||
                  value[i] == '_'))
            {
                return false;
            }
        }

        return true;
    }
}