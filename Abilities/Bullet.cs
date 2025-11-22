using Unity.Mathematics;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;

public class Bullet : MonoBehaviour
{
    public GameObject Impact;
    public GameObject smallSparks;

    [SerializeField] private GameObject DamageText;
    public int damage;
    public string ignoreTag; // tag to ignore
    public string attackTag; // tag to inflict damage
    public Color bulletColor;
    private SpriteRenderer sRenderer;
    public string bulletTag = null;

    void Awake()
    {
        if (!string.IsNullOrEmpty(bulletTag)) gameObject.tag = bulletTag;
    }

    void Start()
    {
        sRenderer = GetComponent<SpriteRenderer>();
        sRenderer.color = bulletColor; // initially set the color of the bullet
    }
    
    // Called automatically when another collider enters this object's trigger collider
    void OnTriggerEnter2D(Collider2D col)   
    {
        // Check if the collider's tag is NOT the one we want to ignore
        if (!col.CompareTag(ignoreTag))
        {
            if (bulletTag.NullIfEmpty() != null)
            {
                if (col.CompareTag(bulletTag)) return;
            }
            // Destroy this game object (e.g., the projectile)  
            Destroy(gameObject);
            
            // Create an impact particle effect at this object's position
            GameObject Particle = Instantiate(Impact, transform.position, quaternion.identity);
            // Destroy the particle effect after 0.5 seconds
            Destroy(Particle, 0.5f);  

            // Check if the collider's tag matches the tag of objects we can attack
            if (col.CompareTag(attackTag))
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
                    GameObject HitParticle = Instantiate(smallSparks, transform.position, quaternion.identity);
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
