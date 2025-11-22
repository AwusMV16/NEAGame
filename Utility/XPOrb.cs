using UnityEngine;

public class XPOrb : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private int moveSpeed;
    private bool move;
    private Collider2D PlayerCol;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        if (move)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, PlayerCol.transform.position, moveSpeed * Time.fixedDeltaTime));
        }
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            move = true;
            PlayerCol = col;
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            move = false;
            PlayerCol = null;
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            PlayerController playerScript = col.collider.GetComponent<PlayerController>();
            playerScript.IncreaseXP();
            playerScript.Energy += 1;
            playerScript.Energy = Mathf.Min(playerScript.Energy, playerScript.MaxEnergy);
            playerScript.UpdateEnergy();
            Destroy(gameObject);
        }
    }
}
