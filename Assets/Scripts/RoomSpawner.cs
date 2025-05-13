using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public List<GameObject> roomPrefabs;

    private Transform currentExit;
    private int roomCounter = 1;

    void Start()
    {
        GameObject firstRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentExit = FindChildByName(firstRoom.transform, "RoomExit");

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

        int index = UnityEngine.Random.Range(0, roomPrefabs.Count);
        GameObject nextRoomPrefab = roomPrefabs[index];

        Transform nextRoomStart = FindChildByName(nextRoomPrefab.transform, "RoomStart");
        if (nextRoomStart == null)
        {
            UnityEngine.Debug.LogError($"[RoomSpawner] Room prefab '{nextRoomPrefab.name}' is missing a RoomStart.");
            return;
        }

        Vector3 offset = currentExit.position - nextRoomStart.position;
        GameObject nextRoomInstance = Instantiate(nextRoomPrefab, offset, Quaternion.identity);

        currentExit = FindChildByName(nextRoomInstance.transform, "RoomExit");
        if (currentExit == null)
        {
            UnityEngine.Debug.LogWarning($"[RoomSpawner] Room '{nextRoomInstance.name}' is missing a RoomExit.");
        }

        UnityEngine.Debug.Log($"[RoomSpawner] Spawned Room {roomCounter++}: {nextRoomInstance.name} at {offset}");
        AddSpawnTrigger(nextRoomInstance);
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
