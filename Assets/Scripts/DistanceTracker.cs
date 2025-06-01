using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI distanceText;
    public PlayerController playerController;

    private void Start()
    {
        if (distanceText == null)
        {
            Debug.LogError("DistanceText (TextMeshProUGUI) is not assigned in the DistanceTracker script!");
            enabled = false;
            return;
        }
        distanceText.text = "0m";
    }

    void Update()
    {
        if (playerController != null)
        {
            float distance = playerController.HorizontalDistanceTraveledRight;
            distanceText.text = string.Format("{0:0}m", Mathf.FloorToInt(distance));
        }
        else
        {
            if (playerController == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                }
            }

            if (playerController == null) distanceText.text = "---m";
        }
    }

    public void SetPlayer(PlayerController pc)
    {
        playerController = pc;
    }
}