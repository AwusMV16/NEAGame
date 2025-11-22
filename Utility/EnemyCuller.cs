using System.Collections;
using UnityEngine;

public class EnemyCuller : MonoBehaviour
{
    public float cullDistance = 40f;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    private PolygonCollider2D polCol;
    private GameObject healthBar;
    private Animator anim;
    private MonoBehaviour[] enemyScripts;  // all behaviour on this enemy

    [SerializeField] private float cullCheckInterval = 0.05f;
    private bool isCulled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        polCol = GetComponent<PolygonCollider2D>();
        anim = GetComponent<Animator>();
        healthBar = transform.Find("EnemyHealthBar").gameObject;

        // Get ALL scripts attached to this enemy (ShooterEnemy, SpinnerEnemy, etc)
        enemyScripts = GetComponents<MonoBehaviour>();

        StartCoroutine(CullRoutine());
    }
    IEnumerator CullRoutine()
    {
        var sqrCullDistance = cullDistance * cullDistance;
        while (true)
        {
            float distSq = (player.position - transform.position).sqrMagnitude;
            bool shouldCull = distSq > sqrCullDistance;

            if (shouldCull && !isCulled)
                Cull();

            else if (!shouldCull && isCulled)
                Uncull();

            yield return new WaitForSeconds(cullCheckInterval);
        }
        
    }

    void Cull()
    {
        isCulled = true;

        // Disable all enemy logic EXCEPT this script
        foreach (var script in enemyScripts)
            if (script != this) script.enabled = false;

        if (rb != null) rb.simulated = false;
        if (col != null) col.enabled = false;
        if (polCol != null) polCol.enabled = false;
        if (anim != null) anim.enabled = false;
        if (healthBar != null) healthBar.SetActive(false);
        
    }

    void Uncull()
    {
        isCulled = false;

        foreach (var script in enemyScripts)
            if (script != this) script.enabled = true;

        if (rb != null) rb.simulated = true;
        if (col != null) col.enabled = true;
        if (polCol != null) polCol.enabled = true;
        if (anim != null) anim.enabled = true;
        if (healthBar != null) healthBar.SetActive(true);
    }
}
