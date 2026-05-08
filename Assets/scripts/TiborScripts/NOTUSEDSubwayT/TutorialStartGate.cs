using UnityEngine;

public class TutorialStartGate : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialCanvas;

    [Header("Minigame Root")]
    [Tooltip("Put the main minigame parent here so gameplay is hidden/disabled until tutorial is closed.")]
    public GameObject minigameRoot;

    [Header("Audio Manager")]
    public UlkassePart1AudioManager audioManager;

    [Header("Optional")]
    [Tooltip("Freeze game time while tutorial is open.")]
    public bool pauseTimeWhileTutorialIsOpen = true;

    [Tooltip("If true, the tutorial is shown automatically at scene start.")]
    public bool showTutorialOnStart = true;

    private bool gameStarted = false;

    void Start()
    {
        if (showTutorialOnStart)
        {
            ShowTutorial();
        }
        else
        {
            StartMinigame();
        }
    }

    public void ShowTutorial()
    {
        gameStarted = false;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (minigameRoot != null)
            minigameRoot.SetActive(false);

        if (pauseTimeWhileTutorialIsOpen)
            Time.timeScale = 0f;
    }

    public void OnPressStartButton()
    {
        StartMinigame();
    }

    public void StartMinigame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        if (pauseTimeWhileTutorialIsOpen)
            Time.timeScale = 1f;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (minigameRoot != null)
            minigameRoot.SetActive(true);

        if (audioManager != null)
            audioManager.StartSequence();
    }
}