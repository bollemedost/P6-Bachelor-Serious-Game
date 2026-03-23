using UnityEngine;

public class PostDialogueUI : MonoBehaviour
{
    [Header("Event To Listen For")]
    public GameEvent triggerEvent;

    [Header("UI Root")]
    public GameObject uiPanel;

    [Header("Animator (Optional)")]
    public Animator uiAnimator;

    private void OnEnable()
    {
        EventManager.OnEventCompleted += OnEventCompletedHandler;
    }

    private void OnDisable()
    {
        EventManager.OnEventCompleted -= OnEventCompletedHandler;
    }

    private void OnEventCompletedHandler(GameEvent completedEvent)
    {
        if (completedEvent == triggerEvent)
        {
            if (uiPanel != null)
                uiPanel.SetActive(true);

            if (uiAnimator != null)
                uiAnimator.SetTrigger("Open");
        }
    }
}