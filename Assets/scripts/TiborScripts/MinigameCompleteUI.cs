using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MinigameCompleteUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI messageText;

    [Header("Reward")]
    public int rewardCoins = 20;
    public string messageTemplate = "Du fuldførte minispillet. Du får nu tildelt {0} mønter";

    [Header("Event")]
    public GameEvent miniGameEvent;

    [Header("Scene Load")]
    public bool loadSpecificScene = true;
    public string sceneToLoad = "Scene13Home1915NOINTERACTION";

    [Header("Legacy Return Options")]
    public bool useSubwayTReturnScene = false;

    [Header("Google Sheets Stats")]
    [Tooltip("Your deployed Apps Script web app URL.")]
    public bool enableStatsLogging = true;
    public string googleScriptUrl = "https://script.google.com/macros/s/AKfycbyEJNpQBf5KjuV3dIg0IycgDfIpoSTXsrAiC85be42KmN0QZDUFqHlWIj22K4pfJtLx-A/exec";
    public string minigameName = "Ulkassetale";
    public string partName = "Part 2";
    public bool debugNetworkLogs = true;

    private bool shown = false;
    private bool doneClicked = false;

    private float sceneStartRealtime;
    private string sceneStartUtc;
    private int sceneStartErrorCount;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        // Track the start of part 2 scene
        UlkasseMinigameFinishT.GetOrCreateSessionId();
        sceneStartRealtime = Time.realtimeSinceStartup;
        sceneStartUtc = DateTime.UtcNow.ToString("o");
        sceneStartErrorCount = UlkasseMinigameFinishT.GetGlobalErrorCount();
    }

    public void Show(int coins)
    {
        rewardCoins = coins;
        shown = true;

        if (messageText != null)
            messageText.text = string.Format(messageTemplate, rewardCoins);

        if (root != null)
            root.SetActive(true);
    }

    public void OnDoneClicked()
    {
        if (doneClicked)
            return;

        doneClicked = true;

        Debug.Log("[MinigameCompleteUI] DONE CLICKED ONCE");

        UnityEngine.UI.Button btn = GetComponentInChildren<UnityEngine.UI.Button>();
        if (btn != null)
            btn.interactable = false;

        StartCoroutine(OnDoneClickedRoutine());
    }

    IEnumerator OnDoneClickedRoutine()
    {
        // SAFETY: ensure reward always works
        if (!shown)
        {
            Debug.LogWarning("[MinigameCompleteUI] Show() was not called. Forcing reward.");
            shown = true;
        }

        CoinManager.EnsureExists().AddCoin(rewardCoins);

        var em = FindObjectOfType<EventManager>();
        if (em != null && miniGameEvent != null)
        {
            em.CompleteEvent(miniGameEvent);
        }

        if (enableStatsLogging && !string.IsNullOrWhiteSpace(googleScriptUrl))
        {
            yield return StartCoroutine(SendStatsToGoogleSheets());
        }

        if (loadSpecificScene && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            yield break;
        }

        if (useSubwayTReturnScene)
        {
            SubwayTReturnToScene13T.ReturnNow();
            yield break;
        }

        ReturnToPreviousSceneT.ReturnNow();
    }

    IEnumerator SendStatsToGoogleSheets()
    {
        float timeElapsedSeconds = Time.realtimeSinceStartup - sceneStartRealtime;
        int errorsThisPart = Mathf.Max(0, UlkasseMinigameFinishT.GetGlobalErrorCount() - sceneStartErrorCount);

        string minigameValue = string.IsNullOrWhiteSpace(partName)
            ? minigameName
            : (minigameName + " - " + partName);

        string url =
            googleScriptUrl +
            "?minigame=" + UnityWebRequest.EscapeURL(minigameValue) +
            "&sessionId=" + UnityWebRequest.EscapeURL(UlkasseMinigameFinishT.GetOrCreateSessionId()) +
            "&startTimeUtc=" + UnityWebRequest.EscapeURL(sceneStartUtc) +
            "&timeElapsedSeconds=" + UnityWebRequest.EscapeURL(timeElapsedSeconds.ToString("F2", CultureInfo.InvariantCulture)) +
            "&errors=" + UnityWebRequest.EscapeURL(errorsThisPart.ToString());

        if (debugNetworkLogs)
            Debug.Log("[MinigameCompleteUI] Sending stats: " + url);

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
                Debug.LogWarning("[MinigameCompleteUI] Stats send failed: " + request.error);
            }
            else
            {
                if (debugNetworkLogs)
                    Debug.Log("[MinigameCompleteUI] Stats send success: " + request.downloadHandler.text);
            }
        }
    }
}