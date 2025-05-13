using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    [Tooltip("Tag of the GameObjects to reset.")]
    public string enemyTag = "Enemy";
    public string playerTag = "Player";

    private Dictionary<GameObject, TransformData> initialTransforms = new Dictionary<GameObject, TransformData>();

    // Simple struct to store position and rotation
    private struct TransformData
    {
        public Vector3 position;
    }

    private void Awake()
    {
        // Find all GameObjects with the specified tag and store their initial transforms
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        GameObject[] targets = enemies.Concat(players).ToArray();

        foreach (GameObject target in targets)
        {
            initialTransforms.Add(target, new TransformData { position = target.transform.position });
        }
    }

    public void ResetEntities()
    {
        foreach (var pair in initialTransforms)
        {
            if (pair.Key != null)
            {
                pair.Key.transform.position = pair.Value.position;
            }
            else
            {
                Debug.LogWarning($"Resettable object was destroyed and cannot be reset.");
            }
        }
    }
}
