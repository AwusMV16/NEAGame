using UnityEngine;

public class FogWall : MonoBehaviour
{
    private bool open = false;
    private bool playerInRange = false;
    [SerializeField] private GameObject text;
    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && !open)
        {
            text.SetActive(true);
            if (Input.GetKey(KeyCode.E))
            {
                if (anim != null)
                {
                    open = true;
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
            Debug.Log("In Player");
            playerInRange = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log("Out Player");
            playerInRange = false;
            text.SetActive(false);

        }
    }
}
