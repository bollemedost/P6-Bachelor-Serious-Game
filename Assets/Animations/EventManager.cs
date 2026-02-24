using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Keeps track of completed events
    private HashSet<string> completedEvents = new HashSet<string>();

    // Check if an event is completed
    public bool IsEventCompleted(string eventID)
    {
        return completedEvents.Contains(eventID);
    }

    // Mark an event as completed
    public void CompleteEvent(string eventID)
    {
        if (!completedEvents.Contains(eventID))
        {
            completedEvents.Add(eventID);
            Debug.Log($"Event completed: {eventID}");
        }
    }
}