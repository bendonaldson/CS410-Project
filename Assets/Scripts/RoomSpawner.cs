using System;
using System.Collections.Generic;
using UnityEngine;

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
        currentExit = currentRoom.transform.Find("RoomExit");

        // 2. Randomly pick and spawn N more rooms
        List<GameObject> pool = new List<GameObject>(roomPrefabs);

        for (int i = 0; i < numberOfRooms; i++)
        {
            if (pool.Count == 0) break;

            int index = UnityEngine.Random.Range(0, pool.Count);
            GameObject room = Instantiate(pool[index], currentExit.position, Quaternion.identity);
            pool.RemoveAt(index); // optional: avoid repeats
            currentExit = room.transform.Find("RoomExit");
        }
    }
}

