using UnityEngine;

public class UlkasseSlotOrderManager : MonoBehaviour
{
    [Header("Ordered Slots")]
    [Tooltip("Put the slots here in the exact order the player must complete them.")]
    public slot[] orderedSlots;

    [Header("Debug")]
    public bool debugLogs = true;

    private int currentRequiredIndex = 0;

    public int CurrentRequiredIndex => currentRequiredIndex;

    public bool IsSlotCurrentlyAllowed(slot targetSlot)
    {
        if (orderedSlots == null || orderedSlots.Length == 0)
            return true; // fail-safe: if nothing assigned, do not block anything

        if (currentRequiredIndex < 0 || currentRequiredIndex >= orderedSlots.Length)
            return false;

        return orderedSlots[currentRequiredIndex] == targetSlot;
    }

    public void NotifyCorrectPlacement(slot completedSlot)
    {
        if (orderedSlots == null || orderedSlots.Length == 0)
            return;

        if (currentRequiredIndex >= orderedSlots.Length)
            return;

        if (orderedSlots[currentRequiredIndex] == completedSlot)
        {
            currentRequiredIndex++;

            if (debugLogs)
                Debug.Log("[UlkasseSlotOrderManager] Correct order placement accepted. Next required slot index = " + currentRequiredIndex);
        }
        else
        {
            if (debugLogs)
                Debug.LogWarning("[UlkasseSlotOrderManager] A slot reported completion out of order: " + completedSlot.name);
        }
    }

    public void ResetOrder()
    {
        currentRequiredIndex = 0;

        if (debugLogs)
            Debug.Log("[UlkasseSlotOrderManager] Order reset.");
    }

    public bool IsSequenceComplete()
    {
        return orderedSlots != null && currentRequiredIndex >= orderedSlots.Length;
    }
}