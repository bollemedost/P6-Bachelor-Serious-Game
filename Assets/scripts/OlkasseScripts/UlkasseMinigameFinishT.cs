using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UlkasseMinigameFinishT : MonoBehaviour
{
    [Header("Finish Condition")]
    public slot[] allSlots;

    [Header("Optional Audio Finish Requirement")]
    public bool waitForNarrationToFinish = true;
    public UlkassePart1AudioManager audioManager;

    [Header("Next Scene")]
    public string nextSceneName = "Scene13Home1915";

    [Header("Optional Completion Popup")]
    public bool showCompletionMessageBeforeSceneChange = true;
    public GameObject completionPanel;
    public Button continueButton;

    [Header("Small Safety Delay")]
    [Tooltip("Prevents the panel from appearing in the exact same frame as the last placement/audio event.")]
    public float finishDelay = 0.1f;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool finished = false;
    private bool waitingForContinue = false;
    private bool slotsCompleted = false;
    private float slotsCompletedTime = -1f;

    void Start()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (audioManager == null)
            audioManager = UlkassePart1AudioManager.Instance;

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

        if (!slotsCompleted && AreAllSlotsCompleted())
        {
            slotsCompleted = true;
            slotsCompletedTime = Time.time;

            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] All slots completed.");
        }

        if (!slotsCompleted)
            return;

        if (Time.time < slotsCompletedTime + finishDelay)
            return;

        if (!waitForNarrationToFinish)
        {
            FinishMinigame();
            return;
        }

        bool audioDone = true;

        if (audioManager != null)
        {
            audioDone = !audioManager.IsAnyManagedAudioPlaying;
        }

        if (audioDone)
        {
            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Slots are completed and audio is finished. Showing completion panel.");

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
        if (finished) return;

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