using UnityEngine;
using UnityEngine.UIElements;

public class AiChase : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distanceBetween;

    public float idleSpeed;
    public float movementDuration;
    public float movementBoundaryX;
    public float movementBoundaryY;

    private Vector2 targetDirection;
    private float timeToChangeDirection;

    private float distance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector2.Distance(transform.position, player.transform.position);
        Vector2 direction = player.transform.position - transform.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        

        if (distance < distanceBetween)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
            if (Mathf.Abs(angle) > 90)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            }
        }
        /*
        else
        {
            float randomNumberX = Random.Range(-1f, 1f);
            float randomNumberY = Mathf.Abs(randomNumberX) - 1;
            int mod = Random.Range(0, 2);
            randomNumberY = randomNumberY - mod;
            Vector3 randDestination = new Vector3(this.transform.position.x + randomNumberX, this.transform.position.y + randomNumberY, 0f);
            transform.position = Vector2.MoveTowards(this.transform.position, randDestination, speed * Time.deltaTime);
        }
        */
    }
}
