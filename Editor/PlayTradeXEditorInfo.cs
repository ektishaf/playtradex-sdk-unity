#if UNITY_EDITOR

using System;
using UnityEditor.PackageManager;


namespace PlayTradeX.Editor
{
    /// <summary>
    /// Central source of PlayTradeX Unity Editor and package metadata.
    /// </summary>
    /// <remarks>
    /// Editor tools should use this class instead of duplicating
    /// package names, repository URLs, documentation URLs, or package
    /// discovery logic.
    ///
    /// This class supports PlayTradeX packages installed through:
    ///
    /// - Git
    /// - Local / disk package
    /// - Embedded package
    /// - Local tarball
    /// - Package registry
    /// </remarks>
    internal static class PlayTradeXEditorInfo
    {
        // ========================================================
        // Package
        // ========================================================

        internal const string PackageName =
            "com.playtradex.sdk";


        // ========================================================
        // Repository
        // ========================================================

        internal const string RepositoryUrl =
            "https://github.com/ektishaf/playtradex-sdk-unity";

        internal const string RepositoryGitUrl =
            "https://github.com/ektishaf/playtradex-sdk-unity.git";

        internal const string DocumentationUrl =
            RepositoryUrl +
            "/blob/main/Documentation~/index.md";

        internal const string IssuesUrl =
            RepositoryUrl +
            "/issues";

        internal const string ChangelogUrl =
            RepositoryUrl +
            "/blob/main/CHANGELOG.md";

        internal const string LicenseUrl =
            RepositoryUrl +
            "/blob/main/LICENSE.md";


        // ========================================================
        // Package Discovery
        // ========================================================

        /// <summary>
        /// Gets the currently registered PlayTradeX package.
        /// </summary>
        /// <returns>
        /// The registered PlayTradeX PackageInfo, or null when the
        /// package cannot be found.
        /// </returns>
        internal static PackageInfo GetPackageInfo()
        {
            /*
             * First try direct package lookup by package name.
             *
             * This works reliably for both local development packages
             * and packages installed through Git.
             */
            PackageInfo[] packages =
                PackageInfo.GetAllRegisteredPackages();


            if (packages == null)
            {
                return null;
            }


            for (int i = 0;
                 i < packages.Length;
                 ++i)
            {
                PackageInfo package =
                    packages[i];
                

                if (package == null)
                {
                    continue;
                }


                if (string.Equals(
                        package.name,
                        PackageName,
                        StringComparison.Ordinal))
                {
                    return package;
                }
            }


            return null;
        }


        // ========================================================
        // Version
        // ========================================================

        /// <summary>
        /// Gets the version declared by the installed PlayTradeX
        /// package.json.
        /// </summary>
        internal static string GetVersion()
        {
            PackageInfo package =
                GetPackageInfo();


            if (package == null ||
                string.IsNullOrWhiteSpace(
                    package.version))
            {
                return "Unknown";
            }


            return
                package.version.Trim();
        }


        // ========================================================
        // Release Channel
        // ========================================================

        /// <summary>
        /// Gets the release channel derived from the package version.
        /// </summary>
        internal static string GetReleaseChannel()
        {
            return
                GetReleaseChannel(
                    GetVersion());
        }


