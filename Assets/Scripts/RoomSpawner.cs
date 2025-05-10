using UnityEngine;
using System.Collections.Generic;
using System;

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
        // Spawn the StartRoom
        GameObject currentRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentExit = currentRoom.transform.Find("RoomExit");

        // Spawn additional rooms
        for (int i = 0; i < numberOfRooms; i++)
        {
            if (roomPrefabs.Count == 0 || currentExit == null) return;

            int index = UnityEngine.Random.Range(0, roomPrefabs.Count);
            GameObject nextRoom = Instantiate(roomPrefabs[index], currentExit.position, Quaternion.identity);
            currentExit = nextRoom.transform.Find("RoomExit");
        }
    }
}
