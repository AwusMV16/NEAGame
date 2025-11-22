using UnityEngine;

public class BossDoorManager : MonoBehaviour
{
    [SerializeField] private GameObject entranceDoor;
    [SerializeField] private GameObject exitDoor;
    [SerializeField] private int bossID;
    private SpawnManager spawnManager;
    private BossHealthBar bossHealthBar;
    private bool closed;

    void Awake()
    {
        bossHealthBar = FindAnyObjectByType<BossHealthBar>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (spawnManager != null)
        {
            if (col.CompareTag("Player") && !closed)
            {
                if (!spawnManager.bossDefeated(bossID)) CloseDoors();
            }
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") && closed)
        {
            OpenDoors();
            closed = false;
        }
    }

    public void OpenDoors()
    {
        entranceDoor.GetComponentInChildren<Animator>().SetBool("Open", true);
        exitDoor.GetComponentInChildren<Animator>().SetBool("Open", true);
    }

    private void CloseDoors()
    {
        closed = true;
        entranceDoor.GetComponentInChildren<Animator>().SetBool("Open", false);
        exitDoor.GetComponentInChildren<Animator>().SetBool("Open", false);
        
        bossHealthBar.SetVisible(true);
        if (spawnManager.activeBoss == null) spawnManager.SpawnBoss(bossID);
    }
}
