using UnityEngine;

public class DoorWithSwitch : MonoBehaviour
{
    [SerializeField] private GameObject door;
    private bool open = false;
    private bool playerInRange = false;
    private Animator anim;
    [SerializeField] private GameObject text;
    public string ID;

    void Start()
    {
        anim = GetComponent<Animator>();
        LoadState();
    }
    void Update()
    {
        if (playerInRange && !open)
        {
            text.SetActive(true);
            if (Input.GetKey(KeyCode.E))
            {
                Animator doorAnim = door.GetComponent<Animator>();
                if (doorAnim != null)
                {
                    open = true;
                    doorAnim.SetTrigger("Open");
                    anim.SetTrigger("Open");
                    text.SetActive(false);
                    SaveState();
                }
            }
        }
    }

    void LateUpdate()
    {
        // Keep the text upright, only inherit position from parent
        text.transform.rotation = Quaternion.identity;
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

    private void SaveState()
    {
        var data = SaveManager.Load();
        data[$"Door_{ID}"] = open ? "1" : "0"; // save as 1=open, 0=closed
        SaveManager.Save(data);
    }

    private void LoadState()
    {
        var data = SaveManager.Load();
        if (data.TryGetValue($"Door_{ID}", out var state))
        {
            open = state == "1";
            if (open)
            {
                Animator doorAnim = door.GetComponent<Animator>();
                if (doorAnim != null) doorAnim.SetTrigger("Open");
                anim.SetTrigger("Open");
            }
        }
    }
}
