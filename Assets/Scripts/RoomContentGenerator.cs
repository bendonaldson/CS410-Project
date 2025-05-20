using System;
using UnityEngine;

public class RoomContentGenerator : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public GameObject[] obstaclePrefabs;
    public GameObject[] trapPrefabs;

    void Start()
    {
        SpawnAtMarkers("EnemySpawn", enemyPrefabs);
        SpawnAtMarkers("ObstacleSpawn", obstaclePrefabs);
        SpawnAtMarkers("TrapSpawn", trapPrefabs);
    }

    void SpawnAtMarkers(string prefix, GameObject[] pool)
    {
        if (pool == null || pool.Length == 0) return;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith(prefix))
            {
                // 50% chance to spawn (customizable)
                //if (UnityEngine.Random.value < 0.5f) continue;

                int index = UnityEngine.Random.Range(0, pool.Length);
                Instantiate(pool[index], child.position, Quaternion.identity, transform);
                Destroy(child.gameObject); // remove marker after use
            }
        }
    }
}