using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossData
{
    public string bossName;
    public GameObject bossPrefab;
    public Transform spawnPoint;
    [HideInInspector] public bool defeated;
}

public class SpawnManager : MonoBehaviour
{
    public GameObject ActiveAnchor;
    private List<EnemySpawner> spawners = new();

    [Header("Boss Settings")]
    [SerializeField] private List<BossData> bosses = new(); // all bosses
    public GameObject activeBoss;
    private BossData currentBoss;
    private int currentBossIndex = -1;
    private BossHealthBar bossHealthBar;
    // private BossDoorManager doorManager;

    void Awake()
    {
        bossHealthBar = FindAnyObjectByType<BossHealthBar>();
    }

    public void SetActiveAnchor(GameObject newAnchor)
    {
        void ToggleAnchor(GameObject anchor, bool state)
        {
            var anim = anchor.GetComponent<Animator>();
            var spawn = anchor.GetComponent<SpawnAnchor>();
            anim.SetBool("Active", state);
            spawn.isActive = state;
        }

        if (ActiveAnchor != null) ToggleAnchor(ActiveAnchor, false);

        ActiveAnchor = newAnchor;
        ToggleAnchor(ActiveAnchor, true);
    }

    public void RegisterEnemySpawner(EnemySpawner spawner)
    {
        spawners.Add(spawner);
    }

    public void SpawnBoss(int bossIndex)
    {
        var bossData = bosses[bossIndex];

        if (bossData.defeated)
        {
            Debug.Log($"{bossData.bossName} already defeated - skipping spawn.");
            return;
        }

        bossHealthBar.SetVisible(true);
        if (bossIndex < 0 || bossIndex >= bosses.Count)
        {
            Debug.LogWarning("Invalid boss index!");
            return;
        }

        // Destroy old one if still around
        if (activeBoss != null)
            Destroy(activeBoss);
            activeBoss = null;

        currentBossIndex = bossIndex;
        currentBoss = bosses[bossIndex];

        Transform spawnPos = currentBoss.spawnPoint;

        // If no spawnPoint assigned manually, look for the marker with matching ID
        if (spawnPos == null)
        {
            BossSpawnPoint[] markers = FindObjectsByType<BossSpawnPoint>(sortMode: FindObjectsSortMode.None);
            foreach (var marker in markers)
            {
                if (marker.ID == bossIndex)
                {
                    spawnPos = marker.transform;
                    break;
                }
            }
        }

        if (spawnPos == null)
        {
            Debug.LogWarning($"No BossSpawnPoint found for {currentBoss.bossName} (index {bossIndex})!");
            return;
        }

        // Instantiate selected boss
        activeBoss = Instantiate(currentBoss.bossPrefab, spawnPos.position, Quaternion.identity, spawnPos.parent);

        if (activeBoss.TryGetComponent<SpinnerBoss>(out var spinner))
        {
            spinner.doorManager = spawnPos.parent.Find("BossArea").GetComponent<BossDoorManager>();
        }
        else if (activeBoss.TryGetComponent<RollerEnemyBoss>(out var roller))
        {
            roller.doorManager = spawnPos.parent.Find("BossArea").GetComponent<BossDoorManager>();
        }
    }

    public void MarkBossDefeated(int bossID)
    {
        if(currentBossIndex >= 0 && currentBossIndex < bosses.Count)
        {
            bosses[bossID].defeated = true;
            Debug.Log($"{bosses[bossID].bossName} marked as defeated!");
        }
    }

    public void ResetBoss()
    {
        if (currentBoss == null || currentBossIndex < 0) return;
        if (bosses[currentBossIndex].defeated) return;
        if (activeBoss != null) Destroy(activeBoss);

        bossHealthBar.SetVisible(false);
    }

    public void RespawnEnemies()
    {
        // Remove any destroyed spawners first
        spawners.RemoveAll(s => s == null);

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.DestroyEnemies();
            spawner.SpawnEnemies();
        }
    }

    public bool bossDefeated(int bossIndex)
    {
        return bosses[bossIndex].defeated;
    }
}
