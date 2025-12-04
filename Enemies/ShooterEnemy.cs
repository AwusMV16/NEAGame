using Unity.Mathematics;
using UnityEngine;

public class ShooterEnemy : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float followRadius;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed;
    private float angle;
    private float dirX; // move direction in X axis
    private bool isEngaged; 
    private GameObject player;
    private float distanceToPlayer;
    public float bulletInterval = 1f; // Every 1s
    private float bulletTimer = 0f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private Transform firePoint;
    private Animator animator;
    public int damage = 20;
    private int currentHealth;
    [SerializeField] private int maxHealth = 100;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    private Settings settings;
    private float lastAngle = float.MaxValue;
    private RaycastHit2D ray;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dirX = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        animator = GetComponent<Animator>();
        settings = FindAnyObjectByType<Settings>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        // Only run if the object is engaged (e.g., player or enemy active)
        if (isEngaged)
        {
            // Increment timer by the time elapsed since last frame
            bulletTimer += Time.deltaTime;

            // Check if it's time to shoot
            if (bulletTimer >= bulletInterval)
            {
                bulletTimer = 0f; // Reset timer
                Shoot();          // Fire a bullet
            }
        }
    }

    void FixedUpdate()
    {
        if (!isEngaged) // back and forth movement when player is not detected
        {
            angle = dirX > 0 ? 0 : -180; //orient the player based on direction of movement

            if (rb.linearVelocity.x == 0)
                dirX = -dirX; // reverse the horizontal movement direction when the player is stuck (not moving)

            // set the velocity based on these directions * move speed
            rb.linearVelocity = new Vector2(dirX * moveSpeed, 0f); 
            // Debug.Log(rb.linearVelocityY);
        }
        else if (isEngaged && player != null) // orbit the player when they are within detection radius
        {
            // distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float sqrDistanceToPlayer = (transform.position - player.transform.position).sqrMagnitude;
            if (sqrDistanceToPlayer <= followRadius * followRadius)
            {
                // Calculate direction from enemy to player
                Vector3 direction = (player.transform.position - transform.position).normalized;
                // Calculate perpendicular direction for orbiting
                Vector3 orbitDirection = Vector3.Cross(direction, Vector3.forward).normalized;
                // Move along the orbit direction
                Vector2 orbitVelocity = new Vector2(orbitDirection.x, orbitDirection.y) * orbitSpeed;
                rb.linearVelocity = orbitVelocity;

                // Face the player
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
            else // if the player is detected but not within orbit radius, move towards the player
            {
                // Move towards player if outside followRadius
                Vector2 moveDir = (player.transform.position - transform.position).normalized;
                rb.linearVelocity = moveDir * moveSpeed;

                // Face the player
                angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            }
        }
        else // if none of the previous conditions are met...
        {
            
            isEngaged = false; // stop pursuing the player
            // set the velocity back to back and forth movement
            rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocityY * moveSpeed); 
        }
        // rotate the enemy to face the player based on the angle
        if (Mathf.Abs(angle - lastAngle) > 1f) { // Only update if changed by >1 degree
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            lastAngle = angle;
        }
    }

    void Shoot()
    {
        // multiplying quaternions *combines* rotation
        Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, -90f); 
        // create an instance of the bullet at the firing position
        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, rotation); 
        Bullet bulletScript = projectile.GetComponent<Bullet>(); 
        // set player attributes like damage, tag to ignore and the tag to attack and the color of the bullet 
        bulletScript.damage = damage; 
        bulletScript.ignoreTag = tag;   
        bulletScript.attackTag = "Player";
        // set the custom color of the bullet
        bulletScript.bulletColor = new Color(0.6784314f, 0.3803922f, 0.9058824f); 
        Rigidbody2D bulletRb = projectile.GetComponent<Rigidbody2D>();
        bulletRb.linearVelocity = transform.up * 15f; // the bullet travels in the up direction of the enemy 
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(isEngaged){
            orbitSpeed = -orbitSpeed;
        }else{
            dirX = -dirX;
        }
        if (other.collider.CompareTag("Player"))
        {
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }

    // Reduces health when damaged and triggers hit animation
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        animator.SetTrigger("Hit");

        settings.IncrementStats(damageDealt: dmg); 

        // If health drops to zero or below, destroy.
        if (Health <= 0)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
            settings.IncrementStats(enemies: 1); 
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
            angle = dirX > 0 ? 0 : -180; // set enemy orientation based on movement direction
            player = null; // reset the player variable
            isEngaged = false; // stop pursuing the player
        }
    }
    
}
