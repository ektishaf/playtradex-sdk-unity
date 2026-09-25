using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayTradeX
{
    /// <summary>
    /// Provides platform-aware runtime file selection for
    /// PlayTradeX wallet import and export.
    /// </summary>
    public static class PlayTradeXFilePicker
    {
        // ============================================================
        // Public API
        // ============================================================

        /// <summary>
        /// Opens the platform file picker for selecting a destination
        /// for an exported PlayTradeX wallet.
        /// </summary>
        /// <returns>
        /// A filesystem path on Windows, a content URI on Android,
        /// or null if the user cancels.
        /// </returns>
        public static Task<string> SaveWalletFileAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            return PlayTradeXFilePickerCallback.Open(true);

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

            return Task.FromResult(
                SaveWalletFileWindows());

#else

            throw new PlatformNotSupportedException(
                "Wallet file export picker is not supported " +
                "on this platform.");

#endif
        }


        /// <summary>
        /// Opens the platform file picker for selecting an existing
        /// PlayTradeX wallet.
        /// </summary>
        /// <returns>
        /// A filesystem path on Windows, a content URI on Android,
        /// or null if the user cancels.
        /// </returns>
        public static Task<string> OpenWalletFileAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            return PlayTradeXFilePickerCallback.Open(false);

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

            return Task.FromResult(
                OpenWalletFileWindows());

#else

            throw new PlatformNotSupportedException(
                "Wallet file import picker is not supported " +
                "on this platform.");

#endif
        }


        // ============================================================
        // Windows
        // ============================================================

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        [StructLayout(
    LayoutKind.Sequential,
    CharSet = CharSet.Unicode)]
        private struct OpenFileName
        {
            public int structSize;
            public IntPtr dlgOwner;
            public IntPtr instance;

            public string filter;
            public string customFilter;
            public int maxCustFilter;
            public int filterIndex;

            public IntPtr file;
            public int maxFile;

            public IntPtr fileTitle;
            public int maxFileTitle;

            public string initialDir;
            public string title;

            public int flags;
            public short fileOffset;
            public short fileExtension;

            public string defExt;

            public IntPtr custData;
            public IntPtr hook;
            public string templateName;

            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }


        private const int OFN_OVERWRITEPROMPT =
            0x00000002;

        private const int OFN_NOCHANGEDIR =
            0x00000008;

        private const int OFN_PATHMUSTEXIST =
            0x00000800;

        private const int OFN_FILEMUSTEXIST =
            0x00001000;

        private const int OFN_EXPLORER =
            0x00080000;


        [DllImport(
    "comdlg32.dll",
    CharSet = CharSet.Unicode,
    SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetOpenFileNameW(
    ref OpenFileName openFileName);


        [DllImport(
            "comdlg32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSaveFileNameW(
            ref OpenFileName openFileName);


        private static string SaveWalletFileWindows()
        {
            const int bufferSize = 4096;

            IntPtr fileBuffer =
                Marshal.AllocHGlobal(
                    bufferSize * sizeof(char));

            try
            {
                for (int i = 0;
                     i < bufferSize * sizeof(char);
                     i++)
                {
                    Marshal.WriteByte(
                        fileBuffer,
                        i,
                        0);
                }

                string defaultFileName =
                    "playtradex-wallet.ptx";

                byte[] defaultFileBytes =
                    Encoding.Unicode.GetBytes(
                        defaultFileName + "\0");

                Marshal.Copy(
                    defaultFileBytes,
                    0,
                    fileBuffer,
                    defaultFileBytes.Length);

                OpenFileName dialog =
                    new OpenFileName
                    {
                        structSize =
                            Marshal.SizeOf<OpenFileName>(),

                        dlgOwner =
                            IntPtr.Zero,

                        filter =
                            "PlayTradeX Wallet (*.ptx)\0*.ptx\0" +
                            "All Files (*.*)\0*.*\0\0",

                        filterIndex = 1,

                        file =
                            fileBuffer,

                        maxFile =
                            bufferSize,

                        title =
                            "Export PlayTradeX Wallet",

                        flags =
                            OFN_EXPLORER |
                            OFN_PATHMUSTEXIST |
                            OFN_OVERWRITEPROMPT |
                            OFN_NOCHANGEDIR,

                        defExt =
                            "ptx"
                    };

                if (!GetSaveFileNameW(
                        ref dialog))
                {
                    return null;
                }

                return Marshal.PtrToStringUni(
                    fileBuffer);
            }
            finally
            {
                Marshal.FreeHGlobal(
                    fileBuffer);
            }
        }


        private static string OpenWalletFileWindows()
        {
            const int bufferSize = 4096;

            IntPtr fileBuffer =
                Marshal.AllocHGlobal(
                    bufferSize * sizeof(char));

            try
            {
                for (int i = 0;
                     i < bufferSize * sizeof(char);
                     i++)
                {
                    Marshal.WriteByte(
                        fileBuffer,
                        i,
                        0);
                }

                OpenFileName dialog =
                    new OpenFileName
                    {
                        structSize =
                            Marshal.SizeOf<OpenFileName>(),

                        dlgOwner =
                            IntPtr.Zero,

                        filter =
                            "PlayTradeX Wallet (*.ptx)\0*.ptx\0" +
                            "All Files (*.*)\0*.*\0\0",

                        filterIndex = 1,

                        file =
                            fileBuffer,

                        maxFile =
                            bufferSize,

                        title =
                            "Import PlayTradeX Wallet",

                        flags =
                            OFN_EXPLORER |
                            OFN_PATHMUSTEXIST |
                            OFN_FILEMUSTEXIST |
                            OFN_NOCHANGEDIR,

                        defExt =
                            "ptx"
                    };

                if (!GetOpenFileNameW(
                        ref dialog))
                {
                    return null;
                }

                return Marshal.PtrToStringUni(
                    fileBuffer);
            }
            finally
            {
                Marshal.FreeHGlobal(
                    fileBuffer);
            }
        }
#endif
    }


    // ============================================================
    // Android Callback
    // ============================================================

#if UNITY_ANDROID && !UNITY_EDITOR

    internal sealed class PlayTradeXFilePickerCallback :
        AndroidJavaProxy
    {
        private readonly TaskCompletionSource<string>
            _completion;

        private static PlayTradeXFilePickerCallback
            _activeCallback;


        private PlayTradeXFilePickerCallback(
            TaskCompletionSource<string> completion)
            : base(
                "com.playtradex.PlayTradeXFilePicker$Callback")
        {
            _completion = completion;
        }


        /// <summary>
        /// Opens the Android document picker.
        /// </summary>
        internal static Task<string> Open(
            bool export)
        {
            if (_activeCallback != null)
            {
                throw new InvalidOperationException(
                    "A PlayTradeX file picker is already open.");
            }

            TaskCompletionSource<string> completion =
                new TaskCompletionSource<string>(
                    TaskCreationOptions
                        .RunContinuationsAsynchronously);

            PlayTradeXFilePickerCallback callback =
                new PlayTradeXFilePickerCallback(
                    completion);

            _activeCallback = callback;

            try
            {
                using AndroidJavaClass unityPlayer =
                    new AndroidJavaClass(
                        "com.unity3d.player.UnityPlayer");

                using AndroidJavaObject activity =
                    unityPlayer.GetStatic<AndroidJavaObject>(
                        "currentActivity");

                if (activity == null)
                {
                    _activeCallback = null;

                    completion.TrySetResult(null);

                    return completion.Task;
                }

                using AndroidJavaClass picker =
                    new AndroidJavaClass(
                        "com.playtradex.PlayTradeXFilePicker");

                picker.CallStatic(
                    export
                        ? "openExportPicker"
                        : "openImportPicker",
                    activity,
                    callback);
            }
            catch (Exception exception)
            {
                _activeCallback = null;

                completion.TrySetException(
                    exception);
            }

            return completion.Task;
        }


        /// <summary>
        /// Receives the URI returned by the Android document picker.
        /// </summary>
        public void onResult(
            string uri)
        {
            TaskCompletionSource<string> completion =
                _completion;

            _activeCallback = null;

            completion.TrySetResult(
                string.IsNullOrEmpty(uri)
                    ? null
                    : uri);
        }
    }

#endif
}