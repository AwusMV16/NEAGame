using UnityEngine;
using System.Collections;

public class LaserTrap : MonoBehaviour
{
    private LineRenderer line;

    [SerializeField] private BoxCollider2D box;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask wallMask;
    private Animator anim;

    [Header("Timing")]
    [SerializeField] private float onTime = 2f;
    [SerializeField] private float offTime = 2f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float initialDelay = 0f;

    [Header("Laser")]
    [SerializeField] private float laserWidth = 0.2f;

    private float laserPower = 1f; // 0 = off, 1 = on
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        line = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
        StartCoroutine(LaserCycle());
        anim.SetBool("Active", laserPower > 0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallMask);
        float length = hit.collider != null ? hit.distance : maxDistance;

        // --- LineRenderer ---
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction * length);

        float currentWidth = laserWidth * laserPower;
        line.startWidth = currentWidth;
        line.endWidth = currentWidth;

        // --- Trigger Collider ---
        box.size = new Vector2(length, currentWidth / 4f);
        box.offset = new Vector2(length / 2f, 0f);

        box.enabled = laserPower > 0.05f;
    }
    

    void OnTriggerEnter2D(Collider2D other)
    {
        if (laserPower < 0.8f) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.TakeDamagePercent(1f);
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (laserPower < 0.8f) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.TakeDamagePercent(1f);
        }
    }

    IEnumerator FadeLaser(float target)
    {
        float start = laserPower;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            laserPower = Mathf.Lerp(start, target, t / fadeTime);
            anim.SetBool("Active", laserPower > 0);
            yield return null;
        }

        laserPower = target;
        
    }

    IEnumerator LaserCycle()
    {
        yield return new WaitForSeconds(initialDelay);
        while (true)
        {
            yield return FadeLaser(1f);
            yield return new WaitForSeconds(onTime);

            yield return FadeLaser(0f);
            yield return new WaitForSeconds(offTime);
        }
    }
}
