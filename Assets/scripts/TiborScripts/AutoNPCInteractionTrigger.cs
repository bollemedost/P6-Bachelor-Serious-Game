using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AutoNPCInteractionTrigger : MonoBehaviour
{
    [Header("References")]
    public NPCInteraction npcInteraction;
    public Transform playerTransform;

    [Header("Trigger Settings")]
    public float triggerDistance = 2f;
    public bool triggerOnlyOnce = true;

    [Header("Optional Scene Restriction")]
    public bool requireCorrectScene = false;
    public string requiredSceneName = "";

    private bool hasTriggered = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (hasTriggered && triggerOnlyOnce)
            return;

        if (npcInteraction == null || playerTransform == null)
            return;

        if (requireCorrectScene)
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != requiredSceneName)
                return;
        }

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= triggerDistance)
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        if (npcInteraction == null)
            return;

        npcInteraction.Interact();

        if (triggerOnlyOnce)
            hasTriggered = true;
    }
}


//References:
//Troublshooting/inspiration with chatgpt