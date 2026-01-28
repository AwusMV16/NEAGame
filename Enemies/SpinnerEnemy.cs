using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpinnerEnemy : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;
    public GameObject spinnerPrefab;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private float cloneHealthLimit;
    [SerializeField] private Animator animator;

    public int maxHealth = 100;
    private int currentHealth;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    public int damage = 20;
    int healthBeforeDamage;
    private Rigidbody2D rb;
    private Vector2 moveVector;
    private Settings settings;

    void Start()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        TryGetComponent(out rb);
        settings = FindAnyObjectByType<Settings>();

        // Choose a random direction on both axes (-1 or 1) and apply moveSpeed
        int rand1 = UnityEngine.Random.Range(0, 2) * 2 - 1;
        int rand2 = UnityEngine.Random.Range(0, 2) * 2 - 1;
        moveVector = new Vector2(rand1 * moveSpeed, rand2 * moveSpeed);

        // Store health before any damage is taken
        currentHealth = maxHealth;
        healthBeforeDamage = currentHealth;
    }

    // This method is called at a fixed time interval, suitable for physics-related updates
    void FixedUpdate()
    {
        // Rotate the object around the Z-axis at a speed defined by 'rotationSpeed'
        // Negative value rotates it clockwise
        transform.Rotate(new Vector3(0, 0, -rotationSpeed));

        if (rb != null)
        {
            // Move continuously in the chosen direction
            rb.MovePosition(new Vector2(transform.position.x, transform.position.y) + moveVector);
        }
        

        // If health drops to zero or below, destroy and possibly replicate
        if (Health <= 0)
        {
            Destroy(gameObject);
            Replicate(2);  // Create 2 clones
        }
    }

    // Reduces health when damaged and triggers hit animation
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.ResetTrigger("Hit");  // clear queued triggers
        animator.SetTrigger("Hit");    // immediately trigger damaged animation

        settings.IncrementStats(damageDealt: damage); 

        // If health drops to zero or below, destroy and possibly replicate
        if (Health <= 0)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            settings.IncrementStats(enemies: 1); 
            Destroy(gameObject);
            Replicate(2);  // Create 2 clones
        }
    }

    public void InitializeHealth(int startingHealth)
    {
        currentHealth = startingHealth;
        maxHealth = startingHealth;
    }

    // Spawns smaller clones if health before damage was high enough
    void Replicate(int n)
    {
        SpawnManager spawnManager = FindAnyObjectByType<SpawnManager>();
        if (cloneHealthLimit < healthBeforeDamage)
        {
            for (int i = 0; i < n; i++)
            {
                // Slight random offset for clone positions
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0);

                // Instantiate a clone
                GameObject child = Instantiate(spinnerPrefab, transform.position + offset, transform.rotation);
                spawnManager?.RegisterRuntimeEnemy(child);

                // Set up the clone's properties
                SpinnerEnemy enemy = child.GetComponent<SpinnerEnemy>();

                EnableAllComponents(child);
                enemy.InitializeHealth(healthBeforeDamage / 2);  // Give clone half health
                enemy.cloneHealthLimit = cloneHealthLimit;       // Preserve clone limit
                child.transform.localScale = transform.localScale * 0.8f; // Shrink clone
                child.GetComponent<SpinnerEnemy>().spinnerPrefab = spinnerPrefab;
            }
        }
        else
        {
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
        }
    }

    // Handles collisions with walls and the player
    void OnCollisionEnter2D(Collision2D other)
    {
        // Bounce off walls using physics reflection
        moveVector = Vector2.Reflect(moveVector, other.contacts[0].normal);

        // Deal damage to player on collision
        if (other.collider.CompareTag("Player"))
        {
            IDamageable target = other.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }

    void EnableAllComponents(GameObject obj)
    {
        // Enable all MonoBehaviour scripts on this object
        foreach (var comp in obj.GetComponents<Behaviour>())
        {
            comp.enabled = true;
        }

        // Enable all Canvas components on this object and children
        foreach (var canvas in obj.GetComponentsInChildren<Canvas>(true))
        {
            canvas.enabled = true;
        }

        // Recursively enable children
        foreach (Transform child in obj.transform)
        {
            EnableAllComponents(child.gameObject);
        }
    }
}
