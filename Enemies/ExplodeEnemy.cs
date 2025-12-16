using Unity.Mathematics;
using UnityEngine;

public class ExplodeEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float followRadius;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed;
    private float direction; // move direction in X axis
    private bool isEngaged; 
    private GameObject player;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject XPOrbPrefab;
    private Animator animator;
    public int damage = 20;
    private int currentHealth;
    public int maxHealth = 100;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    private Settings settings;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        direction = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        animator = GetComponent<Animator>();
        settings = FindAnyObjectByType<Settings>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        animator.SetBool("isEngaged", isEngaged);
    }

    void FixedUpdate()
    {
        if (!isEngaged) // back and forth movement when player is not detected
        {
            if (rb.linearVelocity.x == 0)
                direction = -direction; // reverse the horizontal movement direction when the player is stuck (not moving)

            // set the velocity based on these directions * move speed
            rb.linearVelocity = new Vector2(direction * moveSpeed, 0f); 
        }
        else if (isEngaged && player != null) // orbit the player when they are within detection radius
        {
            // Move towards player if outside followRadius

            Vector2 moveDir = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = moveDir * moveSpeed * 3f;
        }
        else // if none of the previous conditions are met...
        {
            
            isEngaged = false; // stop pursuing the player
            // set the velocity back to back and forth movement
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocityY * moveSpeed); 
        }
    }

    public void InitializeHealth(int startingHealth)
    {
        currentHealth = startingHealth;
        maxHealth = startingHealth;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(!isEngaged) direction *= -1f;
        if (other.collider.CompareTag("Player"))
        {
            //explode
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    // Reduces health when damaged and triggers hit animation
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        if(settings != null) settings.IncrementStats(damageDealt: dmg); 
        // If health drops to zero or below, destroy.
        if (Health <= 0)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
            if(settings != null) settings.IncrementStats(enemies: 1); 
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col) // player triggers detection circle
    {
        if (col.CompareTag("Player")) // check for "Player" tag
        {
            player = col.gameObject; // saves player gameObject for info
            isEngaged = true; // pursue the player
        }
    }

    void OnTriggerExit2D(Collider2D col) // player leaves detection circle
    {
        if (col.CompareTag("Player")) // check for "Player" tag
        {
            player = null; // reset the player variable
            isEngaged = false; // stop pursuing the player
        }
    }
}
