using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class TrapCuller : MonoBehaviour
{
    public float cullDistance = 40f;
    private Transform player;
    private Collider2D col;
    private Animator anim;
    private Light2D light2D;
    private MonoBehaviour[] trapScripts;  // all behaviour on this trap
    [SerializeField] private float cullCheckInterval = 0.05f;
    
    private bool isCulled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        col = GetComponent<Collider2D>();

        // Get ALL scripts attached to this trap
        trapScripts = GetComponents<MonoBehaviour>();

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
        foreach (var script in trapScripts)
            if (script != this) script.enabled = false;

        if (col != null) col.enabled = false;
    }

    void Uncull()
    {
        isCulled = false;

        foreach (var script in trapScripts)
            if (script != this) script.enabled = true;

        if (col != null) col.enabled = true;
    }
}
