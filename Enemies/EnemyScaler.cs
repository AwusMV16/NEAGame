using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    private Transform target;
    private float maxDistance = 1000f;

    private SpinnerEnemy spinner;
    private RollerEnemy roller;
    private ExplodeEnemy explode;
    private ShooterEnemy shooter;

    void Awake()
    {
        spinner = GetComponent<SpinnerEnemy>();
        roller = GetComponent<RollerEnemy>();
        explode = GetComponent<ExplodeEnemy>();
        shooter = GetComponent<ShooterEnemy>();

        SpawnManager spawnManager = FindAnyObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            target = spawnManager.originalSpawnPoint;
        }
        else
        {
            target = transform;
        }

        float maxScale = 3f;
        float sqrDist = (target.position - transform.position).sqrMagnitude;
        float sqrMaxDistance = maxDistance * maxDistance;

        // normalizedDistance is 0..1 based on squared distance
        float normalizedDistance = Mathf.Clamp01(sqrDist / sqrMaxDistance);

        // linear scaling by taking square root
        float linearNormalized = Mathf.Sqrt(normalizedDistance);

        // scale factor: 1 at 0 distance, maxScale at maxDistance
        float scale = 1f + linearNormalized * (maxScale - 1f);

        // Only scale the enemy that exists on this GameObject 
        if (spinner != null)
        {
            spinner.damage = Mathf.FloorToInt(spinner.damage * scale);
            spinner.InitializeHealth(Mathf.FloorToInt(spinner.maxHealth * scale));
        }
        else if (roller != null)
        {
            roller.damage = Mathf.FloorToInt(roller.damage * scale);
            roller.InitializeHealth(Mathf.FloorToInt(roller.maxHealth * scale));
        }
        else if (explode != null)
        {
            explode.damage = Mathf.FloorToInt(explode.damage * scale);
            explode.InitializeHealth(Mathf.FloorToInt(explode.maxHealth * scale));
        }
        else if (shooter != null)
        {
            shooter.damage = Mathf.FloorToInt(shooter.damage * scale);
            shooter.InitializeHealth(Mathf.FloorToInt(shooter.maxHealth * scale));
        }
    }
}
