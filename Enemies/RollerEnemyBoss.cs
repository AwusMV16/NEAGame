using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RollerEnemyBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosion;
    private RaycastHit2D rayRight;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private float gravityInterval;
    [SerializeField] private float attackCooldown = 5f; // seconds between charges
    private float attackTimer = 0f;
    [SerializeField] private float chargeMoveSpeed;
    private float gravityTimer = 0f;
    public int Health;
    public int damage;
    private float direction;
    // private bool canCharge;
    public Transform player; // set from spawner
    private Settings settings;
    private Animator anim;
    private bool isAttacking;
    private int healthBeforeDamage;
    private SpawnManager spawnManager;
    private BossHealthBar healthBar;
    private bool crossedThreshold;
    public BossDoorManager doorManager;

    void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
        settings = FindAnyObjectByType<Settings>();
        anim = GetComponent<Animator>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
        healthBar = FindAnyObjectByType<BossHealthBar>();

        UpdateHealth();
    }

    void Start()
    {
        // Set initial movement direction randomly to left (-1) or right (1)
        direction = Random.value < 0.5f ? -1f : 1f;

        healthBeforeDamage = Health;
        healthBar.SetVisible(true);
    }

    void FixedUpdate()
    {
        // Increment timer
        attackTimer += Time.deltaTime;

        // Check if cooldown is finished
        if (attackTimer >= attackCooldown)
        {
            StartCoroutine(LaserAttack());
            attackTimer = 0f; // reset cooldown
        }
        else
        {
            HandleNormalBehaviour();
        }
    }

    private IEnumerator LaserAttack()
    {
        string[] laserAttacks = { "LaserBeam1", "LaserBeam2", "LaserBeam3", "LaserBeam4", "LaserBeam5", "LaserBeam5" };
        string chosenAttack = laserAttacks[Random.Range(0, laserAttacks.Length)];

        isAttacking = true;
        anim.Play(chosenAttack, 0, 0f);
        yield return new WaitForSeconds(3f);
        isAttacking = false;
    }

    private void HandleNormalBehaviour()
    {
        // Increment timer by the time elapsed since last frame
        gravityTimer += Time.deltaTime;

        // Check if it's time to shoot
        if (gravityTimer >= gravityInterval && !isAttacking)
        {
            gravityTimer = 0f; // Reset timer
            rb.gravityScale *= -1;
        }

        if (rb.linearVelocityX == 0) // fixes issue where boss gets stuck in corners
            direction = -direction;

        // Set the enemy's horizontal velocity based on direction and speed, preserving current vertical velocity
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    public void TakeDamage(int damage)
    {
        int nSegments = 4;
        int segmentSize = healthBeforeDamage / nSegments;
        int prevHealth = Health; // store health before damage
        int prevSegment = Mathf.FloorToInt((prevHealth - 1) / segmentSize);

        Health -= damage;
        // anim.SetTrigger("Hit");

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
            direction = -direction;
        }
        // If health drops to zero or below, destroy
        if (Health <= 0)
        {
            spawnManager.MarkBossDefeated(1);
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            for (int i = 0; i < 25; i++)
            {
                Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), Quaternion.identity, transform.parent);
            }
            doorManager.OpenDoors();    
            healthBar.SetVisible(false);
            if (settings != null) settings.IncrementStats(enemies: 1);
            Destroy(gameObject);
        }
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        healthBar.UpdateBossHealth(Health, healthBeforeDamage);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // Check if the collision was with a side wall
        foreach (ContactPoint2D contact in other.contacts)
        {
            // If the wall is mostly horizontal (normal pointing left or right)
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                direction = -direction;
                break; // Don’t need to check other contacts
            }
        }

        // Deal damage to player on collision   
        if (other.collider.CompareTag("Player"))
        {
            // Debug.Log("Hit");
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            IDamageable target = col.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }
}