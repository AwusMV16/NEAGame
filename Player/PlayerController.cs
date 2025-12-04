using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    // ---------- TRANSFORMS & OBJECTS ----------
    [Header("Player References")]
    [SerializeField] private Transform rightOrigin;
    [SerializeField] private Transform leftOrigin;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform originalSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject AOEBlastPrefab;
    [SerializeField] private GameObject HealPrefab;
    [SerializeField] private GameObject XpPrefab;

    // ---------- CAMERA & CINEMACHINE ----------
    [Header("Camera Settings")]
    private CinemachineCamera cinemachineCam;
    private CinemachineBasicMultiChannelPerlin camNoise;
    private bool CamFollow = false;

    // ---------- PLAYER STATS ----------
    [Header("Stats")]
    [SerializeField] private int turretDamage = 20;
    public int MaxHealth;
    public int Health = 100;
    public int MaxEnergy;
    public int Energy;
    public int Level = 1;
    public int Xp;
    public int BaseXP = 50;
    public float XPLevelMultiplier = 1.2f;
    [SerializeField] private float invincibilityDuration = 1.0f;
    private int baseMaxHealth;
    private int baseTurretDamage;
    private int baseMaxEnergy;
    private float baseRunSpeed;

    // ---------- MOVEMENT ----------
    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float jumpPower;
    [SerializeField] private float climbCheckDistance = 2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float horizontal;
    private float lastHorizontal;
    private bool isUpright;
    private bool isUpsideDown;
    private bool isRightSide;
    private bool isLeftSide;
    private bool isGrounded;
    public bool climbEnabled = true;
    private bool canClimb = false;
    private bool wasTouchingSurface = false;

    // ---------- COMBAT ----------
    [Header("Combat Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootInterval;
    [SerializeField] private float spreadAngle;
    private bool isShooting;
    private bool canShoot;
    private float timeSinceLastShot;
    [SerializeField] private float OverHeatRecoverRate = 0.1f;
    [SerializeField] private float MaxOverHeat = 5f;
    private float OverHeatValue;
    private float originalRecoveryRate;
    private OverheatBar OverheatMeter;

    // ---------- HEALING ----------
    [Header("Healing Settings")]
    private bool isHealing;

    // ---------- FX & AUDIO ----------
    [Header("Audio Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource ClimbToggleSource;
    [SerializeField] private AudioSource ShootSource;
    [SerializeField] private float footstepInterval = 0.35f;
    private float footstepTimer = 0f;

    // ---------- UI REFERENCES ----------
    [Header("UI References")]
    [SerializeField] private Canvas DeathScreen;
    private HealthBar healthBar;
    private EnergyBar energyBar;
    private NextLevelRadial nextLevelRadial;
    private LevelText levelText;

    // ---------- LAYERS ----------
    [Header("Layers")]
    [SerializeField] private LayerMask climblayerMask;

    // ---------- INTERNAL STATE ----------
    private bool dead;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private float angle;
    private Vector3 directionToMouse;
    private bool hasRotated = false;
    private bool isInvincible = false;
    private Coroutine damageFlashRoutine;
    private Vector3 lastPositionBeforeDeath;
    private bool isInBossRoom;

    // ---------- RAYS ----------
    private RaycastHit2D rayDown;
    private RaycastHit2D rayUp;
    private RaycastHit2D rayLeft;
    private RaycastHit2D rayRight;
    private RaycastHit2D[] rays;
    private bool[] hits;

    // ---------- COMPONENTS ----------
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Settings settings;
    private SpawnManager spawnManager;

    // ---------- SAVE SYSTEM ----------
    private float saveTimer = 0f;
    private float saveInterval = 5f;
    private bool loaded;

    void Awake()
    {
        Health = MaxHealth;
        baseMaxHealth = MaxHealth;
        baseMaxEnergy = MaxEnergy;
        baseRunSpeed = runSpeed;
        baseTurretDamage = turretDamage;

        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        settings = FindAnyObjectByType<Settings>();
        healthBar = FindAnyObjectByType<HealthBar>();
        nextLevelRadial = FindAnyObjectByType<NextLevelRadial>();
        energyBar = FindAnyObjectByType<EnergyBar>();
        levelText = FindAnyObjectByType<LevelText>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        OverheatMeter = FindAnyObjectByType<OverheatBar>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
        cinemachineCam = FindAnyObjectByType<CinemachineCamera>();
        if(cinemachineCam != null) camNoise = cinemachineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Start()
    {
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        UpdateEnergy();
        UpdateHealth();

        originalRecoveryRate = OverHeatRecoverRate;
    }

    void Update()
    {
        saveTimer += Time.deltaTime;

        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            if(GameSession.playerSaveLoaded)
            {
                SaveManager.SavePlayer();
            }
        }

        if (GameSession.playerSaveLoaded && !loaded)
        {
            SaveManager.LoadPlayer();
            SaveManager.LoadBosses(spawnManager);
            SaveManager.LoadSpawnAnchor();
            loaded = true;
        }

        if (Time.timeScale == 0f) return;
        if (!dead)
        {
            horizontal = isHealing ? Input.GetAxisRaw("Horizontal") * 0.25f : Input.GetAxisRaw("Horizontal");
            lastHorizontal = Mathf.Abs(horizontal) == 1 ? horizontal : lastHorizontal;

            // Check if the player is grounded by casting a horizontal capsule at groundCheck position,
            // and ensure the player is upright to allow jumping only when standing on the floor
            isGrounded = Physics2D.OverlapCapsule(
                groundCheck.position,                  // Center of the capsule
                new Vector2(2f, 0.333f),               // Size of the capsule (width, height)
                CapsuleDirection2D.Horizontal,         // Horizontal capsule for wider ground detection
                0,                                     // No rotation
                climblayerMask                         // Layer to check collisions against
            ) && isUpright;                            // Only count as grounded if the player is upright. **fixes wall hop bug**


            // footstep sounds
            if (rayDown.collider != null && Mathf.Abs(horizontal) > 0.1f)
            {
                footstepTimer -= Time.deltaTime;

                if (footstepTimer <= 0f)
                {
                    if (!footstepSource.isPlaying)
                    {
                        footstepSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                        footstepSource.Play();
                    }

                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                footstepTimer = 0f; // reset so the step plays instantly on next movement
            }

            if (GameSession.playerSaveLoaded)
            {
                // Animation Triggers                       
                animator.SetBool("moving", Math.Abs(horizontal) > 0);
                animator.SetBool("Normal", !climbEnabled);

                HandleJump();
                Drawrays();
                Move();
                HandleFlip();
                AimTurret();

                // Inputs
                // if (Input.GetMouseButtonDown(0)) Shoot(directionToMouse);
                if (Input.GetMouseButton(0) && !isShooting && canShoot){ timeSinceLastShot = 0f; StartCoroutine(Shoot()); } 
                if (Input.GetKeyDown(KeyCode.F) && !climbEnabled && !isGrounded && Energy >= MaxEnergy / 2) AOEAttack();
                if (Input.GetKeyDown(KeyCode.LeftShift) && canClimb) ToggleClimb();
                if (Input.GetKeyDown(KeyCode.LeftControl) && !isHealing && Energy == MaxEnergy) StartCoroutine(HealRoutine());

                if (!isShooting) timeSinceLastShot += Time.deltaTime;
                if (isShooting)
                {
                    if (!ShootSource.isPlaying) ShootSource.Play();
                }
                else
                {
                    ShootSource.Stop();
                }


                EnableCameraShake(isShooting);
            }
            
            float reducedRecoveryRate = originalRecoveryRate * 0.25f;
            if (!isShooting && OverHeatValue > 0)
            {
                muzzle.GetChild(0).gameObject.SetActive(true);
                // grace delay before cooling
                if (timeSinceLastShot > 0.25f)
                    OverHeatValue -= OverHeatRecoverRate * 2f * Time.deltaTime;
                    OverHeatValue = Mathf.Max(OverHeatValue, 0);
                OverheatMeter.UpdateOverHeat(OverHeatValue, MaxOverHeat);
            }
            else
            {
                muzzle.GetChild(0).gameObject.SetActive(false);
            }
            if (OverHeatValue >= MaxOverHeat)
            {
                // Overheat -> block shooting
                canShoot = false;
                OverHeatRecoverRate = reducedRecoveryRate;
            }
            else if (OverHeatValue == 0)
            {
                // Fully cooled -> allow shooting
                canShoot = true;
                OverHeatRecoverRate = originalRecoveryRate;
            }

            // Check for death
            if (Health <= 0)
            {
                if(DeathScreen != null) DeathScreen.GetComponentInChildren<Animator>().SetTrigger("Activate");
                animator.SetBool("Dead", true);
                dead = true;
                climbEnabled = false;

                // Start the post-death delay
                StartCoroutine(PostDeathDelay());
                UpdateHealth();
            }
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                        Camera.main.WorldToScreenPoint(transform.position).z)
        );
        Vector3 lookAheadOffset = (mouseWorldPos - transform.position) * 0.025f; // 0.1 = small lean
        lookAheadOffset.z = 0; // keep camera z correct

        if (CamFollow)
        {
            Vector3 targetPos;
            if (isInBossRoom)
            {
                targetPos = transform.position + new Vector3(0, 5f, -5f) + lookAheadOffset;
            }
            else
            {
                targetPos = transform.position + lookAheadOffset;
            }
            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, targetPos, Time.deltaTime * 5f); // smooth follow
        }
        else
        {
            GameObject nearestAnchor = GetNearestWithTag("AnchorPoint", transform.position);
            if (nearestAnchor != null)
            {
                // Apply the same offset toward mouse
                Vector3 targetPos = nearestAnchor.transform.position + lookAheadOffset;
                targetPos.z = 0; // ensure camera Z stays correct      
                MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, targetPos, Time.deltaTime * 7.5f);
            }
        }
    }

    void EnableCameraShake(bool enable)
    {
        if (camNoise != null)
        {
            camNoise.AmplitudeGain = enable ? 0.1f : 0.0f; // intensity of shake
            camNoise.FrequencyGain = enable ? 5.0f : 0.0f; // frequency of shake
        }
    }
    

    private IEnumerator HealRoutine()
    {
        isHealing = true;

        // Instantiate the healing aura effect
        GameObject effect = Instantiate(
            HealPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        float holdTime = 0f;
        float requiredHold = 0.45f;

        // Keep waiting while key is held and time < requiredHold
        while (Input.GetKey(KeyCode.LeftControl))
        {
            holdTime += Time.deltaTime;

            // If player held key long enough → heal
            if (holdTime >= requiredHold)
            {
                Health += Mathf.FloorToInt(MaxHealth * 0.25f);
                Health = Mathf.Min(Health, MaxHealth);
                UpdateHealth();

                Energy = 0;
                UpdateEnergy();

                break; // done healing
            }

            yield return null; // wait one frame
        }

        // If released early (didn’t reach required Hold), cancel healing
        if (holdTime < requiredHold)
        {
            // destroy effect
            Destroy(effect);
        }

        isHealing = false;
    }

        private void AOEAttack()
    {
        Energy -= MaxEnergy / 2;
        UpdateEnergy();
        StartCoroutine(AOECoroutine());
    }

    // Coroutine handles the timer
    IEnumerator PostDeathDelay()
    {
        lastPositionBeforeDeath = transform.position;
        float delay = 3f; // seconds
        yield return new WaitForSeconds(delay);

        body.constraints = RigidbodyConstraints2D.None;
        Respawn();
    }

    void ResetStats()
    {
        turretDamage = baseTurretDamage;
        runSpeed = baseRunSpeed;
        MaxHealth = baseMaxHealth;
        Energy = 0;
        UpdateEnergy();
    }

    public void Respawn()
    {
        UpdateStatsFromLevel();
        // ResetStats();
        if (spawnManager.ActiveAnchor != null)
        {
            transform.position = spawnManager.ActiveAnchor.transform.position;
        }
        else
        {
            transform.position = originalSpawnPoint.position;
        }

        DropXp();
        Xp = 0;
        UpdateRemainingXP();
        Level = Math.Max(Level - 1, 1);
        UpdateLevelText(Level);


        Health = MaxHealth;
        Energy = 0;
        UpdateEnergy();


        dead = false;
        animator.SetBool("Dead", false);
        climbEnabled = false;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        UpdateHealth();
        spawnManager.RespawnEnemies();
        
        // Respawn the boss if already encountered
        spawnManager.ResetBoss();

        // SaveManager.SavePlayer();
    }

    void DropXp()
    {
        for (int i = 0; i < Mathf.FloorToInt(Xp * 0.5f); i++)
        {
            Instantiate(XpPrefab, lastPositionBeforeDeath + new Vector3(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), 0), quaternion.identity);
        }
    }

    private IEnumerator AOECoroutine()
    {
        animator.SetBool("AOE", true); //play the player's attack animation

        float elapsed = 0f;
        while (elapsed < 0.5f) // keep slow motion for full duration
        {
            // Reduce player velocity each frame
            body.linearVelocity *= 0.2f;

            elapsed += Time.fixedDeltaTime; // or Time.deltaTime if using Update
            yield return new WaitForFixedUpdate(); // wait for next physics frame
        }


        GameObject blast = Instantiate(AOEBlastPrefab, transform.position, quaternion.identity);
        AOEBlast blastScript = blast.GetComponent<AOEBlast>();
        blastScript.damage = turretDamage * 5;
        animator.SetBool("AOE", false);
    }   


    private void Move()
    {
        isUpright = Mathf.Abs(transform.rotation.eulerAngles.z) == 0;     // Check if player is upright (facing upward)
        isUpsideDown = Mathf.Abs(transform.rotation.eulerAngles.z) == 180;   // Check if player is upside down (on ceiling)
        isRightSide = Mathf.Abs(transform.rotation.eulerAngles.z) == 270;   // Check if player is on the right wall
        isLeftSide = Mathf.Abs(transform.rotation.eulerAngles.z) == 90;    // Check if player is on the left wall

        // Cast ray downward relative to current up direction to detect ground or climbable surface
        // Cast ray to the left and right to detect walls when climbing
        // Cast ray upward to detect climbable ceilings or transitions
        rayDown = Physics2D.Raycast(transform.position, -transform.up, climbCheckDistance * 1.55f, climblayerMask);
        rayLeft = Physics2D.Raycast(transform.position, -transform.right, climbCheckDistance * 0.6f, climblayerMask);
        rayRight = Physics2D.Raycast(transform.position, transform.right, climbCheckDistance * 0.6f, climblayerMask);
        rayUp = Physics2D.Raycast(transform.position, transform.up, climbCheckDistance, climblayerMask);

        Vector2 capsuleSize = new Vector2(0.2f, 0.2f); // small capsule "tip"

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

        if (climbEnabled)
        {
            // Handle corner transitions when no walls are being touched
            if (!touchingNow && wasTouchingSurface) // Use guard flag to prevent multiple rotations during one corner move
            {
                if (horizontal > 0  && hits[1] == false && hits[0] == true)
                {
                    RotateAroundCorner(rightOrigin.position, -90); // Turn right corner counterclockwise
                    // hasRotated = true; // Set guard to prevent repeated rotation
                }
                else if (horizontal < 0 && hits[2] == false && hits[0] == true)
                {
                    RotateAroundCorner(leftOrigin.position, 90); // Turn left corner clockwise
                    // hasRotated = true;
                }
                else
                {
                    // Fallback to last known direction if no current input
                    if (lastHorizontal > 0)
                    {
                        RotateAroundCorner(rightOrigin.position, -90);
                        // hasRotated = true;
                    }
                    else if (lastHorizontal < 0)
                    {
                        RotateAroundCorner(leftOrigin.position, 90);
                        // hasRotated = true;
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
                    // hasRotated = true;
                }
                else
                {
                    if (rays[0] && rays[2]) // bottom + left wall contact
                    {
                        RotateAroundCenter(-90); // Rotate counterclockwise around player's center
                    }
                    // hasRotated = true;
                }
            }

            hasRotated = !(CollCount > 0); // Reset rotation flag if not in contact with walls anymore

            if (CollCount > 0)
            {
                // Apply force away from the wall to help the player "stick" to surface (for stability)
                Vector2 wallNormal = hits[0] ? rays[0].normal : new Vector2(); // Use downward normal if available
                body.AddForce(-wallNormal * 1000, ForceMode2D.Force); // Push into wall slightly
            }
        }

        // Update ability to climb based on whether any surface is currently being touched
        canClimb = CollCount > 0;

        wasTouchingSurface = touchingNow;
    }

    private int XPRequiredForNextLevel()
    {
        return Mathf.FloorToInt(BaseXP * Mathf.Pow(XPLevelMultiplier, Level));
    }

    public void IncreaseXP()
    {
        Xp++;
        UpdateRemainingXP();

        while (Xp >= XPRequiredForNextLevel())
        {
            Xp -= XPRequiredForNextLevel();
            LevelUp();
        }
    }
    private void LevelUp()
    {
        Level++;
        levelText.updateLevelText(Level);

        UpdateStatsFromLevel();

        // Increase stats by 20%
        if (Health == MaxHealth)
        {
            // MaxHealth = Mathf.FloorToInt(MaxHealth * 1.2f);
            Health = MaxHealth;
        }
        else
        {
            Health = Mathf.Min(Health, MaxHealth);
            // MaxHealth = Mathf.FloorToInt(MaxHealth * 1.2f);
        }
        UpdateHealth();  
    }

    public void UpdateStatsFromLevel()
    {
        MaxHealth      = Mathf.FloorToInt(baseMaxHealth * Mathf.Pow(1.3f, Level - 1));
        turretDamage   = Mathf.FloorToInt(baseTurretDamage * Mathf.Pow(1.2f, Level - 1));
        runSpeed       = baseRunSpeed * Mathf.Pow(1.03f, Level - 1);
        MaxEnergy      = Mathf.FloorToInt(baseMaxEnergy * Mathf.Pow(0.98f, Level - 1));
    }
    
    public void FlashRed()
    {
        if (damageFlashRoutine != null)
            StopCoroutine(damageFlashRoutine);

        damageFlashRoutine = StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void AimTurret()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                        Camera.main.WorldToScreenPoint(gun.transform.position).z)
        );
        mouseWorldPos.z = gun.transform.position.z;
        directionToMouse = (mouseWorldPos - gun.transform.position).normalized;
        angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
        
        // Set the gun's rotation to face the mouse, considering the turret's orientation
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 
    }

    private IEnumerator Shoot()
    {

        isShooting = true;

        OverHeatValue += 1f / 5;
        OverHeatValue = Mathf.Min(OverHeatValue, MaxOverHeat);
        OverheatMeter.UpdateOverHeat(OverHeatValue, MaxOverHeat);

        // Create a muzzle flash effect at the gun when shooting
        GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, muzzle.position, gun.transform.rotation);
        muzzleFlash.transform.SetParent(muzzle, true);
        Destroy(muzzleFlash, 0.1f);

        // Apply random angular spread to the bullet's rotation
        Quaternion spreadRotation = Quaternion.Euler(0, 0, 180f + UnityEngine.Random.Range(-spreadAngle, spreadAngle));

        // Spawn a bullet at the fire point, rotated correctly
        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, gun.transform.rotation * spreadRotation);

        // Set up bullet properties like damage, target, and color
        Bullet bulletScript = projectile.GetComponent<Bullet>();
        bulletScript.damage = turretDamage;
        bulletScript.ignoreTag = tag;
        bulletScript.attackTag = "Enemy";
        bulletScript.bulletColor = new Color(1, 0.1f, 0.1f);
        bulletScript.bulletTag = "PlayerBullet";

        // Apply velocity to the bullet so it moves in the shoot direction
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = -projectile.transform.right * bulletSpeed;

        // Destroy the bullet after 2 seconds to prevent clutter
        Destroy(projectile, 2f);

        //wait n second(s)
        yield return new WaitForSeconds(shootInterval);

        isShooting = false;
    }
    
    private void FixedUpdate()
    {
        if (climbEnabled)
        {
            body.gravityScale = 0; // Disable gravity while climbing

            if (isUpright)
            {
                body.linearVelocity = new Vector2(horizontal * runSpeed, 0); // Move horizontally along the floor
            }
            else if (isUpsideDown)
            {
                body.linearVelocity = new Vector2(-horizontal * runSpeed, 0); // Move horizontally along the ceiling (inverted)
            }
            else if (isRightSide)
            {
                body.linearVelocity = new Vector2(0, -horizontal * runSpeed); // Climb vertically along the right wall
            }
            else if (isLeftSide)
            {
                body.linearVelocity = new Vector2(0, horizontal * runSpeed); // Climb vertically along the left wall
            }
            else
            {
                body.linearVelocity = Vector2.zero; // No valid orientation, stay still
            }
        }
        else
        {
            // Restore default behavior when not climbing
            transform.rotation = Quaternion.Euler(0, 0, 0); // Reset rotation
            body.linearVelocity = new Vector2(horizontal * runSpeed, body.linearVelocity.y); // Move left/right with gravity
            body.gravityScale = 5; // Re-enable gravity
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            animator.SetBool("landing", false);
            animator.SetBool("falling", false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            animator.SetBool("falling", body.linearVelocityY > 0);
            animator.SetBool("landing", body.linearVelocityY < 0);
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocityX, jumpPower);

            jumpBufferCounter = 0;
        }
        if (Input.GetButtonUp("Jump") && body.linearVelocityY > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocityX, body.linearVelocityY * 0.75f);

            coyoteTimeCounter = 0;
        }
    }

    public void UpdateEnergy()
    {
        if (energyBar != null)
            energyBar.UpdateEnergy(Energy, MaxEnergy);
    }

    public void UpdateHealth()
    {
        // SaveManager.SavePlayer();
        if (healthBar != null)
            healthBar.UpdateHealth(Health, MaxHealth);
    }

    public void UpdateLevelText(int lvl)
    {
        levelText.updateLevelText(lvl);
    }

    private void UpdateRemainingXP()
    {
        nextLevelRadial.UpdateRemainingXP(Xp, XPRequiredForNextLevel());
    }


    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        FlashRed();
        Health -= damage;
        UpdateHealth();

        if (settings != null)
            settings.IncrementStats(damageTaken: damage);

        StartCoroutine(InvincibilityFrames());
        lastPositionBeforeDeath = transform.position;
    }

    public void TakeDamagePercent(float percent)
    {
        if (isInvincible)
            return;
            
        FlashRed();
        Health -= Mathf.FloorToInt(Health * percent); 
 
        UpdateHealth();

        if (settings != null)
            settings.IncrementStats(damageTaken: Mathf.FloorToInt(MaxHealth / percent));

        StartCoroutine(InvincibilityFrames());
        lastPositionBeforeDeath = transform.position;
    }

    private IEnumerator InvincibilityFrames()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        isInvincible = true;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isInvincible = false;
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

    private void Drawrays()
    {
        Debug.DrawRay(transform.position, -transform.up * climbCheckDistance * 2f, new Color(0, 0.3f, 0));
        Debug.DrawRay(transform.position, transform.up * climbCheckDistance * 1.5f, new Color(0, 0.2f, 0.3f));
        Debug.DrawRay(transform.position, -transform.right * climbCheckDistance * 0.55f, new Color(0.3f, 0, 0));
        Debug.DrawRay(transform.position, transform.right * climbCheckDistance * 0.55f, new Color(1, 0.2f, 0.2f));
    }

    void HandleFlip()
    {
        spriteRenderer.flipX = horizontal < 0;
    }

    void ToggleClimb()
    {
        ClimbToggleSource.Play();
        climbEnabled = !climbEnabled;

        if (climbEnabled)
        {
            if (rays[1])
            { //right 
                RotateAroundCenter(90);
                hasRotated = true;
            }
            else if (rays[2])
            { //left 
                RotateAroundCenter(-90);
                hasRotated = true;
            }
            else if (rays[3])
            { //up
                RotateAroundCenter(180);
                hasRotated = true;
            }
        }
    }

    GameObject GetNearestWithTag(string tag, Vector3 position)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            float dist = Vector3.Distance(position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = obj;
            }
        }

        return nearest;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("MainArea"))
        {
            CamFollow = true;
            isInBossRoom = false;
        }
        else if (col.CompareTag("BossArea"))
        {
            CamFollow = true;
            isInBossRoom = true;
        }
        
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("MainArea"))
        {
            CamFollow = false;
        }
        else if (col.CompareTag("BossArea"))
        {
            CamFollow = false;
            isInBossRoom = false;
        }
    }
    void DrawDebugCapsuleCast(Vector3 origin, Vector2 size, Vector3 direction, float distance, Color color)
    {
        // End position of the cast
        Vector3 endPoint = origin + direction.normalized * distance;

        float radius = size.x * 0.5f;

        // Draw start circle
        DrawCircle(origin, radius, color);

        // Draw end circle
        DrawCircle(endPoint, radius, color);

        // Draw capsule sides
        Vector3 sideOffset = Vector3.Cross(direction.normalized, Vector3.forward) * radius;
        Debug.DrawLine(origin + sideOffset, endPoint + sideOffset, color);
        Debug.DrawLine(origin - sideOffset, endPoint - sideOffset, color);
    }

    void DrawCircle(Vector3 center, float radius, Color color, int segments = 16)
    {
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = Mathf.Deg2Rad * (i * angleStep);
            float angle2 = Mathf.Deg2Rad * ((i + 1) * angleStep);

            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(p1, p2, color);
        }
    }
}



