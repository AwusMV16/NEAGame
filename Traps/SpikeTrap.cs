using System;
using System.Collections;
using Unity.Mathematics;
// using Unity.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private float lastDamageTime = -Mathf.Infinity;
    
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(StartWithRandomDelay());
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            // check if the damage cooldown hasnt finished
            if (Time.time - lastDamageTime < 1) return; // still on cooldown, skip damage

            // disable the player climb mode if hit
            PlayerController player = other.collider.GetComponent<PlayerController>();
            player.TakeDamagePercent(0.5f);
            player.climbEnabled = false;

            // push the player away from the spike
            Rigidbody2D rb = other.collider.GetComponent<Rigidbody2D>();
            rb.AddForceX(Math.Sign(transform.position.x - other.collider.transform.position.x) * -10000);

            // reset cooldown
            lastDamageTime = Time.time;
        }
    }

    private IEnumerator StartWithRandomDelay()
    {
        animator.speed = 0; // freeze animation at frame 0

        float delay = UnityEngine.Random.Range(0f, 0.5f); // change range as needed
        yield return new WaitForSeconds(delay);

        animator.speed = 1; // start animating
    }
}
