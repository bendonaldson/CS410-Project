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
            GameObject player = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);

            // Assign camera follow target
            Camera.main.GetComponent<CameraFollow>().target = player.transform;
        }
        else
        {
            UnityEngine.Debug.LogError("PlayerSpawnPoint or playerPrefab is missing!");
        }
    }
}
