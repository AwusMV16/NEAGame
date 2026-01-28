using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class LaserEnemy : MonoBehaviour, IDamageable, IEnemy
{
    // ---------- MOVEMENT ----------
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float climbCheckDistance = 2f;
    private float horizontal;
    private float lastHorizontal;
    private bool isUpright;
    private bool isUpsideDown;
    private bool isRightSide;
    private bool isLeftSide;
    private bool wasTouchingSurface = false;
    private bool canClimb = false;
    private float initialHorizontal;

    // ---------- REFERENCES ----------
    [SerializeField] private Transform rightOrigin;
    [SerializeField] private Transform leftOrigin;
    [SerializeField] private GameObject XPOrbPrefab;
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject radiusVisual;

    // ---------- RAYS ----------
    private RaycastHit2D rayDown;
    private RaycastHit2D rayUp;
    private RaycastHit2D rayLeft;
    private RaycastHit2D rayRight;
    private RaycastHit2D[] rays;
    private bool[] hits;

    // ---------- LAYERS ----------
    [Header("Layers")]
    [SerializeField] private LayerMask climblayerMask;

    // ---------- COROUTINES ----------
    private Coroutine floatingRoutine;

    // ---------- OTHER ----------
    private Rigidbody2D body;
    private bool isEngaged;
    private LineRenderer line;
    private Animator anim;
    [SerializeField] private float maxDistance = 20f;
    private SpriteRenderer radiusRenderer;
    private Light2D radiusLight;
    private int currentHealth;
    public int damage = 10;
    public int maxHealth = 100;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0;
        initialHorizontal = Random.value < 0.5f ? 1f : -1f;
        line = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
        radiusRenderer = radiusVisual.GetComponent<SpriteRenderer>();
        radiusLight = radiusVisual.GetComponent<Light2D>();
    }

    void Start()
    {
        horizontal = initialHorizontal;
        lastHorizontal = Mathf.Abs(horizontal) == 1 ? horizontal : lastHorizontal;
        line.SetPositions(new Vector3[] { transform.position, transform.position });
        line.enabled = false;

        currentHealth = maxHealth;
    }

    void Update()
    {
        float sqrDist = (PlayerService.position - transform.position).sqrMagnitude;
        float maxSqrDist = maxDistance * maxDistance;

        // 0 = close, 1 = far
        float t = Mathf.Clamp01(sqrDist / maxSqrDist);

        // invert: 1 = close, 0 = far
        float fade = 1f - t;

        radiusRenderer.color = new Color(radiusRenderer.color.r, radiusRenderer.color.g, radiusRenderer.color.b, fade * 0.1f);
        radiusLight.intensity = fade * 6;

        if (isEngaged)
        {
            anim.SetBool("IsEngaged", true);
            line.SetPosition(0, transform.position);
            line.SetPosition(1, PlayerService.position);
            line.enabled = true;
            PlayerService.transform.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
        else
        {
            anim.SetBool("IsEngaged", false);
            line.enabled = false;
        }
    }

    void FixedUpdate()
    {
        horizontal = !isEngaged ? initialHorizontal : 0;

        Move();
        if (isUpright)
        {
            body.linearVelocity = new Vector2(horizontal * moveSpeed, 0); // Move horizontally along the floor
        }
        else if (isUpsideDown)
        {
            body.linearVelocity = new Vector2(-horizontal * moveSpeed, 0); // Move horizontally along the ceiling (inverted)
        }
        else if (isRightSide)
        {
            body.linearVelocity = new Vector2(0, -horizontal * moveSpeed); // Climb vertically along the right wall
        }
        else if (isLeftSide)
        {
            body.linearVelocity = new Vector2(0, horizontal * moveSpeed); // Climb vertically along the left wall
        }
        else
        {
            body.linearVelocity = Vector2.zero; // No valid orientation, stay still
        }
    }

    private void Move()
    {
        Vector2 up = transform.up;

        isUpright     = Vector2.Dot(up, Vector2.up)    > 0.9f;
        isUpsideDown  = Vector2.Dot(up, Vector2.down)  > 0.9f;
        isRightSide   = Vector2.Dot(up, Vector2.right) > 0.9f;
        isLeftSide    = Vector2.Dot(up, Vector2.left)  > 0.9f;

        float speedStep = moveSpeed * Time.fixedDeltaTime;
        float rayLength = climbCheckDistance + speedStep;

        // Cast ray downward relative to current up direction to detect ground or climbable surface
        // Cast ray to the left and right to detect walls when climbing
        // Cast ray upward to detect climbable ceilings or transitions
        rayDown = Physics2D.Raycast(transform.position, -transform.up, rayLength * 1.55f, climblayerMask);
        rayLeft = Physics2D.Raycast(transform.position, -transform.right, rayLength * 0.6f, climblayerMask);
        rayRight = Physics2D.Raycast(transform.position, transform.right, rayLength * 0.6f, climblayerMask);
        rayUp = Physics2D.Raycast(transform.position, transform.up, rayLength, climblayerMask);

        // Store all rays in an array for easier iteration and collision checking
        rays = new RaycastHit2D[4];
        rays[0] = rayDown;   // Downward ray
        rays[1] = rayRight;  // Rightward ray
        rays[2] = rayLeft;   // Leftward ray
        rays[3] = rayUp;     // Upward ray

        hits = new bool[rays.Length]; // Create a boolean array to track which rays hit something

        int CollCount = 0; // Track how many rays successfully collided with a surface
        for (int i = 0; i < rays.Length; i++)
        {
            hits[i] = rays[i].collider != null; // Store whether each ray hit something

            if (hits[i])
            {
                CollCount += 1; // Increment counter if a collision was detected
            }
        }

        bool touchingNow = CollCount > 0;

        // if (!touchingNow && floatingRoutine == null) floatingRoutine = StartCoroutine(CheckFloating());
        if (rayDown.collider == null && floatingRoutine == null) floatingRoutine = StartCoroutine(CheckFloating());

        // Handle corner transitions when no walls are being touched
        if (!touchingNow && wasTouchingSurface) // Use guard flag to prevent multiple rotations during one corner move
        {
            if (horizontal > 0  && hits[1] == false && hits[0] == true)
            {
                RotateAroundCorner(rightOrigin.position, -90); // Turn right corner counterclockwise
            }
            else if (horizontal < 0 && hits[2] == false && hits[0] == true)
            {
                RotateAroundCorner(leftOrigin.position, 90); // Turn left corner clockwise
            }
            else
            {
                // Fallback to last known direction if no current input
                if (lastHorizontal > 0)
                {
                    RotateAroundCorner(rightOrigin.position, -90);
                }
                else if (lastHorizontal < 0)
                {
                    RotateAroundCorner(leftOrigin.position, 90);
                }
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------

        // Handle rotation when player is touching two surfaces (e.g. floor + wall)
        else if (CollCount == 2)
        {
            if (horizontal > 0)
            {
                if (rays[0] && rays[1]) // bottom + right wall contact
                {
                    RotateAroundCenter(90); // Rotate clockwise around player's center
                }
            }
            else
            {
                if (rays[0] && rays[2]) // bottom + left wall contact
                {
                    RotateAroundCenter(-90); // Rotate counterclockwise around player's center
                }
            }
        }

        if (CollCount > 0)
        {
            // Apply force away from the wall to help the player "stick" to surface (for stability)
            Vector2 wallNormal = hits[0] ? rays[0].normal : new Vector2(); // Use downward normal if available
            body.AddForce(-wallNormal * 5000, ForceMode2D.Force); // Push into wall slightly
        }

        canClimb = CollCount > 0;

        wasTouchingSurface = touchingNow;
    }

    // Rotates the player around a corner pivot point by a given angle (used for turning corners while climbing)
    void RotateAroundCorner(Vector3 originPosition, float angle)
    {
        transform.position = originPosition;            // Snap player to the corner pivot point
        transform.Rotate(Vector3.forward, angle);       // Rotate the player around the Z-axis by the specified angle
    }

    // Rotates the player around its current center by a given angle 
    // (used when climbing from one surface to another)
    void RotateAroundCenter(float angle)
    {
        transform.Rotate(Vector3.forward, angle);       // Rotate player in place around Z-axis
    }

    private IEnumerator CheckFloating()
    {
        yield return new WaitForSeconds(0.1f);
        if (!canClimb)
        {
            transform.rotation.Set(0, 0, 0, 0);
            body.gravityScale = 100;
        }
        else
        {
            body.gravityScale = 0;
        }
        floatingRoutine = null;
    }

    // Reduces health when damaged and triggers hit animation
    public void TakeDamage(int dmg)
    {
        Settings settings = FindAnyObjectByType<Settings>();
        currentHealth -= dmg;
        anim.SetTrigger("Hit");

        if(settings != null) settings.IncrementStats(damageDealt: dmg); 
        // If health drops to zero or below, destroy.
        if (currentHealth <= 0)
        {
            GameObject Particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Particle, 2f);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), Quaternion.identity);
            Instantiate(XPOrbPrefab, transform.position + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), Quaternion.identity);
            if(settings != null) settings.IncrementStats(enemies: 1); 
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col) // player triggers detection circle
    {
        if (col.CompareTag("Player")) // check for "Player" tag
        {
            isEngaged = true; // pursue the player
        }
    }

    void OnTriggerExit2D(Collider2D col) // player leaves detection circle
    {
        if (col.CompareTag("Player")) // check for "Player" tag
        {
            isEngaged = false; // stop pursuing the player
        }
    }
}
