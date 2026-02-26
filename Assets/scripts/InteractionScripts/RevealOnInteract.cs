using UnityEngine;
using System.Collections;

public class RevealOnInteract : Interactable
{
    [Header("Animation")]
    public Animator animator;
    public string animationTriggerName = "Reveal";

    [Header("Spawn Settings")]
    public GameObject objectToSpawn;
    public Transform spawnPoint;
    public float spawnDelay = 0f;

    [Header("Interaction Settings")]
    public bool interactOnlyOnce = true;

    private bool hasInteracted = false;
    private bool isInteracting = false;

    public override void Interact()
    {
        if (interactOnlyOnce && hasInteracted)
            return;

        StartCoroutine(HandleInteraction());
    }

    private IEnumerator HandleInteraction()
    {
        isInteracting = true;

        // Hide canvas immediately
        if (canvas != null)
            canvas.SetActive(false);

        // Play animation
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
            animator.SetTrigger(animationTriggerName);

        // Wait before spawning
        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        // Spawn object
        if (objectToSpawn != null && spawnPoint != null)
            Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);

        hasInteracted = true;
        isInteracting = false;
    }

    protected override bool IsCurrentlyInteracting()
    {
        // 🔒 Prevent canvas from ever showing again after interaction
        if (interactOnlyOnce && hasInteracted)
            return true;

        return isInteracting;
    }
}