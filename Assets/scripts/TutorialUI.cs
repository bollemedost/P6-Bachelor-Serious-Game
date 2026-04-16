using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Optional Animation")]
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private string showTrigger = "Show";
    [SerializeField] private string hideTrigger = "Hide";

    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;

    private bool hasBeenShown = false;
    private bool isVisible = false;

    private void Start()
    {
        if (showOnStart)
        {
            ShowTutorial();
        }
        else if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    public void ShowTutorial()
    {
        if (hasBeenShown)
            return;

        if (tutorialPanel == null)
            return;

        hasBeenShown = true;
        isVisible = true;

        tutorialPanel.SetActive(true);

        if (uiAnimator != null)
        {
            uiAnimator.ResetTrigger(hideTrigger);
            uiAnimator.SetTrigger(showTrigger);
        }

        Time.timeScale = 0f;
    }

    public void OnContinuePressed()
    {
        if (!isVisible)
            return;

        if (uiAnimator != null)
        {
            uiAnimator.ResetTrigger(showTrigger);
            uiAnimator.SetTrigger(hideTrigger);
        }

        HideTutorial();
    }

    public void HideTutorial()
    {
        if (tutorialPanel == null)
            return;

        isVisible = false;
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}