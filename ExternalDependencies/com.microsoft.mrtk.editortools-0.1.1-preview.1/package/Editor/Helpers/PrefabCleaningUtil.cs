using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    [InitializeOnLoad]
    public static class PrefabCleaningUtil
    {
        private static HashSet<GameObject> instanceOverrides = new HashSet<GameObject>();

        private static int totalOverrideCount = 0;
        private static int cleanOverrideCount = 0;

        static PrefabCleaningUtil()
        {
            // PrefabStage.prefabSaving += TryRevertMatchingOverrides;
            // EditorSceneManager.sceneSaving += OnBeforeSceneSaved;
        }

        [MenuItem("Mixed Reality/MRTK3/Utilities/Clean Local Prefabs (experimental)", false, 3)]
        public static void CleanLocalPrefabOverrides()
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            Scene currentScene = EditorSceneManager.GetActiveScene();

            totalOverrideCount = 0;
            cleanOverrideCount = 0;

            Debug.Log("Searching for local prefab overrides that can be cleaned...");

            EditorApplication.delayCall += () =>
            {
                if (stage != null)
                {
                    TryRevertMatchingOverrides(stage.prefabContentsRoot);
                }
                else if (currentScene != null)
                {
                    TryRevertSceneOverrides(currentScene);
                }

                if (cleanOverrideCount > 0)
                {
                    if (stage != null)
                    {
                        EditorUtility.SetDirty(stage.prefabContentsRoot);
                    }
                    else
                    {
                        EditorSceneManager.MarkSceneDirty(currentScene);
                    }
                    Debug.Log("Cleaned!");
                    Debug.LogFormat($"<b>{cleanOverrideCount}/{totalOverrideCount}</b> local prefab overrides were reverted because they were <i>identical</i> to their base prefab values.");
                }
                else
                {
                    Debug.Log("Nothing to clean.");
                    if (totalOverrideCount > 0)
                    {
                        Debug.Log("All local prefab overrides are <i>actually</i> different from their base prefab values.");
                    }
                }
            };

        }

        private static void TryRevertMatchingOverrides(GameObject rootObject)
        {
            if (!StageUtility.GetCurrentStageHandle().IsValid())
            {
                return;
            }

            instanceOverrides.Clear();

            Transform[] transforms = rootObject.GetComponentsInChildren<Transform>(true);

            foreach (var transform in transforms)
            {
                GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(transform);

                if (instanceRoot != null && !instanceOverrides.Contains(instanceRoot))
                {
                    instanceOverrides.Add(instanceRoot);

                    if (PrefabUtility.HasPrefabInstanceAnyOverrides(instanceRoot, true))
                    {
                        RevertPrefabInstanceMatchingOverrides(instanceRoot);
                    }
                }
            }
        }

        private static void RevertPrefabInstanceMatchingOverrides(GameObject prefabInstance)
        {
            SerializedObject serializedInstance = new SerializedObject(prefabInstance);
            GameObject sourceObject = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstance);

            if (sourceObject == null)
            {
                return;
            }

            SerializedObject serializedSource = new SerializedObject(sourceObject);

            RevertMatchingPrefabOverridesForSingleObject(serializedInstance, serializedSource);
            Component[] components = prefabInstance.GetComponentsInChildren<Component>(true);

            foreach (Component instanceComponent in components)
            {
                if (instanceComponent == null)
                {
                    continue;
                }

                Component sourceComponent = PrefabUtility.GetCorrespondingObjectFromSource(instanceComponent);

                if (sourceComponent == null)
                {
                    continue;
                }

                if (instanceComponent is Transform)
                {
                    GameObject sourceChildObject = PrefabUtility.GetCorrespondingObjectFromSource(instanceComponent.gameObject);
                    if (sourceChildObject != null)
                    {
                        RevertMatchingPrefabOverridesForSingleObject(new SerializedObject(instanceComponent.gameObject), new SerializedObject(sourceChildObject));
                    }
                }

                RevertMatchingPrefabOverridesForSingleObject(new SerializedObject(instanceComponent), new SerializedObject(sourceComponent));
            }
        }

        private static void RevertMatchingPrefabOverridesForSingleObject(SerializedObject serializedInstance, SerializedObject serializedSource)
        {
            var instanceProperty = serializedInstance.GetIterator();
            do
            {
                if (!instanceProperty.prefabOverride || instanceProperty.hasChildren)
                {
                    continue;
                }

                if (!instanceProperty.hasChildren)
                {
                    totalOverrideCount++;
                }

                SerializedProperty sourceProperty = serializedSource.FindProperty(instanceProperty.propertyPath);
                if (sourceProperty == null)
                {
                    //Debug.Log($"Hm.... {instanceProperty.propertyPath}", instanceProperty.serializedObject.targetObject);
                    continue;
                }

                bool approximateMatchingFloats = SerializedPropertyExtensions.HasApproximateMatchingFloatValues(instanceProperty, sourceProperty);
                bool matchingValue = SerializedPropertyExtensions.HasMatchingValue(instanceProperty, sourceProperty);
                Transform transformInstance = serializedInstance.targetObject as Transform;
                bool transformRootOrder = transformInstance != null && instanceProperty.propertyPath == "m_RootOrder" && !PrefabUtility.IsOutermostPrefabInstanceRoot(transformInstance.gameObject);

                if (!approximateMatchingFloats && !matchingValue && instanceProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    matchingValue = instanceProperty.objectReferenceValue != null && sourceProperty.objectReferenceValue != null && PrefabUtility.GetCorrespondingObjectFromSource(instanceProperty.objectReferenceValue) == sourceProperty.objectReferenceValue;
                }

                if (approximateMatchingFloats || matchingValue || transformRootOrder)
                {
#if true
                    if (!transformRootOrder)
                    {
                        string[] type = serializedInstance.targetObject.GetType().ToString().Split('.');
                        string objectName = string.Format($"<b>{serializedInstance.targetObject.name}</b>");
                        string componentType = ColorLogString("orange", $"[{type[type.Length - 1]}]");
                        string propertyPath = ColorLogString("yellow", $"[{instanceProperty.propertyPath}]");
                        string sourceValue = ColorLogString("lime", $"[{sourceProperty.GetValueAsString()}]");

                        Debug.Log($"Cleaning {objectName}'s {componentType}.{propertyPath} local prefab override\nwas <i>identical</i> to its Prefab Source value of {sourceValue}", instanceProperty.serializedObject.targetObject);
                    }
#endif              
                    PrefabUtility.RevertPropertyOverride(instanceProperty, InteractionMode.AutomatedAction);
                    cleanOverrideCount++;
                }
                instanceProperty.serializedObject.ApplyModifiedProperties();
            } while (instanceProperty.Next(true));
        }

        private static string ColorLogString(string color, string log)
        {
            return string.Format("<color={0}>{1}</color>", color, log);
        }

        private static void TryRevertSceneOverrides(Scene scene, string scenePath = null)
        {
            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            foreach (var gameObject in rootGameObjects)
            {
                TryRevertMatchingOverrides(gameObject);
            }
        }
    }
}
