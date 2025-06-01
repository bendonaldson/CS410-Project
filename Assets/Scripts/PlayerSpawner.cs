using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    IEnumerator SpawnAfterDelay()
    {
        yield return null;

        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");

        if (spawnPoint != null && playerPrefab != null)
        {
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
            PlayerController spawnedPlayerController = playerInstance.GetComponent<PlayerController>();

            if (spawnedPlayerController == null)
            {
                Debug.LogError("Spawned player prefab does not have a PlayerController component!");
                yield break;
            }

            CameraFollow mainCameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (mainCameraFollow != null)
            {
                mainCameraFollow.target = playerInstance.transform;
            }
            else
            {
                Debug.LogWarning("Main Camera does not have a CameraFollow component.");
            }

            DistanceTracker distanceTracker = FindFirstObjectByType<DistanceTracker>();
            if (distanceTracker != null)
            {
                distanceTracker.SetPlayer(spawnedPlayerController);
            }
            else
            {
                Debug.LogWarning("DistanceTracker component not found in the scene. Distance will not be displayed.");
            }
        }
        else
        {
            if (spawnPoint == null) Debug.LogError("PlayerSpawnPoint GameObject not found!");
            if (playerPrefab == null) Debug.LogError("PlayerPrefab is not assigned in PlayerSpawner!");
        }
    }
}