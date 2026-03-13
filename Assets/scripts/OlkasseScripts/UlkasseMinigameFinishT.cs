using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UlkasseMinigameFinishT : MonoBehaviour
{
    [Header("Finish Condition")]
    public slot[] allSlots;

    [Header("Next Scene")]
    public string nextSceneName = "Scene13Home1915";

    [Header("Optional Completion Popup")]
    public bool showCompletionMessageBeforeSceneChange = false;
    public GameObject completionPanel;
    public Button continueButton;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool finished = false;
    private bool waitingForContinue = false;

    void Start()
    {
        // Always hide panel at scene start
        if (completionPanel != null)
            completionPanel.SetActive(false);

        // Hook up button automatically
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }

    void Update()
    {
        if (finished) return;
        if (allSlots == null || allSlots.Length == 0) return;

        if (AreAllSlotsCompleted())
        {
            FinishMinigame();
        }
    }

    bool AreAllSlotsCompleted()
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] == null) continue;

            if (allSlots[i].acceptAnyItem)
            {
                if (allSlots[i].transform.childCount == 0)
                    return false;
            }
            else
            {
                if (!allSlots[i].IsCorrectPlaced)
                    return false;
            }
        }

        return true;
    }

    void FinishMinigame()
    {
        finished = true;

        if (showCompletionMessageBeforeSceneChange)
        {
            waitingForContinue = true;

            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Minigame complete. Showing completion panel.");

            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UlkasseMinigameFinishT] No completionPanel assigned. Loading next scene directly.");
                LoadNextScene();
            }
        }
        else
        {
            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Minigame complete. Loading next scene directly.");

            LoadNextScene();
        }
    }

    public void OnContinuePressed()
    {
        if (!waitingForContinue)
            return;

        if (debugLogs)
            Debug.Log("[UlkasseMinigameFinishT] Continue button pressed. Loading next scene.");

        waitingForContinue = false;

        if (completionPanel != null)
            completionPanel.SetActive(false);

        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.FadeToScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}