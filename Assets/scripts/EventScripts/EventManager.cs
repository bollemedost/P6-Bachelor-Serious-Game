using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private HashSet<GameEvent> completedEvents = new HashSet<GameEvent>();

    public static event Action<GameEvent> OnEventCompleted;

    public bool IsEventCompleted(GameEvent gameEvent)
    {
        return completedEvents.Contains(gameEvent);
    }

    public void CompleteEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        if (!completedEvents.Contains(gameEvent))
        {
            completedEvents.Add(gameEvent);
            Debug.Log($"Event completed: {gameEvent.name}");

            OnEventCompleted?.Invoke(gameEvent);
        }
    }
}