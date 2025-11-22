using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class AOEBlast : MonoBehaviour
{
    public GameObject Impact;
    public int damage;
    [SerializeField] private GameObject DamageText;

    void OnTriggerEnter2D(Collider2D col)
    {
        // Check if the collider's tag is NOT the one we want to ignore (player)
        if (!col.CompareTag("Player"))
        {
            // Check if the collider's tag matches the tag of objects we can attack
            if (col.CompareTag("Enemy"))
            {
                // Attempt to get the IDamageable component from the collided object
                IDamageable target = col.GetComponent<IDamageable>();
                if (target != null) // If the object can take damage
                {
                    // Apply damage to the target
                    target.TakeDamage(damage);

                    // Create floating damage text at the target's position
                    createDamageText(damage, col.transform.position);

                    // Create a small sparks effect at the point of impact
                    GameObject HitParticle = Instantiate(Impact, transform.position, quaternion.identity);
                    // Destroy the sparks effect after 1.5 seconds
                    Destroy(HitParticle, 1.5f);
                }
            }
        }
    }
    
    void createDamageText(int dmg, Vector3 position)
    {
        // create a new instance of the damage text at the given position
        GameObject newText = Instantiate(DamageText, position, Quaternion.identity);

        // access TextMeshPro component from child
        var tmp = newText.GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
            tmp.text = dmg.ToString(); // set the text of tmp to the given damage
    }
}
