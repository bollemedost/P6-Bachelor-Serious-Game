using UnityEngine;

public class WaypointWalker : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 1f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypointIndex >= waypoints.Length) return;

        Transform target = waypoints[currentWaypointIndex];

        // Move toward current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Face toward waypoint
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // If reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex++;

            // Destroy at final waypoint
            if (currentWaypointIndex >= waypoints.Length)
            {
                Destroy(gameObject);
            }
        }
    }

    public void ApplyHeadStart(float simulatedSeconds)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        float distanceToTravel = moveSpeed * simulatedSeconds;

        while (distanceToTravel > 0f && currentWaypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            float segmentDistance = Vector3.Distance(transform.position, targetPosition);

            if (segmentDistance <= distanceToTravel)
            {
                transform.position = targetPosition;
                distanceToTravel -= segmentDistance;
                currentWaypointIndex++;
            }
            else
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * distanceToTravel;
                distanceToTravel = 0f;
            }
        }

        // If already past the whole route, destroy immediately
        if (currentWaypointIndex >= waypoints.Length)
        {
            Destroy(gameObject);
            return;
        }

        // Face next waypoint
        Vector3 faceDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        if (faceDirection != Vector3.zero)
        {
            transform.forward = faceDirection;
        }
    }
}