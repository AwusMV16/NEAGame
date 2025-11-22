using Unity.Mathematics;
using UnityEngine;

public class SpinnerBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject spinnerPrefab;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private float cloneHealthLimit;
    [SerializeField] private Animator animator;
    public int Health;
    public int damage = 20;
    private int healthBeforeDamage;
    private Rigidbody2D rb;
    public float bulletInterval = 1f; // Every 1s
    private float bulletTimer = 0f;
    public BossDoorManager doorManager;
    private SpawnManager spawnManager;
    private BossHealthBar healthBar;
    private Settings settings;


    void Awake()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
        healthBar = FindAnyObjectByType<BossHealthBar>();
        settings = FindAnyObjectByType<Settings>();
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(
            (UnityEngine.Random.Range(0, 2) * 2 - 1) * moveSpeed,
            (UnityEngine.Random.Range(0, 2) * 2 - 1) * moveSpeed
        );

        // Store health before any damage is taken
        healthBeforeDamage = Health;
        healthBar.SetVisible(true);
        UpdateHealth();
    }

    void Update()
    {
        // Increment timer by the time elapsed since last frame
        bulletTimer += Time.deltaTime;

        // Check if it's time to shoot
        if (bulletTimer >= bulletInterval)
        {
            bulletTimer = 0f; // Reset timer
            Shoot(new Vector3(0, 3, 0), transform.up, Quaternion.Euler(0, 0, 0f));
            Shoot(new Vector3(3, 0, 0), transform.right, Quaternion.Euler(0, 0, 270f));
            Shoot(new Vector3(0, -3, 0), -transform.up, Quaternion.Euler(0, 0, 180f));
            Shoot(new Vector3(-3, 0, 0), -transform.right, Quaternion.Euler(0, 0, 90f));
        }
    }

    public void UpdateHealth()
    {
        healthBar.UpdateBossHealth(Health, healthBeforeDamage);
    }

    void Shoot(Vector3 offset, Vector3 direction, Quaternion rotationOffset)
    {
        // multiplying quaternions *combines* rotation
        GameObject projectile = Instantiate(bulletPrefab, transform.position + offset, transform.rotation * Quaternion.Euler(0, 0, -90f) * rotationOffset); // create an instance of the bullet at the firing position
        Bullet bulletScript = projectile.GetComponent<Bullet>();
        bulletScript.damage = damage; // set player attributes like damage, tag to ignore and the tag to attack and the color of the bullet 
        bulletScript.ignoreTag = tag;
        bulletScript.attackTag = "Player";
        bulletScript.bulletColor = new Color(0.157f, 0.675f, 0.773f);  // set the custom color of the bullet
        bulletScript.bulletTag = "EnemyBullet";
        Rigidbody2D bulletRb = projectile.GetComponent<Rigidbody2D>();
        bulletRb.linearVelocity = direction * 20f; // the bullet travels in the up direction of the enemy  trasform.up for up 
    }

    // Reduces health when damaged and triggers hit animation
    public void TakeDamage(int damage)
    {
        int nSegments = 4;
        int segmentSize = healthBeforeDamage / nSegments;
        int prevHealth = Health; // store health before damage
        int prevSegment = Mathf.FloorToInt((prevHealth - 1) / segmentSize);

        Health -= damage;
        animator.SetTrigger("Hit");

        if (settings != null) settings.IncrementStats(damageDealt: damage); 

        int newSegment = Mathf.FloorToInt((Health - 1) / segmentSize);
        bool crossedThreshold = newSegment < prevSegment;

        // If health drops to zero or below, destroy and possibly replicate
        if (crossedThreshold)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            transform.localScale *= 0.9f;
            moveSpeed *= 1.1f;
            rb.linearVelocity = new Vector2(
                (UnityEngine.Random.Range(0, 2) * 2 - 1) * moveSpeed,
                (UnityEngine.Random.Range(0, 2) * 2 - 1) * moveSpeed
            );
        }
        if (Health <= 0)
        {
            spawnManager.MarkBossDefeated(0);
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            for (int i = 0; i < 25; i++)
            {
                Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity, transform.parent);
            }
            doorManager.OpenDoors();
            healthBar.SetVisible(false);
            if(settings != null) settings.IncrementStats(enemies: 1); 
            Destroy(gameObject);
        }
        UpdateHealth();
    }

    // Handles collisions with walls and the player
    void OnCollisionEnter2D(Collision2D other)
    {
        // Deal damage to player on collision
        if (other.collider.CompareTag("Player"))
        {
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
        // Preserve speed after collision
        rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
    }

    void OnDestroy()
    {
        if (healthBar.isActiveAndEnabled)
        {
            healthBar.SetVisible(false);
        }
    }
}