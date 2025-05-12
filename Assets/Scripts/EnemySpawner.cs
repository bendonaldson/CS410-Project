using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    void Start()
    {
        SpawnSpecificEnemies();
    }

    void SpawnSpecificEnemies()
    {
        SpecificEnemySpawnPoint[] spawnPoints = GetComponentsInChildren<SpecificEnemySpawnPoint>();

        foreach (SpecificEnemySpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.enemyPrefabToSpawn != null)
            {
                Instantiate(spawnPoint.enemyPrefabToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);
            }
            else
            {
                Debug.LogWarning($"EnemySpawnPoint at {spawnPoint.transform.position} in {gameObject.name} has no enemy prefab assigned.");
            }
        }
    }
}