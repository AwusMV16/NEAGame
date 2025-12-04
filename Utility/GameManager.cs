using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var spawnManager = FindAnyObjectByType<SpawnManager>();
        var roomGenRoot = FindFirstObjectByType<RoomGenRoot>();

        if (GameSession.loadSavedGame)
        {
            GameSession.playerSaveLoaded = false;
            // SaveManager.LoadBosses();
            SaveManager.LoadSeed();
        }
        else
        {
            // reset defaults for New Game
            player.Level = 1;
            player.Health = player.MaxHealth;
            roomGenRoot.seed = Random.Range(int.MinValue, int.MaxValue);

            GameSession.playerSaveLoaded = true;
            

            foreach (var boss in spawnManager.bosses)
                boss.defeated = false;

            SaveManager.ClearSave(); // optional, empties the file
        }
    }
}
