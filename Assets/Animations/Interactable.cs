using UnityEngine;
using System.Collections.Generic;

public abstract class Interactable : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject canvas;
    public float interactDistance = 3f;
    public string playerTag = "Player";

    protected Transform player;

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

        Interactable closest = GetClosestInteractable();

        bool isClosest = closest == this;

        if (canvas != null)
            canvas.SetActive(isClosest);

        if (isClosest && Input.GetKeyDown(KeyCode.E))
        {
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

    public abstract void Interact();
}