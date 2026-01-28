using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RollerEnemyBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosion;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private float gravityInterval;
    [SerializeField] private float attackCooldown = 5f; // seconds between charges
    private float attackTimer = 0f;
    private float gravityTimer = 0f;
    public int Health;
    public int damage;
    private float direction;
    public GameObject player;
    private Settings settings;
    private bool isAttacking;
    private int healthBeforeDamage;
    private SpawnManager spawnManager;
    private BossHealthBar healthBar;
    public BossDoorManager doorManager;
    private LineRenderer line;
    [SerializeField] private Transform aimTarget;
    private SpriteRenderer aimTargetVisual;
    [SerializeField] private float aimDelaySeconds = 1f;
    private Queue<(float time, Vector2 position)> playerHistory = new Queue<(float, Vector2)>();
    private Vector2 laserEndPoint;

    private AudioSource bossMusicSource;
    [SerializeField] private float musicFadeSpeed = 1.5f;
    private bool isDying = false;
    private Light2D aimTargetLight;

    void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
        settings = FindAnyObjectByType<Settings>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
        healthBar = FindAnyObjectByType<BossHealthBar>();
        bossMusicSource = GetComponent<AudioSource>();
        line = GetComponentInChildren<LineRenderer>();
        player = GameObject.FindWithTag("Player");
        aimTargetVisual = aimTarget.GetComponent<SpriteRenderer>();
        aimTargetLight = aimTarget.GetComponentInChildren<Light2D>();
        line.useWorldSpace = true;

        UpdateHealth();
    }

    void Start()
    {
        // Set initial movement direction randomly to left (-1) or right (1)
        direction = Random.value < 0.5f ? -1f : 1f;

        healthBeforeDamage = Health;
        healthBar.SetVisible(true);

        aimTarget.SetParent(null);
        line.enabled = false;
        aimTargetVisual.enabled = false;
        aimTargetLight.enabled = false;
    }

    void FixedUpdate()
    {
        line.SetPosition(0, transform.position);

        // Increment timer
        attackTimer += Time.deltaTime;

        // Check if cooldown is finished
        if (attackTimer >= attackCooldown)
        {
            bool playerBelow = Mathf.Abs(player.transform.position.y - transform.position.y) > 5;

            // Only attack if player is on opposite side of gravity
            if (playerBelow)
            {
                StartCoroutine(LaserAttack());
                attackTimer = 0f; // reset cooldown
            }
            else
            {
                HandleNormalBehaviour();
                if (!isAttacking)
                {
                    aimTargetVisual.enabled = true;
                    aimTargetLight.enabled = true;
                }
            }
        }
        else
        {
            if (!isAttacking)
            {
                aimTargetVisual.enabled = true;
                aimTargetLight.enabled = true;
            }
            HandleNormalBehaviour();
        }


        // Constantly check laser hit while laser is active
        if (isAttacking)
        {
            CheckLaserHit();
        }
    }

    void Update()
    {
        if (player != null)
        {
            playerHistory.Enqueue((Time.time, player.transform.position));

            // Remove entries older than we need
            while (playerHistory.Count > 0 &&
                Time.time - playerHistory.Peek().time > aimDelaySeconds)
            {
                playerHistory.Dequeue();
            }
        }

        if (isDying && bossMusicSource != null)
        {
            bossMusicSource.volume -= musicFadeSpeed * Time.deltaTime;
            if (bossMusicSource.volume <= 0f)
            {
                bossMusicSource.Stop();
                Destroy(gameObject);
            }
            return; // stop boss logic while dying
        }

        Vector2 targetPos = player.transform.position;
        if (playerHistory.Count > 0)
        {
            targetPos = playerHistory.Peek().position;
        }

        Vector2 rot = (targetPos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rot, 50, LayerMask.GetMask("Wall"));


        if(hit.collider != null && player != null)
        {
            aimTarget.position = hit.point;
        }
    }

    private IEnumerator LaserAttack()
    {
        isAttacking = true;

        line.enabled = true;

        aimTargetVisual.enabled = false;

        Vector2 start = transform.position;
        Vector2 end = aimTarget.position;
        Vector2 dir = (end - start).normalized;
        float extraLength = 0.5f; // tweak (world units)

        laserEndPoint = end + dir * extraLength;
        

        line.SetPosition(1, laserEndPoint);

        yield return new WaitForSeconds(2f); //3s

        line.enabled = false;
        isAttacking = false;
    }

    private void CheckLaserHit()
    {
        Vector2 start = transform.position;
        Vector2 end = laserEndPoint;
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        float laserWidth = line.startWidth/2;
        
        Collider2D hit = Physics2D.OverlapBox(
            start + dir * distance * 0.5f,
            new Vector2(distance, laserWidth),
            Vector2.SignedAngle(Vector2.right, dir),
            LayerMask.GetMask("Player")
        );

        if (hit != null)
        {
            Debug.Log("hit");
            hit.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (aimTarget == null) return;

        Vector2 start = transform.position;
        Vector2 end = laserEndPoint;
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        float laserWidth = line != null ? line.startWidth/2 : 0.2f;

        Vector2 center = start + dir * distance * 0.5f;
        float angle = Vector2.SignedAngle(Vector2.right, dir);

        Gizmos.color = Color.red;
        DrawRotatedBox(center, new Vector2(distance, laserWidth), angle);
    }

    void DrawRotatedBox(Vector2 center, Vector2 size, float angle)
    {
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Vector2 half = size * 0.5f;

        Vector2[] corners =
        {
            center + (Vector2)(rot * new Vector3(-half.x, -half.y)),
            center + (Vector2)(rot * new Vector3(-half.x,  half.y)),
            center + (Vector2)(rot * new Vector3( half.x,  half.y)),
            center + (Vector2)(rot * new Vector3( half.x, -half.y)),
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
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
            Destroy(aimTarget.gameObject);
            spawnManager.MarkBossDefeated(1);
            SaveManager.SaveBosses(spawnManager);
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            for (int i = 0; i < 25; i++)
            {
                Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), Quaternion.identity, transform.parent);
            }
            doorManager.OpenDoors();    
            healthBar.SetVisible(false);
            if (settings != null) settings.IncrementStats(enemies: 1);
            isDying = true;
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

    void OnDestroy()
    {
        Destroy(aimTarget.gameObject);
    }
}