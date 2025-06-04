using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public List<GameObject> roomPrefabs;

    private Transform currentExit;
    private int roomCounter = 1;
    private bool lastWasV;  // Tracks whether the previously spawned room’s name started with “V”

    void Start()
    {
        // Spawn the start room
        GameObject firstRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentExit = FindChildByName(firstRoom.transform, "RoomExit");

        // Initialize lastWasV based on the start room’s name
        lastWasV = startRoomPrefab.name.StartsWith("V");

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

        if (lastWasV)
        {
            // If the last room started with “V”, pick a prefab that does NOT start with “V”
            List<GameObject> nonVRooms = roomPrefabs
                .Where(r => !r.name.StartsWith("V"))
                .ToList();

            if (nonVRooms.Count > 0)
            {
                nextRoomPrefab = nonVRooms[UnityEngine.Random.Range(0, nonVRooms.Count)];
            }
            else
            {
                // If all prefabs start with “V”, just pick any to avoid an infinite loop
                nextRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Count)];
            }
        }
        else
        {
            // If the last room did NOT start with “V”, pick any prefab
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

        // Update lastWasV for the next spawn
        lastWasV = nextRoomPrefab.name.StartsWith("V");
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