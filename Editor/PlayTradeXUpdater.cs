#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using Debug = UnityEngine.Debug;


namespace PlayTradeX.Editor
{
    /// <summary>
    /// PlayTradeX SDK version manager.
    ///
    /// Supports both:
    ///
    /// 1. Local / disk development installations.
    /// 2. Git-installed Unity Package Manager installations.
    ///
    /// Available release versions are discovered from Git tags.
    /// </summary>
    internal sealed class PlayTradeXUpdater : EditorWindow
    {
        // ========================================================
        // Editor Configuration
        // ========================================================

        private const string WindowTitle =
            "PlayTradeX SDK Updater";

        private const string MenuPath =
            "PlayTradeX/SDK Updater";

        private const int GitTimeoutMilliseconds =
            30000;


        // ========================================================
        // Installed Package State
        // ========================================================

        private string _packageVersion =
            string.Empty;

        private string _installedPackageId =
            string.Empty;

        private string _repositoryUrl =
            PlayTradeXEditorInfo.RepositoryGitUrl;

        private string _installedRevision =
            string.Empty;

        private PackageSource _packageSource;

        private bool _packageFound;

        private bool _isLocalInstallation;


        // ========================================================
        // Version State
        // ========================================================

        private readonly List<string> _versions =
            new List<string>();

        private int _selectedVersionIndex;

        private string _latestVersion =
            string.Empty;

        private string _customRevision =
            string.Empty;


        // ========================================================
        // UI State
        // ========================================================

        private bool _loadingPackage;

        private bool _refreshingVersions;

        private bool _updateStarted;

        private Vector2 _scrollPosition;

        private string _statusMessage =
            string.Empty;

        private MessageType _statusMessageType =
            MessageType.Info;


        // ========================================================
        // Package Manager Requests
        // ========================================================

        private ListRequest _listRequest;

        private AddRequest _addRequest;


        // ========================================================
        // Menu
        // ========================================================

        [MenuItem(MenuPath)]
        private static void OpenWindow()
        {
            PlayTradeXUpdater window =
                GetWindow<PlayTradeXUpdater>(
                    true,
                    WindowTitle,
                    true);


            window.minSize =
                new Vector2(
                    580f,
                    560f);


            window.Show();
        }


        // ========================================================
        // Unity Lifecycle
        // ========================================================

        private void OnEnable()
        {
            titleContent =
                new GUIContent(
                    WindowTitle);


            EditorApplication.delayCall +=
                BeginInitialization;
        }


        private void OnDisable()
        {
            EditorApplication.update -=
                PollPackageList;

            EditorApplication.update -=
                PollPackageUpdate;
        }


        // ========================================================
        // Initialization
        // ========================================================

        private void BeginInitialization()
        {
            if (this == null)
            {
                return;
            }


            RefreshInstalledPackage();
        }


