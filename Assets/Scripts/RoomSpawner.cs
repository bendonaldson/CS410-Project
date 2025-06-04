using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public List<GameObject> roomPrefabs;

    private Transform currentExit;
    private int roomCounter = 1;
    private bool lastWasUp;    // True if the previously spawned room’s name started with “Up”
    private bool lastWasDown;  // True if the previously spawned room’s name started with “Down”

    void Start()
    {
        // Spawn the start room
        GameObject firstRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentExit = FindChildByName(firstRoom.transform, "RoomExit");

        // Initialize lastWasUp / lastWasDown based on the start room’s name
        lastWasUp = startRoomPrefab.name.StartsWith("Up");
        lastWasDown = startRoomPrefab.name.StartsWith("Down");

        AddSpawnTrigger(firstRoom);
        UnityEngine.Debug.Log($"[RoomSpawner] Spawned StartRoom at {Vector3.zero}");
    }

    public void SpawnNextRoom()
    {
        if (roomPrefabs.Count == 0 || currentExit == null)
        {
            UnityEngine.Debug.LogWarning("[RoomSpawner] No room prefabs available or currentExit missing.");
            return;
        }

        GameObject nextRoomPrefab;

        if (lastWasUp)
        {
            // If the last room started with “Up,” disallow any prefab starting with “Down”
            List<GameObject> notDown = roomPrefabs
                .Where(r => !r.name.StartsWith("Down"))
                .ToList();

            if (notDown.Count > 0)
            {
                nextRoomPrefab = notDown[UnityEngine.Random.Range(0, notDown.Count)];
            }
            else
            {
                // Fallback if all prefabs start with “Down”
                nextRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Count)];
            }
        }
        else if (lastWasDown)
        {
            // If the last room started with “Down,” disallow any prefab starting with “Up”
            List<GameObject> notUp = roomPrefabs
                .Where(r => !r.name.StartsWith("Up"))
                .ToList();

            if (notUp.Count > 0)
            {
                nextRoomPrefab = notUp[UnityEngine.Random.Range(0, notUp.Count)];
            }
            else
            {
                // Fallback if all prefabs start with “Up”
                nextRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Count)];
            }
        }
        else
        {
            // If the last room was neither “Up” nor “Down,” pick any prefab
            nextRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Count)];
        }

        // Find the “RoomStart” marker in the new prefab
        Transform nextRoomStart = FindChildByName(nextRoomPrefab.transform, "RoomStart");
        if (nextRoomStart == null)
        {
            UnityEngine.Debug.LogError($"[RoomSpawner] Room prefab '{nextRoomPrefab.name}' is missing a RoomStart.");
            return;
        }

        // Calculate spawn position
        Vector3 offset = currentExit.position - nextRoomStart.position;
        GameObject nextRoomInstance = Instantiate(
            nextRoomPrefab,
            offset,
            nextRoomPrefab.transform.rotation
        );

        // Update currentExit to the newly spawned room’s “RoomExit”
        currentExit = FindChildByName(nextRoomInstance.transform, "RoomExit");
        if (currentExit == null)
        {
            UnityEngine.Debug.LogWarning($"[RoomSpawner] Room '{nextRoomInstance.name}' is missing a RoomExit.");
        }

        UnityEngine.Debug.Log($"[RoomSpawner] Spawned Room {roomCounter++}: {nextRoomInstance.name} at {offset}");
        AddSpawnTrigger(nextRoomInstance);

        // Update lastWasUp / lastWasDown for the next spawn
        lastWasUp = nextRoomPrefab.name.StartsWith("Up");
        lastWasDown = nextRoomPrefab.name.StartsWith("Down");
    }

    private void AddSpawnTrigger(GameObject room)
    {
        Transform spawnTrigger = FindChildByName(room.transform, "RoomSpawnTrigger");
        if (spawnTrigger == null)
        {
            UnityEngine.Debug.LogWarning($"[RoomSpawner] Room '{room.name}' is missing a RoomSpawnTrigger.");
            return;
        }

        BoxCollider2D col = spawnTrigger.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = spawnTrigger.gameObject.AddComponent<BoxCollider2D>();
        }

        col.isTrigger = true;
        col.size = new Vector2(1f, 3f);

        RoomSpawnTrigger triggerScript = spawnTrigger.GetComponent<RoomSpawnTrigger>();
        if (triggerScript == null)
        {
            triggerScript = spawnTrigger.gameObject.AddComponent<RoomSpawnTrigger>();
        }

        triggerScript.spawner = this;
        UnityEngine.Debug.Log($"[RoomSpawner] Connected RoomSpawnTrigger in room '{room.name}'");
    }

    private Transform FindChildByName(Transform parent, string targetName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                return child;
        }
        return null;
    }
}