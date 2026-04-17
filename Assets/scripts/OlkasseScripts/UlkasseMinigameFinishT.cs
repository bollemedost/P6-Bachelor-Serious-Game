using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
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

    [Header("Button Listener")]
    [Tooltip("Turn this ON only in the scene where this script should react to the continue button. Turn it OFF in part 2 so MinigameCompleteUI handles the button alone.")]
    public bool attachContinueButtonListener = true;

    [Header("Small Safety Delay")]
    [Tooltip("Prevents the panel from appearing in the exact same frame as the last placement/audio event.")]
    public float finishDelay = 0.1f;

    [Header("Google Sheets Stats")]
    [Tooltip("Your deployed Apps Script web app URL.")]
    public bool enableStatsLogging = true;
    public string googleScriptUrl = "https://script.google.com/macros/s/AKfycbyEJNpQBf5KjuV3dIg0IycgDfIpoSTXsrAiC85be42KmN0QZDUFqHlWIj22K4pfJtLx-A/exec";
    public string minigameName = "Ulkassetale";
    public string partName = "Part 1";
    public bool debugNetworkLogs = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool finished = false;
    private bool waitingForContinue = false;
    private bool slotsCompleted = false;
    private float slotsCompletedTime = -1f;
    private bool isLoadingNextScene = false;

    private float sceneStartRealtime;
    private string sceneStartUtc;
    private int sceneStartErrorCount;

    private static string sharedSessionId = "";
    private static int sharedErrorCount = 0;

    public static void RegisterError()
    {
        sharedErrorCount++;
    }

    public static string GetOrCreateSessionId()
    {
        if (string.IsNullOrEmpty(sharedSessionId))
            sharedSessionId = Guid.NewGuid().ToString("N");

        return sharedSessionId;
    }

    public static int GetGlobalErrorCount()
    {
        return sharedErrorCount;
    }

    void Start()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (audioManager == null)
            audioManager = UlkassePart1AudioManager.Instance;

        if (attachContinueButtonListener && continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
            continueButton.onClick.AddListener(OnContinuePressed);

            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Continue button listener attached.");
        }
        else
        {
            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Continue button listener NOT attached.");
        }

        GetOrCreateSessionId();
        sceneStartRealtime = Time.realtimeSinceStartup;
        sceneStartUtc = DateTime.UtcNow.ToString("o");
        sceneStartErrorCount = GetGlobalErrorCount();
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
            audioDone = !audioManager.IsAnyManagedAudioPlayingOrQueued;
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

        if (audioManager != null)
        {
            audioManager.BeginCompletionAudioLock();
        }

        if (showCompletionMessageBeforeSceneChange)
        {
            waitingForContinue = true;

            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Minigame complete. Audio locked. Showing completion panel.");

            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UlkasseMinigameFinishT] No completionPanel assigned. Logging and loading next scene directly.");
                StartLoadingNextScene();
            }
        }
        else
        {
            if (debugLogs)
                Debug.Log("[UlkasseMinigameFinishT] Minigame complete. Logging and loading next scene directly.");
            StartLoadingNextScene();
        }
    }

    public void OnContinuePressed()
    {
        if (!waitingForContinue)
            return;

        if (debugLogs)
            Debug.Log("[UlkasseMinigameFinishT] Continue button pressed. Logging stats and loading next scene.");

        waitingForContinue = false;

        if (completionPanel != null)
            completionPanel.SetActive(false);

        StartLoadingNextScene();
    }

    void StartLoadingNextScene()
    {
        if (isLoadingNextScene)
            return;

        isLoadingNextScene = true;
        StartCoroutine(LogStatsThenLoadScene());
    }

    IEnumerator LogStatsThenLoadScene()
    {
        if (enableStatsLogging && !string.IsNullOrWhiteSpace(googleScriptUrl))
        {
            yield return StartCoroutine(SendStatsToGoogleSheets());
        }

        LoadNextScene();
    }

    IEnumerator SendStatsToGoogleSheets()
    {
        float timeElapsedSeconds = Time.realtimeSinceStartup - sceneStartRealtime;
        int errorsThisPart = Mathf.Max(0, GetGlobalErrorCount() - sceneStartErrorCount);

        string minigameValue = string.IsNullOrWhiteSpace(partName)
            ? minigameName
            : (minigameName + " - " + partName);

        string url =
            googleScriptUrl +
            "?minigame=" + UnityWebRequest.EscapeURL(minigameValue) +
            "&sessionId=" + UnityWebRequest.EscapeURL(GetOrCreateSessionId()) +
            "&startTimeUtc=" + UnityWebRequest.EscapeURL(sceneStartUtc) +
            "&timeElapsedSeconds=" + UnityWebRequest.EscapeURL(timeElapsedSeconds.ToString("F2", CultureInfo.InvariantCulture)) +
            "&errors=" + UnityWebRequest.EscapeURL(errorsThisPart.ToString());

        if (debugNetworkLogs)
            Debug.Log("[UlkasseMinigameFinishT] Sending stats: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            bool failed =
                request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError;

            if (failed)
            {
                Debug.LogWarning("[UlkasseMinigameFinishT] Stats send failed: " + request.error);
            }
            else
            {
                if (debugNetworkLogs)
                    Debug.Log("[UlkasseMinigameFinishT] Stats send success: " + request.downloadHandler.text);
            }
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[UlkasseMinigameFinishT] nextSceneName is empty.");
        }
    }
}