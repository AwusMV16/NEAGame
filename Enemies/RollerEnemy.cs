// using System.Numerics;
// using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RollerEnemy : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] private GameObject explosion;
    private Rigidbody2D rb;
    private RaycastHit2D rayUp;
    private RaycastHit2D rayDown;
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravityFlipCooldown = 1f; // seconds
    [SerializeField] private GameObject XPOrbPrefab;
    private float lastFlipTime = -Mathf.Infinity;
    private int currentHealth;
    [SerializeField] private int maxHealth = 100;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    public int damage = 20;
    private float direction;
    private Settings settings;

    void Start()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
        settings = FindAnyObjectByType<Settings>();

        // Set initial movement direction randomly to left (-1) or right (1)
        direction = Random.value < 0.5f ? -1f : 1f;
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        // Cast a ray upward from the enemy's position to detect the player
        rayUp = Physics2D.Raycast(transform.position, transform.up, rayLength, playerMask);

        // Cast a ray downward from the enemy's position to detect the player
        rayDown = Physics2D.Raycast(transform.position, -transform.up, rayLength, playerMask);

        if (rb.linearVelocityX == 0) // !! fixes issue where enemy gets stuck in corners
            direction = -direction;

        // Set the enemy's horizontal velocity based on direction and speed, preserving current vertical velocity
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // If a player is detected above or below and the cooldown has passed, flip gravity
        if ((rayUp.collider != null || rayDown.collider != null) && Time.time >= lastFlipTime + gravityFlipCooldown)
        {
            // Invert gravity direction
            rb.gravityScale = -rb.gravityScale;

            // Update the last time gravity was flipped
            lastFlipTime = Time.time;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        settings.IncrementStats(damageDealt: damage); 

        // If health drops to zero or below, destroy
        if (Health <= 0)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(Random.Range(1, 4), Random.Range(1, 4), 0), Quaternion.identity);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(Random.Range(1, 4), Random.Range(1, 4), 0), Quaternion.identity);
            settings.IncrementStats(enemies: 1); 
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // Bounce off walls
        direction = -direction;

        // Deal damage to player on collision
        if (other.collider.CompareTag("Player"))
        {
            // Debug.Log("Hit");
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }
}