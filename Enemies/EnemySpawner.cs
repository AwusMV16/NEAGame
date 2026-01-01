using System;
using System.Collections.Generic;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] Enemies;
    private List<GameObject> spawnedEnemies = new();
    private SpawnManager spawnManager;
    private int numberOfEnemies;

    void Awake()
    {
        numberOfEnemies = UnityEngine.Random.Range(1, 4);
        spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
            spawnManager.RegisterEnemySpawner(this); 
    }
    void Start()
    {
        SpawnEnemies();
    }
    public void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, Enemies.Length);
            GameObject newEnemy = Instantiate(Enemies[randomIndex], transform.position, Quaternion.identity, transform);
            spawnedEnemies.Add(newEnemy);
        }
    }

    public void DestroyEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }
}
