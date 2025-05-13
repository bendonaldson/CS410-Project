using UnityEngine;

public class RoomSpawnTrigger : MonoBehaviour
{
    public RoomSpawner spawner;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        UnityEngine.Debug.Log("[RoomSpawnTrigger] Player triggered next room spawn.");
        spawner.SpawnNextRoom();
        Destroy(this); // optional: avoid duplicate trigger
    }
}