        /// <summary>
        /// Determines the release channel represented by a package
        /// version.
        /// </summary>
        internal static string GetReleaseChannel(
            string version)
        {
            if (string.IsNullOrWhiteSpace(
                    version) ||
                string.Equals(
                    version,
                    "Unknown",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Unknown";
            }


            if (version.IndexOf(
                    "alpha",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Alpha";
            }


            if (version.IndexOf(
                    "beta",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Beta";
            }


            if (version.IndexOf(
                    "-rc",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Release Candidate";
            }


            return "Stable";
        }


        // ========================================================
        // Installation Source
        // ========================================================

        /// <summary>
        /// Gets a human-readable description of the current package
        /// installation source.
        /// </summary>
        internal static string GetInstallationSource()
        {
            PackageInfo package =
                GetPackageInfo();


            if (package == null)
            {
                return "Unknown";
            }


            return
                GetInstallationSource(
                    package.source);
        }


        /// <summary>
        /// Converts Unity's PackageSource into a human-readable
        /// PlayTradeX installation source.
        /// </summary>
        internal static string GetInstallationSource(
            PackageSource source)
        {
            switch (source)
            {
                case PackageSource.Git:

                    return "Git";


                case PackageSource.Local:

                    return "Local / Disk";


                case PackageSource.Embedded:

                    return "Embedded / Local Development";


                case PackageSource.LocalTarball:

                    return "Local Tarball";


                case PackageSource.Registry:

                    return "Unity Package Registry";


                case PackageSource.BuiltIn:

                    return "Built-in";


                default:

                    return source.ToString();
            }
        }


        // ========================================================
        // Installation Helpers
        // ========================================================

        /// <summary>
        /// Returns true when the package is being used from a local
        /// development source.
        /// </summary>
        internal static bool IsLocalInstallation()
        {
            PackageInfo package =
                GetPackageInfo();


            if (package == null)
            {
                return false;
            }


            return
                package.source == PackageSource.Local ||
                package.source == PackageSource.Embedded ||
                package.source == PackageSource.LocalTarball;
        }


        /// <summary>
        /// Returns true when PlayTradeX is installed directly from
        /// a Git repository.
        /// </summary>
        internal static bool IsGitInstallation()
        {
            PackageInfo package =
                GetPackageInfo();


            return
                package != null &&
                package.source == PackageSource.Git;
        }


        // ========================================================
        // Git Revision
        // ========================================================

        /// <summary>
        /// Gets the currently installed Git revision when PlayTradeX
        /// is installed from Git.
        /// </summary>
        /// <returns>
        /// The Git tag, branch, or commit revision. Local packages
        /// return "Local Development".
        /// </returns>
        internal static string GetInstalledRevision()
        {
            PackageInfo package =
                GetPackageInfo();


            if (package == null)
            {
                return "Unknown";
            }


            if (package.source == PackageSource.Local ||
                package.source == PackageSource.Embedded ||
                package.source == PackageSource.LocalTarball)
            {
                return "Local Development";
            }


            if (package.source != PackageSource.Git)
            {
                return "Unknown";
            }


            if (string.IsNullOrWhiteSpace(
                    package.packageId))
            {
                return "Unknown";
            }


            string packageId =
                package.packageId;


            int hashIndex =
                packageId.LastIndexOf(
                    '#');


            if (hashIndex < 0 ||
                hashIndex >= packageId.Length - 1)
            {
                return "Unpinned";
            }


            return
                packageId.Substring(
                        hashIndex + 1)
                    .Trim();
        }


        // ========================================================
        // Repository Resolution
        // ========================================================

        /// <summary>
        /// Gets the repository URL used for Git version discovery.
        /// </summary>
        /// <remarks>
        /// For Git installations, the actual installed repository is
        /// returned when it can be extracted from PackageInfo.
        ///
        /// Local development installations fall back to the official
        /// PlayTradeX repository.
        /// </remarks>
        internal static string GetRepositoryGitUrl()
        {
            PackageInfo package =
                GetPackageInfo();


            if (package == null ||
                package.source != PackageSource.Git ||
                string.IsNullOrWhiteSpace(
                    package.packageId))
            {
                return RepositoryGitUrl;
            }


            string packageId =
                package.packageId.Trim();


            int atIndex =
                packageId.IndexOf(
                    '@');


            if (atIndex >= 0 &&
                atIndex < packageId.Length - 1)
            {
                packageId =
                    packageId.Substring(
                        atIndex + 1);
            }


            int hashIndex =
                packageId.LastIndexOf(
                    '#');


            if (hashIndex >= 0)
            {
                packageId =
                    packageId.Substring(
                        0,
                        hashIndex);
            }


            if (LooksLikeGitUrl(
                    packageId))
            {
                return
                    packageId.Trim();
            }


            return RepositoryGitUrl;
        }


        private static bool LooksLikeGitUrl(
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
    }
}

#endif