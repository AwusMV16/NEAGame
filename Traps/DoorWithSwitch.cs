using UnityEngine;

public class DoorWithSwitch : MonoBehaviour
{
    [SerializeField] private GameObject door;
    private bool open = false;
    private bool playerInRange = false;
    private Animator anim;
    [SerializeField] private GameObject text;

    void Start()
    {
        anim = GetComponent<Animator>();
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
}