        /// <summary>
        /// Queries Unity Package Manager for the installed
        /// PlayTradeX package.
        /// </summary>
        private void RefreshInstalledPackage()
        {
            if (_loadingPackage ||
                _updateStarted)
            {
                return;
            }


            _loadingPackage =
                true;


            SetStatus(
                "Reading installed PlayTradeX SDK...",
                MessageType.Info);


            try
            {
                _listRequest =
                    Client.List(
                        true,
                        false);


                EditorApplication.update -=
                    PollPackageList;

                EditorApplication.update +=
                    PollPackageList;
            }
            catch (Exception exception)
            {
                _loadingPackage =
                    false;


                SetStatus(
                    "Unable to query Unity Package Manager: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
        }


        // ========================================================
        // Package Discovery
        // ========================================================

        private void PollPackageList()
        {
            if (_listRequest == null)
            {
                EditorApplication.update -=
                    PollPackageList;

                _loadingPackage =
                    false;

                return;
            }


            if (!_listRequest.IsCompleted)
            {
                return;
            }


            EditorApplication.update -=
                PollPackageList;


            try
            {
                if (_listRequest.Status ==
                    StatusCode.Failure)
                {
                    string error =
                        _listRequest.Error != null
                            ? _listRequest.Error.message
                            : "Unknown Package Manager error.";


                    SetStatus(
                        "Unable to read installed packages: " +
                        error,
                        MessageType.Error);

                    return;
                }


                var package =
                    _listRequest.Result?
                        .FirstOrDefault(
                            item =>
                                string.Equals(
                                    item.name,
                                    PlayTradeXEditorInfo.PackageName,
                                    StringComparison.Ordinal));


                if (package == null)
                {
                    ResetPackageInformation();


                    SetStatus(
                        $"Package '{PlayTradeXEditorInfo.PackageName}' is not installed.",
                        MessageType.Error);

                    return;
                }


                _packageFound =
                    true;


                _packageVersion =
                    package.version ??
                    string.Empty;


                _installedPackageId =
                    package.packageId ??
                    string.Empty;


                _packageSource =
                    package.source;


                DetermineInstallationType(
                    package);


                SetStatus(
                    "PlayTradeX SDK detected. Checking available versions...",
                    MessageType.Info);


                _ =
                    RefreshVersionsAsync();
            }
            catch (Exception exception)
            {
                SetStatus(
                    "Failed to inspect the installed PlayTradeX SDK: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
            finally
            {
                _loadingPackage =
                    false;

                _listRequest =
                    null;


                Repaint();
            }
        }


        // ========================================================
        // Installation Type
        // ========================================================

        /// <summary>
        /// Determines whether PlayTradeX is installed from Git or
        /// from a local development source.
        /// </summary>
        private void DetermineInstallationType(
            UnityEditor.PackageManager.PackageInfo package)
        {
            _isLocalInstallation =
                false;


            _installedRevision =
                string.Empty;


            /*
             * Always retain the official repository as fallback.
             *
             * Local packages do not know their remote repository.
             */
            _repositoryUrl =
                PlayTradeXEditorInfo.RepositoryGitUrl;


            switch (package.source)
            {
                case PackageSource.Git:
                {
                    _isLocalInstallation =
                        false;


                    ParseGitPackageId(
                        package.packageId);


                    break;
                }


                case PackageSource.Local:
                case PackageSource.LocalTarball:
                case PackageSource.Embedded:
                {
                    _isLocalInstallation =
                        true;


                    _installedRevision =
                        "Local Development";


                    break;
                }


                default:
                {
                    /*
                     * The package may have been installed through a
                     * source Unity represents differently.
                     *
                     * Attempt Git detection before treating it as
                     * non-Git/local.
                     */
                    if (LooksLikeGitPackageId(
                            package.packageId))
                    {
                        ParseGitPackageId(
                            package.packageId);
                    }
                    else
                    {
                        _isLocalInstallation =
                            true;


                        _installedRevision =
                            "Local Development";
                    }


                    break;
                }
            }


            if (string.IsNullOrWhiteSpace(
                    _repositoryUrl))
            {
                _repositoryUrl =
                    PlayTradeXEditorInfo.RepositoryGitUrl;
            }
        }


        // ========================================================
        // Git Package Parsing
        // ========================================================

        private void ParseGitPackageId(
            string packageId)
        {
            if (string.IsNullOrWhiteSpace(
                    packageId))
            {
                return;
            }


            string value =
                packageId.Trim();


            int atIndex =
                value.IndexOf(
                    '@');


            if (atIndex >= 0 &&
                atIndex < value.Length - 1)
            {
                value =
                    value.Substring(
                        atIndex + 1);
            }


            int hashIndex =
                value.LastIndexOf(
                    '#');


            if (hashIndex >= 0)
            {
                if (hashIndex < value.Length - 1)
                {
                    _installedRevision =
                        value.Substring(
                                hashIndex + 1)
                            .Trim();
                }


                value =
                    value.Substring(
                        0,
                        hashIndex);
            }


            if (LooksLikeGitSource(
                    value))
            {
                _repositoryUrl =
                    value.Trim();
            }


            _isLocalInstallation =
                false;
        }


        private static bool LooksLikeGitPackageId(
            string packageId)
        {
            if (string.IsNullOrWhiteSpace(
                    packageId))
            {
                return false;
            }


            int atIndex =
                packageId.IndexOf(
                    '@');


            if (atIndex < 0 ||
                atIndex >= packageId.Length - 1)
            {
                return false;
            }


            return LooksLikeGitSource(
                packageId.Substring(
                    atIndex + 1));
        }


        private static bool LooksLikeGitSource(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return false;
            }


            return
                value.StartsWith(
                    "http://",
                    StringComparison.OrdinalIgnoreCase) ||

                value.StartsWith(
                    "https://",
                    StringComparison.OrdinalIgnoreCase) ||

                value.StartsWith(
                    "ssh://",
                    StringComparison.OrdinalIgnoreCase) ||

                value.StartsWith(
                    "git://",
                    StringComparison.OrdinalIgnoreCase) ||

                value.StartsWith(
                    "git@",
                    StringComparison.OrdinalIgnoreCase) ||

                value.Contains(
                    ".git");
        }


        // ========================================================
        // Reset
        // ========================================================

        private void ResetPackageInformation()
        {
            _packageFound =
                false;

            _packageVersion =
                string.Empty;

            _installedPackageId =
                string.Empty;

            _repositoryUrl =
                PlayTradeXEditorInfo.RepositoryGitUrl;

            _installedRevision =
                string.Empty;

            _latestVersion =
                string.Empty;

            _isLocalInstallation =
                false;

            _versions.Clear();

            _selectedVersionIndex =
                0;
        }


        // ========================================================
        // Version Discovery
        // ========================================================

        /// <summary>
        /// Fetches available PlayTradeX versions from Git tags.
        /// </summary>
        private async Task RefreshVersionsAsync()
        {
            if (_refreshingVersions ||
                _updateStarted)
            {
                return;
            }


            if (string.IsNullOrWhiteSpace(
                    _repositoryUrl))
            {
                SetStatus(
                    "The PlayTradeX Git repository is unavailable.",
                    MessageType.Error);

                return;
            }


            _refreshingVersions =
                true;


            SetStatus(
                "Checking available PlayTradeX SDK versions...",
                MessageType.Info);


            Repaint();


            try
            {
                GitResult result =
                    await Task.Run(
                        () =>
                            RunGit(
                                "ls-remote --tags " +
                                QuoteArgument(
                                    _repositoryUrl)));


                if (!result.Success)
                {
                    SetStatus(
                        "Unable to retrieve PlayTradeX versions.\n\n" +
                        result.Error,
                        MessageType.Error);

                    return;
                }


                List<string> discoveredVersions =
                    ParseGitTags(
                        result.Output);


                _versions.Clear();


                _versions.AddRange(
                    discoveredVersions);


                _selectedVersionIndex =
                    0;


                _latestVersion =
                    _versions.Count > 0
                        ? _versions[0]
                        : string.Empty;


                if (_versions.Count == 0)
                {
                    SetStatus(
                        "The repository was reached successfully, but no Git tags were found.",
                        MessageType.Warning);

                    return;
                }


                /*
                 * For Git installations select the currently installed
                 * tag when that tag still exists remotely.
                 */
                if (!_isLocalInstallation &&
                    !string.IsNullOrWhiteSpace(
                        _installedRevision))
                {
                    int installedIndex =
                        _versions.FindIndex(
                            version =>
                                string.Equals(
                                    version,
                                    _installedRevision,
                                    StringComparison.Ordinal));


                    if (installedIndex >= 0)
                    {
                        _selectedVersionIndex =
                            installedIndex;
                    }
                }


                if (!_isLocalInstallation &&
                    string.Equals(
                        _installedRevision,
                        _latestVersion,
                        StringComparison.Ordinal))
                {
                    SetStatus(
                        $"PlayTradeX SDK is already using the latest " +
                        $"tag ({_latestVersion}).",
                        MessageType.Info);
                }
                else
                {
                    SetStatus(
                        $"Found {_versions.Count} PlayTradeX SDK " +
                        $"version(s). Latest: {_latestVersion}",
                        MessageType.Info);
                }
            }
            catch (Exception exception)
            {
                SetStatus(
                    "Failed to retrieve PlayTradeX SDK versions: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
            finally
            {
                _refreshingVersions =
                    false;


                if (this != null)
                {
                    Repaint();
                }
            }
        }


        // ========================================================
        // Git Tag Parsing
        // ========================================================

        private static List<string> ParseGitTags(
            string output)
        {
            HashSet<string> tags =
                new HashSet<string>(
                    StringComparer.Ordinal);


            if (!string.IsNullOrWhiteSpace(
                    output))
            {
                string[] lines =
                    output.Split(
                        new[]
                        {
                            '\r',
                            '\n'
                        },
                        StringSplitOptions.RemoveEmptyEntries);


                foreach (string line in lines)
                {
                    int referenceIndex =
                        line.IndexOf(
                            "refs/tags/",
                            StringComparison.Ordinal);


                    if (referenceIndex < 0)
                    {
                        continue;
                    }


                    string tag =
                        line.Substring(
                                referenceIndex +
                                "refs/tags/".Length)
                            .Trim();


                    /*
                     * Annotated tags produce a second peeled reference:
                     *
                     * refs/tags/v1.0.0^{}
                     */
                    if (tag.EndsWith(
                            "^{}",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }


                    if (!string.IsNullOrWhiteSpace(
                            tag))
                    {
                        tags.Add(
                            tag);
                    }
                }
            }


            List<string> result =
                tags.ToList();


            result.Sort(
                CompareVersionsDescending);


            return result;
        }


        // ========================================================
        // Semantic Version Sorting
        // ========================================================

        private static int CompareVersionsDescending(
            string left,
            string right)
        {
            bool leftSemantic =
                TryParseSemanticVersion(
                    left,
                    out SemanticVersion leftVersion);


            bool rightSemantic =
                TryParseSemanticVersion(
                    right,
                    out SemanticVersion rightVersion);


            if (leftSemantic &&
                rightSemantic)
            {
                return
                    -leftVersion.CompareTo(
                        rightVersion);
            }


            if (leftSemantic)
            {
                return -1;
            }


            if (rightSemantic)
            {
                return 1;
            }


            return string.Compare(
                right,
                left,
                StringComparison.OrdinalIgnoreCase);
        }


        private static bool TryParseSemanticVersion(
            string value,
            out SemanticVersion version)
        {
            version =
                default;


            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return false;
            }


            string normalized =
                value.Trim();


            if (normalized.StartsWith(
                    "v",
                    StringComparison.OrdinalIgnoreCase))
            {
                normalized =
                    normalized.Substring(
                        1);
            }


            Match match =
                Regex.Match(
                    normalized,
                    @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<pre>[0-9A-Za-z.-]+))?(?:\+[0-9A-Za-z.-]+)?$");


            if (!match.Success)
            {
                return false;
            }


            if (!int.TryParse(
                    match.Groups["major"].Value,
                    out int major) ||
                !int.TryParse(
                    match.Groups["minor"].Value,
                    out int minor) ||
                !int.TryParse(
                    match.Groups["patch"].Value,
                    out int patch))
            {
                return false;
            }


            version =
                new SemanticVersion(
                    major,
                    minor,
                    patch,
                    match.Groups["pre"].Success
                        ? match.Groups["pre"].Value
                        : string.Empty);


            return true;
        }


        private readonly struct SemanticVersion :
            IComparable<SemanticVersion>
        {
            private readonly int _major;

            private readonly int _minor;

            private readonly int _patch;

            private readonly string _prerelease;


            public SemanticVersion(
                int major,
                int minor,
                int patch,
                string prerelease)
            {
                _major =
                    major;

                _minor =
                    minor;

                _patch =
                    patch;

                _prerelease =
                    prerelease ??
                    string.Empty;
            }


            public int CompareTo(
                SemanticVersion other)
            {
                int comparison =
                    _major.CompareTo(
                        other._major);


                if (comparison != 0)
                {
                    return comparison;
                }


                comparison =
                    _minor.CompareTo(
                        other._minor);


                if (comparison != 0)
                {
                    return comparison;
                }


                comparison =
                    _patch.CompareTo(
                        other._patch);


                if (comparison != 0)
                {
                    return comparison;
                }


                bool thisPrerelease =
                    !string.IsNullOrEmpty(
                        _prerelease);

                bool otherPrerelease =
                    !string.IsNullOrEmpty(
                        other._prerelease);


                /*
                 * Stable version is newer than a prerelease with
                 * the same numeric version.
                 */
                if (thisPrerelease !=
                    otherPrerelease)
                {
                    return thisPrerelease
                        ? -1
                        : 1;
                }


                return ComparePrerelease(
                    _prerelease,
                    other._prerelease);
            }


            private static int ComparePrerelease(
                string left,
                string right)
            {
                if (string.Equals(
                        left,
                        right,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }


                string[] leftParts =
                    left.Split('.');

                string[] rightParts =
                    right.Split('.');


                int count =
                    Math.Max(
                        leftParts.Length,
                        rightParts.Length);


                for (int i = 0;
                     i < count;
                     ++i)
                {
                    if (i >= leftParts.Length)
                    {
                        return -1;
                    }


                    if (i >= rightParts.Length)
                    {
                        return 1;
                    }


                    string leftPart =
                        leftParts[i];

                    string rightPart =
                        rightParts[i];


                    bool leftNumeric =
                        int.TryParse(
                            leftPart,
                            out int leftNumber);

                    bool rightNumeric =
                        int.TryParse(
                            rightPart,
                            out int rightNumber);


                    if (leftNumeric &&
                        rightNumeric)
                    {
                        int numericComparison =
                            leftNumber.CompareTo(
                                rightNumber);


                        if (numericComparison != 0)
                        {
                            return numericComparison;
                        }


                        continue;
                    }


                    /*
                     * SemVer numeric identifiers have lower precedence
                     * than non-numeric identifiers.
                     */
                    if (leftNumeric !=
                        rightNumeric)
                    {
                        return leftNumeric
                            ? -1
                            : 1;
                    }


                    int textComparison =
                        string.Compare(
                            leftPart,
                            rightPart,
                            StringComparison.OrdinalIgnoreCase);


                    if (textComparison != 0)
                    {
                        return textComparison;
                    }
                }


                return 0;
            }
        }


        // ========================================================
        // GUI
        // ========================================================

        private void OnGUI()
        {
            _scrollPosition =
                EditorGUILayout.BeginScrollView(
                    _scrollPosition);


            DrawHeader();


            EditorGUILayout.Space(
                12);


            DrawInstalledSdk();


            EditorGUILayout.Space(
                16);


            DrawAvailableVersions();


            EditorGUILayout.Space(
                16);


            DrawCustomRevision();


            EditorGUILayout.Space(
                16);


            DrawStatus();


            GUILayout.FlexibleSpace();


            EditorGUILayout.Space(
                16);


            DrawActions();


            EditorGUILayout.EndScrollView();
        }


        // ========================================================
        // Header GUI
        // ========================================================

        private static void DrawHeader()
        {
            GUIStyle titleStyle =
                new GUIStyle(
                    EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleLeft
                };


            EditorGUILayout.LabelField(
                "PlayTradeX SDK Updater",
                titleStyle);


            EditorGUILayout.Space(
                4);


            EditorGUILayout.LabelField(
                "View installed SDK information and switch between " +
                "available PlayTradeX Git releases.",
                EditorStyles.wordWrappedLabel);
        }


        // ========================================================
        // Installed SDK GUI
        // ========================================================

        private void DrawInstalledSdk()
        {
            EditorGUILayout.LabelField(
                "Installed SDK",
                EditorStyles.boldLabel);


            using (new EditorGUI.DisabledScope(
                       true))
            {
                EditorGUILayout.TextField(
                    "Package",
                    PlayTradeXEditorInfo.PackageName);


                EditorGUILayout.TextField(
                    "Package Version",
                    string.IsNullOrWhiteSpace(
                        _packageVersion)
                        ? "Unknown"
                        : _packageVersion);


                EditorGUILayout.TextField(
                    "Installation Source",
                    GetInstallationSourceText());


                EditorGUILayout.TextField(
                    "Installed Revision",
                    string.IsNullOrWhiteSpace(
                        _installedRevision)
                        ? "Unpinned / Unknown"
                        : _installedRevision);


                EditorGUILayout.TextField(
                    "Latest Available",
                    string.IsNullOrWhiteSpace(
                        _latestVersion)
                        ? "Unknown"
                        : _latestVersion);
            }


            EditorGUILayout.Space(
                6);


            EditorGUILayout.LabelField(
                "Repository",
                EditorStyles.miniBoldLabel);


            EditorGUILayout.SelectableLabel(
                _repositoryUrl,
                EditorStyles.textField,
                GUILayout.Height(
                    EditorGUIUtility.singleLineHeight));


            if (_packageFound &&
                _isLocalInstallation)
            {
                EditorGUILayout.Space(
                    8);


                EditorGUILayout.HelpBox(
                    "PlayTradeX is currently loaded from a local/disk " +
                    "package. You can still browse Git releases. Updating " +
                    "to a selected release will replace the local package " +
                    "dependency in this Unity project with the selected " +
                    "Git version.",
                    MessageType.Warning);
            }
        }


        private string GetInstallationSourceText()
        {
            if (!_packageFound)
            {
                return "Not Installed";
            }


            if (_isLocalInstallation)
            {
                switch (_packageSource)
                {
                    case PackageSource.Embedded:

                        return "Embedded / Local Development";


                    case PackageSource.LocalTarball:

                        return "Local Tarball";


                    default:

                        return "Local / Disk";
                }
            }


            if (_packageSource ==
                PackageSource.Git)
            {
                return "Git";
            }


            return
                _packageSource.ToString();
        }


        // ========================================================
        // Available Versions GUI
        // ========================================================

        private void DrawAvailableVersions()
        {
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.LabelField(
                "Available Versions",
                EditorStyles.boldLabel);


            GUILayout.FlexibleSpace();


            using (new EditorGUI.DisabledScope(
                       _refreshingVersions ||
                       _updateStarted))
            {
                if (GUILayout.Button(
                        _refreshingVersions
                            ? "Refreshing..."
                            : "Refresh Versions",
                        GUILayout.Width(130),
                        GUILayout.Height(22)))
                {
                    _ =
                        RefreshVersionsAsync();
                }
            }


            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(
                6);


            using (new EditorGUI.DisabledScope(
                       _refreshingVersions ||
                       _updateStarted ||
                       _versions.Count == 0))
            {
                if (_versions.Count > 0)
                {
                    _selectedVersionIndex =
                        Mathf.Clamp(
                            _selectedVersionIndex,
                            0,
                            _versions.Count - 1);


                    _selectedVersionIndex =
                        EditorGUILayout.Popup(
                            "Version",
                            _selectedVersionIndex,
                            _versions.ToArray());
                }
                else
                {
                    EditorGUILayout.Popup(
                        "Version",
                        0,
                        new[]
                        {
                            "No versions available"
                        });
                }
            }
        }


        // ========================================================
        // Custom Revision GUI
        // ========================================================

        private void DrawCustomRevision()
        {
            EditorGUILayout.LabelField(
                "Custom Revision",
                EditorStyles.boldLabel);


            EditorGUILayout.LabelField(
                "Optional. Enter a Git tag, branch, or commit hash. " +
                "When this field is not empty, Update Selected uses it " +
                "instead of the version dropdown.",
                EditorStyles.wordWrappedMiniLabel);


            EditorGUILayout.Space(
                6);


            using (new EditorGUI.DisabledScope(
                       _updateStarted))
            {
                _customRevision =
                    EditorGUILayout.TextField(
                        "Revision",
                        _customRevision);
            }
        }


        // ========================================================
        // Status GUI
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
        }


        // ========================================================
        // Action GUI
        // ========================================================

        private void DrawActions()
        {
            EditorGUILayout.LabelField(
                string.Empty,
                GUI.skin.horizontalSlider);


            EditorGUILayout.Space(
                8);


            if (_updateStarted)
            {
                EditorGUILayout.HelpBox(
                    "Unity Package Manager is updating PlayTradeX. " +
                    "The package may be recompiled or reloaded during " +
                    "this operation.",
                    MessageType.Info);
            }


            EditorGUILayout.BeginHorizontal();


            using (new EditorGUI.DisabledScope(
                       !CanUpdateSelected()))
            {
                if (GUILayout.Button(
                        "Update Selected",
                        GUILayout.Height(30)))
                {
                    ConfirmAndUpdate(
                        GetSelectedRevision());
                }
            }


            using (new EditorGUI.DisabledScope(
                       !CanUpdateLatest()))
            {
                if (GUILayout.Button(
                        "Update Latest",
                        GUILayout.Height(30)))
                {
                    ConfirmAndUpdate(
                        _latestVersion);
                }
            }


            /*
             * Cancel is intentionally disabled once the Package Manager
             * update has begun. Closing this window would not reliably
             * cancel an already-running UPM operation.
             */
            using (new EditorGUI.DisabledScope(
                       _updateStarted))
            {
                if (GUILayout.Button(
                        "Cancel",
                        GUILayout.Width(100),
                        GUILayout.Height(30)))
                {
                    Close();
                }
            }


            EditorGUILayout.EndHorizontal();
        }


        private bool CanUpdateSelected()
        {
            return
                _packageFound &&
                !_loadingPackage &&
                !_refreshingVersions &&
                !_updateStarted &&
                !string.IsNullOrWhiteSpace(
                    _repositoryUrl) &&
                !string.IsNullOrWhiteSpace(
                    GetSelectedRevision());
        }


        private bool CanUpdateLatest()
        {
            return
                _packageFound &&
                !_loadingPackage &&
                !_refreshingVersions &&
                !_updateStarted &&
                !string.IsNullOrWhiteSpace(
                    _repositoryUrl) &&
                !string.IsNullOrWhiteSpace(
                    _latestVersion);
        }


        // ========================================================
        // Selected Revision
        // ========================================================

        private string GetSelectedRevision()
        {
            /*
             * Custom revision takes precedence.
             */
            if (!string.IsNullOrWhiteSpace(
                    _customRevision))
            {
                return
                    _customRevision.Trim();
            }


            if (_versions.Count == 0)
            {
                return string.Empty;
            }


            if (_selectedVersionIndex < 0 ||
                _selectedVersionIndex >= _versions.Count)
            {
                return string.Empty;
            }


            return
                _versions[_selectedVersionIndex];
        }


        // ========================================================
        // Update Confirmation
        // ========================================================

        private void ConfirmAndUpdate(
            string revision)
        {
            if (string.IsNullOrWhiteSpace(
                    revision))
            {
                SetStatus(
                    "Select or enter a PlayTradeX revision first.",
                    MessageType.Warning);

                return;
            }


            revision =
                revision.Trim()
                    .TrimStart('#');


            /*
             * Only Git installations have a meaningful currently
             * installed Git revision.
             */
            if (!_isLocalInstallation &&
                string.Equals(
                    revision,
                    _installedRevision,
                    StringComparison.Ordinal))
            {
                SetStatus(
                    $"PlayTradeX SDK is already using '{revision}'.",
                    MessageType.Info);

                return;
            }


            string currentText =
                _isLocalInstallation
                    ? $"Local / Disk ({_packageVersion})"
                    : string.IsNullOrWhiteSpace(
                        _installedRevision)
                        ? _packageVersion
                        : _installedRevision;


            string message;


            if (_isLocalInstallation)
            {
                message =
                    "PlayTradeX is currently loaded from a local/disk package.\n\n" +

                    "Updating will replace the local package dependency " +
                    "in this Unity project with the selected Git release.\n\n" +

                    $"Current:  {currentText}\n" +
                    $"Selected: {revision}\n\n" +

                    "Your source files on disk will not be deleted. " +
                    "This Unity project will simply stop referencing " +
                    "that local package and use the Git package instead.\n\n" +

                    "Continue?";
            }
            else
            {
                message =
                    "Unity Package Manager will change the installed " +
                    "PlayTradeX SDK revision.\n\n" +

                    $"Current:  {currentText}\n" +
                    $"Selected: {revision}\n\n" +

                    "Unity may recompile scripts and reload the package.\n\n" +

                    "Continue?";
            }


            bool confirmed =
                EditorUtility.DisplayDialog(
                    "Update PlayTradeX SDK",
                    message,
                    "Update",
                    "Cancel");


            if (!confirmed)
            {
                return;
            }


            BeginPackageUpdate(
                revision);
        }


        // ========================================================
        // Package Update
        // ========================================================

        /// <summary>
        /// Requests Unity Package Manager to install the official
        /// PlayTradeX repository at the requested revision.
        /// </summary>
        private void BeginPackageUpdate(
            string revision)
        {
            if (_updateStarted)
            {
                return;
            }


            if (string.IsNullOrWhiteSpace(
                    _repositoryUrl))
            {
                SetStatus(
                    "The PlayTradeX repository URL is unavailable.",
                    MessageType.Error);

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    revision))
            {
                SetStatus(
                    "The requested revision is empty.",
                    MessageType.Error);

                return;
            }


            revision =
                revision.Trim()
                    .TrimStart('#');


            string packageUrl =
                _repositoryUrl +
                "#" +
                revision;


            _updateStarted =
                true;


            SetStatus(
                $"Updating PlayTradeX SDK to '{revision}'...",
                MessageType.Info);


            Repaint();


            try
            {
                /*
                 * Client.Add replaces an existing package dependency
                 * with the same package name.
                 *
                 * This allows the same operation to move:
                 *
                 * Local -> Git
                 * Git   -> another Git revision
                 */
                _addRequest =
                    Client.Add(
                        packageUrl);


                EditorApplication.update -=
                    PollPackageUpdate;

                EditorApplication.update +=
                    PollPackageUpdate;
            }
            catch (Exception exception)
            {
                _updateStarted =
                    false;


                SetStatus(
                    "Unable to start the PlayTradeX SDK update: " +
                    exception.Message,
                    MessageType.Error);


                Debug.LogException(
                    exception);
            }
        }


        private void PollPackageUpdate()
        {
            if (_addRequest == null)
            {
                EditorApplication.update -=
                    PollPackageUpdate;

                _updateStarted =
                    false;

                return;
            }


            if (!_addRequest.IsCompleted)
            {
                return;
            }


            EditorApplication.update -=
                PollPackageUpdate;


            try
            {
                if (_addRequest.Status ==
                    StatusCode.Failure)
                {
                    string error =
                        _addRequest.Error != null
                            ? _addRequest.Error.message
                            : "Unknown Package Manager error.";


                    _updateStarted =
                        false;


                    SetStatus(
                        "PlayTradeX SDK update failed:\n" +
                        error,
                        MessageType.Error);


                    return;
                }


                string requestedRevision =
                    GetRequestedRevisionFromResult();


                SetStatus(
                    $"PlayTradeX SDK updated successfully to " +
                    $"'{requestedRevision}'. Unity may now reload " +
                    "or recompile the package.",
                    MessageType.Info);


                Debug.Log(
                    "[PlayTradeX] SDK update completed: " +
                    requestedRevision);


                /*
                 * The updater itself belongs to the package that has
                 * just been changed. Unity may trigger a domain reload
                 * immediately after this point.
                 *
                 * Do not perform another package operation here.
                 */
            }
            catch (Exception exception)
            {
                SetStatus(
                    "The package update completed, but the updater " +
                    "could not refresh its state: " +
                    exception.Message,
                    MessageType.Warning);


                Debug.LogException(
                    exception);
            }
            finally
            {
                _updateStarted =
                    false;

                _addRequest =
                    null;


                if (this != null)
                {
                    Repaint();
                }
            }
        }


        private string GetRequestedRevisionFromResult()
        {
            if (_addRequest != null &&
                _addRequest.Result != null &&
                !string.IsNullOrWhiteSpace(
                    _addRequest.Result.packageId))
            {
                string packageId =
                    _addRequest.Result.packageId;


                int hashIndex =
                    packageId.LastIndexOf(
                        '#');


                if (hashIndex >= 0 &&
                    hashIndex < packageId.Length - 1)
                {
                    return
                        packageId.Substring(
                                hashIndex + 1)
                            .Trim();
                }
            }


            return
                GetSelectedRevision();
        }


        // ========================================================
        // Git Process
        // ========================================================

        private static GitResult RunGit(
            string arguments)
        {
            try
            {
                ProcessStartInfo startInfo =
                    new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };


                using (Process process =
                    new Process())
                {
                    process.StartInfo =
                        startInfo;


                    if (!process.Start())
                    {
                        return new GitResult(
                            false,
                            string.Empty,
                            "Unable to start Git.");
                    }


                    /*
                     * Read both redirected streams asynchronously.
                     *
                     * Waiting synchronously on one full redirected
                     * stream before the other can deadlock if Git
                     * produces enough output on stderr.
                     */
                    Task<string> outputTask =
                        process.StandardOutput.ReadToEndAsync();

                    Task<string> errorTask =
                        process.StandardError.ReadToEndAsync();


                    if (!process.WaitForExit(
                            GitTimeoutMilliseconds))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // Ignore termination errors.
                        }


                        return new GitResult(
                            false,
                            string.Empty,
                            "Git operation timed out.");
                    }


                    Task.WaitAll(
                        outputTask,
                        errorTask);


                    string output =
                        outputTask.Result;

                    string error =
                        errorTask.Result;


                    if (process.ExitCode != 0)
                    {
                        return new GitResult(
                            false,
                            output,
                            string.IsNullOrWhiteSpace(
                                error)
                                ? $"Git exited with code {process.ExitCode}."
                                : error.Trim());
                    }


                    return new GitResult(
                        true,
                        output,
                        string.Empty);
                }
            }
            catch (Exception exception)
            {
                return new GitResult(
                    false,
                    string.Empty,
                    "Git could not be executed. Ensure Git is installed " +
                    "and available through the system PATH.\n\n" +
                    exception.Message);
            }
        }


        private static string QuoteArgument(
            string value)
        {
            if (string.IsNullOrEmpty(
                    value))
            {
                return "\"\"";
            }


            return
                "\"" +
                value.Replace(
                    "\"",
                    "\\\"") +
                "\"";
        }


        private readonly struct GitResult
        {
            public bool Success { get; }

            public string Output { get; }

            public string Error { get; }


            public GitResult(
                bool success,
                string output,
                string error)
            {
                Success =
                    success;

                Output =
                    output ??
                    string.Empty;

                Error =
                    error ??
                    string.Empty;
            }
        }


        // ========================================================
        // Status
        // ========================================================

        private void SetStatus(
            string message,
            MessageType type)
        {
            _statusMessage =
                message ??
                string.Empty;

            _statusMessageType =
                type;


            if (this != null)
            {
                Repaint();
            }
        }
    }
}

#endif