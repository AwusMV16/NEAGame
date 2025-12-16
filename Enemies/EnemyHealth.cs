using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyHealthBar healthBar;
    private IEnemy enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = GetComponentInChildren<EnemyHealthBar>();
        enemy = GetComponent<IEnemy>();

        if (healthBar != null) healthBar.HideHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy != null && enemy.Health != enemy.MaxHealth && healthBar != null)
        {
            healthBar.ShowHealth();
            healthBar.UpdateHealth(enemy.Health, enemy.MaxHealth);
        }
    }
}
