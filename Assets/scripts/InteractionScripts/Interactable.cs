using UnityEngine;
using System.Collections.Generic;

public abstract class Interactable : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject canvas;
    public float interactDistance = 3f;
    public string playerTag = "Player";

    protected Transform player;

    // 🔒 Global interaction lock
    public static bool interactionLocked = false;

    // 🔥 Static list of all interactables
    private static List<Interactable> allInteractables = new List<Interactable>();

    protected virtual void Awake()
    {
        allInteractables.Add(this);
    }

    protected virtual void OnDestroy()
    {
        allInteractables.Remove(this);
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        if (canvas != null)
            canvas.SetActive(false);
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // Only consider the closest interactable
        Interactable closest = GetClosestInteractable();
        bool isClosest = closest == this;

        if (canvas != null)
        {
            canvas.SetActive(isClosest && CanInteract() && !IsCurrentlyInteracting() && !interactionLocked);
        }

        // 🚫 If interaction is locked, block all E presses
        if (interactionLocked)
            return;

        // Toggle interaction with E if closest
        if (isClosest && Input.GetKeyDown(KeyCode.E))
        {
            if (IsCurrentlyInteracting())
                StopInteraction();
            else
                Interact();
        }
    }

    private Interactable GetClosestInteractable()
    {
        float closestDistance = Mathf.Infinity;
        Interactable closest = null;

        foreach (Interactable interactable in allInteractables)
        {
            if (interactable.player == null) continue;

            float distance = Vector3.Distance(
                interactable.transform.position,
                interactable.player.position
            );

            if (distance <= interactable.interactDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    // 🔓 Call this when interaction finishes
    protected void UnlockInteraction()
    {
        interactionLocked = false;
    }

    // 🔒 Call this when interaction starts
    protected void LockInteraction()
    {
        interactionLocked = true;
    }

    protected virtual bool IsCurrentlyInteracting()
    {
        return false;
    }

    protected virtual void StopInteraction()
    {
    }

    public abstract void Interact();

    protected virtual bool CanInteract()
    {
        return true;
    }
}