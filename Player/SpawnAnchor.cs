using UnityEngine;

public class SpawnAnchor : MonoBehaviour
{
    private bool playerInRange;
    public bool isActive;
    [SerializeField] private GameObject text;
    private SpawnManager spawnManager;
    public string ID;

    void Awake()
    {
        spawnManager = FindFirstObjectByType<SpawnManager>();
    }

    void Update()
    {
        if (playerInRange && !isActive)
        {
            text.SetActive(true);
            if (Input.GetKey(KeyCode.E))
            {
                Activate();
            }
        }
    }

    public void Activate()
    {
        SpawnManager spawnManagerScript = spawnManager.GetComponent<SpawnManager>();
        spawnManagerScript.SetActiveAnchor(gameObject);
        SaveManager.SaveSpawnAnchor(ID);
        text.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            text.SetActive(false);
        }
    }
}
