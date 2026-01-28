using UnityEngine;

public class AcidTrap : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float damageInterval = 1f;
    private float lastDamageTime = -Mathf.Infinity;

    // store the damageable target currently inside the acid
    private IDamageable player;

    void Update()
    {
        // if the player is in the acid
        if (player != null)
        {
            // check if enough time has passed for damage tick
            if (Time.time - lastDamageTime >= damageInterval)
            {
                // apply damage over time
                player.TakeDamage(damage);

                // reset the damage cooldown timer
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // get the damage interface from the entering player
            player = other.GetComponent<IDamageable>();

            if (player != null)
            {
                // apply immediate damage upon touching the acid
                // player.TakeDamage(damage);

                PlayerController player = other.GetComponent<PlayerController>();
                player.TakeDamagePercent(100);

                // reset the damage tick timer
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // if the object leaving is the one we were damaging, stop damaging it
            if (other.GetComponent<IDamageable>() == player)
            {
                player = null;
            }
        }
    }
}