using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private HashSet<string> completedEvents = new HashSet<string>();

    // Event fired when an event is completed
    public static event Action<string> OnEventCompleted;

    public bool IsEventCompleted(string eventID)
    {
        return completedEvents.Contains(eventID);
    }

    public void CompleteEvent(string eventID)
    {
        if (!completedEvents.Contains(eventID))
        {
            completedEvents.Add(eventID);
            Debug.Log($"Event completed: {eventID}");

            // Notify listeners
            OnEventCompleted?.Invoke(eventID);
        }
    }
}