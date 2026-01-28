using UnityEngine;

public class AutomaticDoors : MonoBehaviour
{
    private GameObject door;
    private Animator anim;
    private bool open = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        door = transform.Find("Door").gameObject;
        anim = door.GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && open == false)
        {
            anim.SetBool("Open", true);
            open = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") && open == true)
        {
            anim.SetBool("Open", false);
            open = false;
        }
    }
}
