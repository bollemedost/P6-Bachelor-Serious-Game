using UnityEngine;

public class WaypointWalker : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 1f; // adjust walking speed here
    public float destroyAfterSeconds = 15f; // auto destroy after this time

    private int currentWaypointIndex = 0;

    void Start()
    {
        // Optional: destroy after a set amount of time just in case
        Destroy(gameObject, destroyAfterSeconds);
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];

        // Move toward waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Rotate toward waypoint smoothly
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            currentWaypointIndex++;

            // Destroy when reaching the last waypoint
            if (currentWaypointIndex >= waypoints.Length)
            {
                Destroy(gameObject);
            }
        }
    }
}