using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Optional Animation")]
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private string showTrigger = "Show";
    [SerializeField] private string hideTrigger = "Hide";

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);

        if (uiAnimator != null)
        {
            uiAnimator.SetTrigger(showTrigger);
        }

        // Pause game (optional but recommended)
        Time.timeScale = 0f;
    }

    public void OnContinuePressed()
    {
        if (uiAnimator != null)
        {
            uiAnimator.SetTrigger(hideTrigger);
        }
        else
        {
            HideTutorial();
        }
    }

    // Call this from animation event OR directly
    public void HideTutorial()
    {
        tutorialPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
    }
}