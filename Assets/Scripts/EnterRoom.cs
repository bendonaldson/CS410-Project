using UnityEngine;

public class EnterRoom : MonoBehaviour, IInteractable
{
    [Tooltip("Make sure this Collider2D is set to 'Is Trigger' in the Inspector.")]
    [SerializeField]
    private Collider2D _interactTriggerCollider; // Reference to the trigger collider

    private void Awake()
    {
        // If not assigned in Inspector, try to get it from this GameObject
        if (_interactTriggerCollider == null)
        {
            _interactTriggerCollider = GetComponent<Collider2D>();
        }

        // Basic check to ensure the collider is a trigger
        if (_interactTriggerCollider != null && !_interactTriggerCollider.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set to 'Is Trigger'. Interaction will not work as expected.", this);
        }
    }

    // Called when another collider enters this object's trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure the entering collider belongs to the Player
        // IMPORTANT: Make sure your Player GameObject has the "Player" tag set!
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCurrentInteractable(this); // Notify the player controller
            }
        }
    }

    // Called when another collider exits this object's trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // Ensure the exiting collider belongs to the Player
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ClearCurrentInteractable(this); // Notify the player controller
            }
        }
    }

    // This is the method required by the IInteractable interface
    public void Interact()
    {
        // >>> IMPORTANT: Put your specific interaction logic here <<<
        Debug.Log($"You interacted with the {gameObject.name}!");

        // Example: If this was a door, you'd open/close it.
        // GetComponent<DoorScript>().ToggleDoor();

        // Example: If this was a collectible, you'd add it to inventory and destroy it.
        // FindObjectOfType<InventoryManager>().AddItem("Coin");
        // Destroy(gameObject);
    }
}
