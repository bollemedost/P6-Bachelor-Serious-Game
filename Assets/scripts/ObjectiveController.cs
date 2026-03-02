using UnityEngine;
using TMPro;

public class ObjectiveController : MonoBehaviour
{
    [Header("Event References")]
    public GameEvent startObjectiveEvent;   // e.g. TalkedToHomelessMan
    public GameEvent completeObjectiveEvent; // e.g. GiveHomelessManMoney

    [Header("UI")]
    public GameObject objectiveCanvas;
    public TextMeshProUGUI objectiveText;
    [TextArea] public string objectiveDescription;

    private EventManager eventManager;
    private bool objectiveShown = false;

    private void Start()
    {
        eventManager = Object.FindFirstObjectByType<EventManager>();

        if (objectiveCanvas != null)
            objectiveCanvas.SetActive(false);

        if (objectiveText != null)
            objectiveText.text = objectiveDescription;
    }

    private void Update()
    {
        if (eventManager == null) return;

        // Show objective when start event is completed
        if (!objectiveShown && eventManager.IsEventCompleted(startObjectiveEvent))
        {
            objectiveShown = true;
            if (objectiveCanvas != null)
                objectiveCanvas.SetActive(true);
        }

        // Hide objective when completion event is completed
        if (objectiveShown && eventManager.IsEventCompleted(completeObjectiveEvent))
        {
            if (objectiveCanvas != null)
                objectiveCanvas.SetActive(false);
        }
    }
}