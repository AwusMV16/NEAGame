using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public static class SaveManager
{
    private static string path => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(Dictionary<string, string> data)
    {
        List<string> lines = new();
        foreach (var keyValue in data)
        {
            lines.Add($"{keyValue.Key}={keyValue.Value}");
        }

        File.WriteAllLines(path, lines);
    }
    
    public static Dictionary<string, string> Load()
    {
        Dictionary<string, string> data = new();

        if (!File.Exists(path)) return data;

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var split = line.Split("=");

            if (split.Length == 2)
            {
                string key = split[0];
                string value = split[1];

                data[key] = value;
            }
        }

        return data;
    }

    public static void SavePlayer()
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        var data = Load();

        data["Level"] = player.Level.ToString();
        data["Health"] = player.Health.ToString();
        data["Energy"] = player.Energy.ToString();
        data["PosX"] = player.transform.position.x.ToString(CultureInfo.InvariantCulture);
        data["PosY"] = player.transform.position.y.ToString(CultureInfo.InvariantCulture);
        data["RotZ"] = player.transform.eulerAngles.z.ToString(CultureInfo.InvariantCulture);
        data["ClimbEnabled"] = player.climbEnabled ? "1" : "0";
        
        // Debug.Log(path);
        Save(data);
    }

    public static void LoadPlayer()
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        var data = Load();

        if (data.TryGetValue("Level", out var level))
        {
            player.Level = int.Parse(level);
            player.UpdateStatsFromLevel();
            player.UpdateLevelText(int.Parse(level));
        }

        if (data.TryGetValue("Health", out var health))
        {
            player.Health = int.Parse(health);
            player.UpdateHealth();
        }

        if (data.TryGetValue("Energy", out var energy))
        {
            player.Energy = int.Parse(energy);
            player.UpdateEnergy();
        }

        if (data.TryGetValue("PosX", out var PosX) && data.TryGetValue("PosY", out var PosY))
        {
            if (float.TryParse(PosX, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && float.TryParse(PosY, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                var position = player.transform.position; // copy
                position.x = x;
                position.y = y;
                player.transform.position = position;     // assign back
            }
        }

        if (data.TryGetValue("ClimbEnabled", out var climbEnabled))
        {
            player.climbEnabled = climbEnabled == "1";
        }

        if (data.TryGetValue("RotZ", out var rotZ))
        {
            if (float.TryParse(rotZ, NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                player.transform.rotation = Quaternion.Euler(0, 0, z);
            }
        }

        GameSession.playerSaveLoaded = true;
    }

    public static void SaveSeed()
    {
        var roomGenRoot = Object.FindFirstObjectByType<RoomGenRoot>();
        if (roomGenRoot == null) return;

        var data = Load();

        data["Seed"] = roomGenRoot.seed.ToString();

        Save(data);
    }

    public static void LoadSeed()
    {
        var roomGenRoot = Object.FindFirstObjectByType<RoomGenRoot>();
        if (roomGenRoot == null) return;

        var data = Load();

        if (data.TryGetValue("Seed", out var seed))
        {
            roomGenRoot.seed = int.Parse(seed);
        }
    }

    public static void SaveSpawnAnchor(string ID)
    {
        var data = Load();

        data["ActiveAnchorID"] = ID;

        Save(data);
    }

    public static void LoadSpawnAnchor()
    {
        var data = Load();

        if (data.TryGetValue("ActiveAnchorID", out var _ID))
        {
            SpawnAnchor[] anchors = Object.FindObjectsByType<SpawnAnchor>(FindObjectsSortMode.None);
            foreach (var anchor in anchors)
            {
                if (anchor.ID == _ID)
                {
                    anchor.Activate();
                    break;
                }
            }
        }
    }

    public static void SaveBosses(SpawnManager spawnManager)
    {
        if (spawnManager == null) return;

        var data = Load();

        // Save defeated state for each boss
        for (int i = 0; i < spawnManager.bosses.Count; i++)
        {
            data[$"Boss_{i}_Defeated"] = spawnManager.bosses[i].defeated.ToString();
        }

        Save(data);
    }

    public static void LoadBosses(SpawnManager spawnManager)
    {
        if (spawnManager == null) return;

        var data = Load();

        for (int i = 0; i < spawnManager.bosses.Count; i++)
        {
            if (data.TryGetValue($"Boss_{i}_Defeated", out var defeatedStr))
            {
                spawnManager.bosses[i].defeated = bool.Parse(defeatedStr);
            }
        }
    }

    public static void ClearSave()
    {
        if (File.Exists(path))
        {
            File.WriteAllText(path, ""); // empty the file
            // Debug.Log("Save file cleared.");
        }
    }
}
