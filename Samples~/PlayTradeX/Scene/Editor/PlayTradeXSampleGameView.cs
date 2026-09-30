#if UNITY_EDITOR

using System;
using System.Reflection;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayTradeX.Sample.Editor
{
    /// <summary>
    /// Automatically selects a 1920x1080 Game View resolution
    /// whenever the PlayTradeX sample scene is opened.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayTradeXSampleGameView
    {
        private const string SampleSceneName =
            "PlayTradeXSample";

        private const string ResolutionName =
            "PlayTradeX 1920x1080";

        private const int Width =
            1920;

        private const int Height =
            1080;


        static PlayTradeXSampleGameView()
        {
            EditorSceneManager.sceneOpened +=
                OnSceneOpened;

            /*
             * Also handle the case where Unity recompiles scripts
             * while the sample scene is already open.
             */
            EditorApplication.delayCall +=
                ApplyIfSampleSceneIsOpen;
        }


        private static void OnSceneOpened(
            Scene scene,
            OpenSceneMode mode)
        {
            if (!IsSampleScene(scene))
            {
                return;
            }


            /*
             * Wait until Unity has finished opening the scene
             * and rebuilding Editor windows.
             */
            EditorApplication.delayCall +=
                SetGameViewResolution;
        }


        private static void ApplyIfSampleSceneIsOpen()
        {
            Scene scene =
                SceneManager.GetActiveScene();


            if (!IsSampleScene(scene))
            {
                return;
            }


            SetGameViewResolution();
        }


        private static bool IsSampleScene(
            Scene scene)
        {
            return scene.IsValid() &&
                   string.Equals(
                       scene.name,
                       SampleSceneName,
                       StringComparison.Ordinal);
        }


        private static void SetGameViewResolution()
        {
            try
            {
                Assembly editorAssembly =
                    typeof(EditorWindow).Assembly;


                Type gameViewType =
                    editorAssembly.GetType(
                        "UnityEditor.GameView");

                Type gameViewSizesType =
                    editorAssembly.GetType(
                        "UnityEditor.GameViewSizes");

                Type gameViewSizeType =
                    editorAssembly.GetType(
                        "UnityEditor.GameViewSize");

                Type gameViewSizeTypeEnum =
                    editorAssembly.GetType(
                        "UnityEditor.GameViewSizeType");


                if (gameViewType == null ||
                    gameViewSizesType == null ||
                    gameViewSizeType == null ||
                    gameViewSizeTypeEnum == null)
                {
                    Debug.LogWarning(
                        "[PlayTradeX] Unable to access Unity Game View " +
                        "resolution API.");

                    return;
                }


                object gameViewSizes =
                    GetGameViewSizes(
                        gameViewSizesType);


                if (gameViewSizes == null)
                {
                    return;
                }


                object group =
                    GetCurrentGameViewSizeGroup(
                        gameViewSizesType,
                        gameViewSizes);


                if (group == null)
                {
                    return;
                }


                int index =
                    FindResolution(
                        group);


                if (index < 0)
                {
                    index =
                        AddResolution(
                            group,
                            gameViewSizeType,
                            gameViewSizeTypeEnum);
                }


                if (index < 0)
                {
                    return;
                }


                SelectResolution(
                    gameViewType,
                    index);
            }
            catch (Exception exception)
            {
                /*
                 * This is Sample Editor convenience functionality.
                 * It must never prevent PlayTradeX or the developer's
                 * project from compiling/running.
                 */
                Debug.LogWarning(
                    "[PlayTradeX] Could not automatically select " +
                    "1920x1080 Game View resolution.\n" +
                    exception.Message);
            }
        }


        private static object GetGameViewSizes(
            Type gameViewSizesType)
        {
            Type scriptableSingletonType =
                typeof(ScriptableSingleton<>)
                    .MakeGenericType(
                        gameViewSizesType);


            PropertyInfo instanceProperty =
                scriptableSingletonType.GetProperty(
                    "instance",
                    BindingFlags.Public |
                    BindingFlags.Static);


            return instanceProperty?.GetValue(
                null);
        }


        private static object GetCurrentGameViewSizeGroup(
            Type gameViewSizesType,
            object gameViewSizes)
        {
            PropertyInfo currentGroupProperty =
                gameViewSizesType.GetProperty(
                    "currentGroupType",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            object currentGroupType =
                currentGroupProperty?.GetValue(
                    gameViewSizes);


            if (currentGroupType == null)
            {
                return null;
            }


            MethodInfo getGroupMethod =
                gameViewSizesType.GetMethod(
                    "GetGroup",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            return getGroupMethod?.Invoke(
                gameViewSizes,
                new[]
                {
                    currentGroupType
                });
        }


        private static int FindResolution(
            object group)
        {
            Type groupType =
                group.GetType();


            MethodInfo getTotalCountMethod =
                groupType.GetMethod(
                    "GetTotalCount",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            MethodInfo getGameViewSizeMethod =
                groupType.GetMethod(
                    "GetGameViewSize",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            if (getTotalCountMethod == null ||
                getGameViewSizeMethod == null)
            {
                return -1;
            }


            int count =
                (int)getTotalCountMethod.Invoke(
                    group,
                    null);


            for (int i = 0;
                 i < count;
                 ++i)
            {
                object size =
                    getGameViewSizeMethod.Invoke(
                        group,
                        new object[]
                        {
                            i
                        });


                if (size == null)
                {
                    continue;
                }


                Type sizeType =
                    size.GetType();


                PropertyInfo widthProperty =
                    sizeType.GetProperty(
                        "width");

                PropertyInfo heightProperty =
                    sizeType.GetProperty(
                        "height");


                if (widthProperty == null ||
                    heightProperty == null)
                {
                    continue;
                }


                int width =
                    Convert.ToInt32(
                        widthProperty.GetValue(
                            size));

                int height =
                    Convert.ToInt32(
                        heightProperty.GetValue(
                            size));


                if (width == Width &&
                    height == Height)
                {
                    return i;
                }
            }


            return -1;
        }


        private static int AddResolution(
            object group,
            Type gameViewSizeType,
            Type gameViewSizeTypeEnum)
        {
            Type groupType =
                group.GetType();


            object fixedResolution =
                Enum.Parse(
                    gameViewSizeTypeEnum,
                    "FixedResolution");


            ConstructorInfo constructor =
                gameViewSizeType.GetConstructor(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance,
                    null,
                    new[]
                    {
                        gameViewSizeTypeEnum,
                        typeof(int),
                        typeof(int),
                        typeof(string)
                    },
                    null);


            if (constructor == null)
            {
                return -1;
            }


            object size =
                constructor.Invoke(
                    new object[]
                    {
                        fixedResolution,
                        Width,
                        Height,
                        ResolutionName
                    });


            MethodInfo addCustomSizeMethod =
                groupType.GetMethod(
                    "AddCustomSize",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            if (addCustomSizeMethod == null)
            {
                return -1;
            }


            addCustomSizeMethod.Invoke(
                group,
                new[]
                {
                    size
                });


            /*
             * Find it again instead of assuming Unity appended it
             * at a particular index.
             */
            return FindResolution(
                group);
        }


        private static void SelectResolution(
            Type gameViewType,
            int index)
        {
            EditorWindow gameView =
                EditorWindow.GetWindow(
                    gameViewType);


            if (gameView == null)
            {
                return;
            }


            PropertyInfo selectedSizeIndexProperty =
                gameViewType.GetProperty(
                    "selectedSizeIndex",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);


            if (selectedSizeIndexProperty == null ||
                !selectedSizeIndexProperty.CanWrite)
            {
                return;
            }


            selectedSizeIndexProperty.SetValue(
                gameView,
                index);


            gameView.Repaint();
        }
    }
}

#endif