using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomSpawner : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public List<GameObject> roomPrefabs;
    public int numberOfRooms = 5;

    private Transform currentExit;

    void Start()
    {
        SpawnRooms();
    }

    void SpawnRooms()
    {
        // 1. Spawn StartRoom at origin
        GameObject currentRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log($"Spawned Start Room: {currentRoom.name} at position: {Vector3.zero}");
        currentExit = currentRoom.transform.Find("RoomExit");

        // 2. Randomly pick and spawn N more rooms
        List<GameObject> pool = new List<GameObject>(roomPrefabs);

        for (int i = 0; i < numberOfRooms; i++)
        {
            if (pool.Count == 0) break;

            int index = Random.Range(0, pool.Count);
            GameObject nextRoomPrefab = pool[index];
            Transform nextRoomStart = nextRoomPrefab.transform.Find("RoomStart");
            GameObject nextRoomInstance;

            if (nextRoomStart != null && currentExit != null)
            {
                Vector3 offset = currentExit.position - nextRoomStart.position;
                nextRoomInstance = Instantiate(nextRoomPrefab, offset, Quaternion.identity);
                Debug.Log($"Spawned Room {i + 1}: {nextRoomInstance.name} (Prefab: {nextRoomPrefab.name}) at position: {offset}, connected to {currentRoom.name}'s exit at {currentExit.position}");
                currentRoom = nextRoomInstance;
                currentExit = nextRoomInstance.transform.Find("RoomExit");
                pool.RemoveAt(index);
            }
            else
            {
                Debug.LogError($"Room prefab {nextRoomPrefab.name} is missing a RoomStart or the current room is missing a RoomExit!");
                break;
            }
        }
    }
}