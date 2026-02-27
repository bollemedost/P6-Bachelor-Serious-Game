/*using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCWalkEventInteraction : MonoBehaviour
{
    [Header("Waypoints & Movement")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float waypointStopDistance = 0.2f;
    public float playerFollowDistance = 5f;

    [Header("Player & Events")]
    public Transform player;
    public GameEvent prerequisiteEvent; // e.g., MomTalk2
    public GameEvent arrivalEvent;      // Event triggered when final waypoint reached

    [Header("Animator")]
    public Animator animator;
    public string walkBoolName = "isWalking";

    private int currentWaypointIndex = 0;
    private bool isWalking = false;
    private EventManager eventManager;

    private void Awake()
    {
        eventManager = FindObjectOfType<EventManager>();
    }

    private void OnEnable()
    {
        if (eventManager != null)
            EventManager.OnEventCompleted += HandleEventCompleted;
    }

    private void OnDisable()
    {
        if (eventManager != null)
            EventManager.OnEventCompleted -= HandleEventCompleted;
    }

    private void HandleEventCompleted(GameEvent completedEvent)
    {
        // Only start walking when the prerequisite event is completed
        if (completedEvent == prerequisiteEvent && !isWalking)
        {
            StartCoroutine(WalkRoutine());
        }
    }

    private IEnumerator WalkRoutine()
    {
        if (waypoints.Length == 0 || player == null)
            yield break;

        isWalking = true;
        currentWaypointIndex = 0;

        // Start exactly at the first waypoint if far away
        if (Vector3.Distance(transform.position, waypoints[0].position) > 0.1f)
            transform.position = waypoints[0].position;

        while (currentWaypointIndex < waypoints.Length)
        {
            Transform target = waypoints[currentWaypointIndex];

            while (Vector3.Distance(transform.position, target.position) > waypointStopDistance)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer > playerFollowDistance)
                {
                    if (animator != null) animator.SetBool(walkBoolName, false); // pause animation
                }
                else
                {
                    // Move NPC toward waypoint
                    Vector3 direction = (target.position - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.deltaTime;

                    // Rotate NPC
                    direction.y = 0;
                    if (direction != Vector3.zero)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

                    if (animator != null) animator.SetBool(walkBoolName, true); // walking animation
                }

                yield return null;
            }

            currentWaypointIndex++;
        }

        // Stop walking animation
        if (animator != null) animator.SetBool(walkBoolName, false);
        isWalking = false;

        // Trigger arrival event
        if (arrivalEvent != null && eventManager != null)
            eventManager.CompleteEvent(arrivalEvent);

        Debug.Log("NPC reached final waypoint.");
    }
}*/