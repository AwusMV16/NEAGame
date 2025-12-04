using UnityEditor;
using UnityEngine;

public class DoorIDGenerator
{
    [MenuItem("Tools/Assign Door IDs to All Doors (Scene + Prefabs)")]
    public static void AssignIDs()
    {
        int count = 0;

        // 1. Assign IDs to doors in the currently open scene
        DoorWithSwitch[] sceneDoors = Object.FindObjectsByType<DoorWithSwitch>(FindObjectsSortMode.None);
        foreach (var door in sceneDoors)
        {
            if (string.IsNullOrEmpty(door.ID))
            {
                door.ID = System.Guid.NewGuid().ToString();
                count++;
                EditorUtility.SetDirty(door); // mark scene object as dirty so it saves
            }
        }

        // 2. Assign IDs to doors inside prefabs in the project
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            DoorWithSwitch[] doorsInPrefab = prefab.GetComponentsInChildren<DoorWithSwitch>(true);
            foreach (var door in doorsInPrefab)
            {
                if (string.IsNullOrEmpty(door.ID))
                {
                    door.ID = System.Guid.NewGuid().ToString();
                    count++;

                    // Mark prefab as dirty so changes are saved
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }

        Debug.Log($"Assigned {count} new IDs to doors (scene + prefabs).");
    }
}
