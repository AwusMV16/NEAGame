using UnityEditor;
using UnityEngine;
using System.IO;

public class AnchorIDGenerator
{
    [MenuItem("Tools/Assign Anchor IDs to All Anchors (Scene + Prefabs)")]
    public static void AssignIDs()
    {
        int count = 0;

        // 1. Assign IDs to anchors in the currently open scene
        SpawnAnchor[] sceneAnchors = Object.FindObjectsByType<SpawnAnchor>(FindObjectsSortMode.None);
        foreach (var anchor in sceneAnchors)
        {
            if (string.IsNullOrEmpty(anchor.ID))
            {
                anchor.ID = System.Guid.NewGuid().ToString();
                count++;
                EditorUtility.SetDirty(anchor);
            }
        }

        // 2. Assign IDs to anchors inside prefabs in the project
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            SpawnAnchor[] anchorsInPrefab = prefab.GetComponentsInChildren<SpawnAnchor>(true);
            foreach (var anchor in anchorsInPrefab)
            {
                if (string.IsNullOrEmpty(anchor.ID))
                {
                    anchor.ID = System.Guid.NewGuid().ToString();
                    count++;

                    // Mark prefab as dirty so changes are saved
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }

        Debug.Log($"Assigned {count} new IDs to anchors (scene + prefabs).");
    }
}
