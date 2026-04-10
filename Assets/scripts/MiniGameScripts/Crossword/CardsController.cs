using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CardsController : MonoBehaviour
{
    [System.Serializable]
    public class SpritePair
    {
        public string matchId;
        public Sprite spriteA;
        public Sprite spriteB;

        [Header("Audio for text card only")]
        public AudioClip textSound;
    }

    [System.Serializable]
    public class Level
    {
        public List<SpritePair> pairs = new List<SpritePair>();
    }

    [Header("Levels")]
    [SerializeField] private List<Level> levels = new List<Level>();
    [SerializeField] private float revealTime = 0.4f;
    [SerializeField] private float nextLevelDelay = 1f;

    [Header("Coin Rewards")]
    [SerializeField] private int coinsForCorrectMatch = 5;
    [SerializeField] private int coinsForWrongMatch = -1;

    [Header("Minigame Complete Reward")]
    [Tooltip("How many coins to award when ALL levels in this minigame are completed.")]
    [SerializeField] private int rewardCoinsOnComplete = 20;

    [Tooltip("Assign your MinigameCompleteUI (canvas script) here.")]
    [SerializeField] private MinigameCompleteUI completeUI;

    [Header("Match Audio")]
    [SerializeField] private AudioClip correctMatchSound;
    [SerializeField] private AudioClip wrongMatchSound;

    [Header("Google Sheets Stats")]
    [Tooltip("Paste your deployed Google Apps Script web app URL here.")]
    [SerializeField] private string googleScriptUrl = "";

    [Tooltip("Name of this minigame shown in Google Sheets.")]
    [SerializeField] private string minigameName = "MemoryCards";

    [Tooltip("If true, tracking starts automatically in Start().")]
    [SerializeField] private bool autoStartTracking = true;

    private AudioSource audioSource;
    private int currentLevel = 0;

    [Header("Board")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;

    private List<CardData> cardsToSpawn;

    private Card firstSelected;
    private Card secondSelected;

    private bool canSelect = true;
    private int matchesFound = 0;

    // Stats tracking
    private bool trackingStarted = false;
    private bool statsSent = false;
    private DateTime startDateTime;
    private string sessionId;

    private struct CardData
    {
        public string id;
        public Sprite sprite;
        public AudioClip sound;

        public CardData(string id, Sprite sprite, AudioClip sound)
        {
            this.id = id;
            this.sprite = sprite;
            this.sound = sound;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        sessionId = Guid.NewGuid().ToString();

        if (autoStartTracking)
            BeginTrackingNow();

        StartLevel(0);
    }

    /// <summary>
    /// Call this manually from a tutorial "Start Game" button
    /// if you want the timer to begin only when the player actually starts playing.
    /// </summary>
    public void BeginTrackingNow()
    {
        if (trackingStarted)
            return;

        trackingStarted = true;
        startDateTime = DateTime.Now;

        Debug.Log("Minigame tracking started at: " + startDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private void StartLevel(int levelIndex)
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels set on CardsController. Add levels and pairs in the Inspector.", this);
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Level index {levelIndex} is out of range.", this);
            return;
        }

        currentLevel = levelIndex;
        matchesFound = 0;
        firstSelected = null;
        secondSelected = null;
        canSelect = true;

        ClearBoard();
        PrepareCardsForCurrentLevel();
        CreateCards();
    }

    private void PrepareCardsForCurrentLevel()
    {
        cardsToSpawn = new List<CardData>();

        var pairList = levels[currentLevel].pairs;

        for (int i = 0; i < pairList.Count; i++)
        {
            var p = pairList[i];

            if (p.spriteA == null || p.spriteB == null)
            {
                Debug.LogWarning($"Level {currentLevel} pair {i} ('{p.matchId}') is missing spriteA or spriteB. Skipping.", this);
                continue;
            }

            // spriteA = image -> no sound
            cardsToSpawn.Add(new CardData(p.matchId, p.spriteA, null));

            // spriteB = text -> has sound
            cardsToSpawn.Add(new CardData(p.matchId, p.spriteB, p.textSound));
        }

        Shuffle(cardsToSpawn);
    }

    private void CreateCards()
    {
        for (int i = 0; i < cardsToSpawn.Count; i++)
        {
            Card c = Instantiate(cardPrefab, gridTransform);
            c.controller = this;

            var d = cardsToSpawn[i];
            c.SetData(d.id, d.sprite, d.sound);

            c.Hide();
        }
    }

    public void SetSelected(Card card)
    {
        if (!canSelect) return;
        if (card == null) return;
        if (card.isSelected) return;

        card.Show();

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        secondSelected = card;
        canSelect = false;
        StartCoroutine(CheckMatching(firstSelected, secondSelected));
    }

    private IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(revealTime);

        if (a != null && b != null && a.matchId == b.matchId)
        {
            matchesFound++;

            CoinManager.EnsureExists().AddCoin(coinsForCorrectMatch);
            PlaySound(correctMatchSound);

            if (matchesFound >= GetExpectedMatchesThisLevel())
            {
                yield return new WaitForSeconds(nextLevelDelay);
                LoadNextLevel();
            }
        }
        else
        {
            CoinManager.EnsureExists().AddCoin(coinsForWrongMatch);
            PlaySound(wrongMatchSound);

            if (a != null) a.Hide();
            if (b != null) b.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        canSelect = true;
    }

    private int GetExpectedMatchesThisLevel()
    {
        int count = 0;
        var pairList = levels[currentLevel].pairs;

        for (int i = 0; i < pairList.Count; i++)
        {
            if (pairList[i].spriteA != null && pairList[i].spriteB != null)
                count++;
        }

        return count;
    }

    private void LoadNextLevel()
    {
        int next = currentLevel + 1;

        if (next >= levels.Count)
        {
            Debug.Log("All levels completed!");

            SendCompletionStats();

            if (completeUI != null)
            {
                canSelect = false;
                completeUI.Show(rewardCoinsOnComplete);
            }
            return;
        }

        StartLevel(next);
    }

    private void SendCompletionStats()
    {
        if (statsSent)
            return;

        statsSent = true;

        if (!trackingStarted)
        {
            Debug.LogWarning("Tracking was not started, so no stats were sent.");
            return;
        }

        if (string.IsNullOrWhiteSpace(googleScriptUrl))
        {
            Debug.LogWarning("Google Script URL is empty. Stats were not sent.");
            return;
        }

        double completionSeconds = (DateTime.Now - startDateTime).TotalSeconds;
        StartCoroutine(SendStatsToGoogleSheets(startDateTime, completionSeconds));
    }

    private IEnumerator SendStatsToGoogleSheets(DateTime startedAt, double completionSeconds)
    {
        string startTimeString = startedAt.ToString("yyyy-MM-dd HH:mm:ss");
        string completionString = Mathf.RoundToInt((float)completionSeconds).ToString();

        string url =
            googleScriptUrl +
            "?minigame=" + UnityWebRequest.EscapeURL(minigameName) +
            "&session_id=" + UnityWebRequest.EscapeURL(sessionId) +
            "&start_time=" + UnityWebRequest.EscapeURL(startTimeString) +
            "&completion_seconds=" + UnityWebRequest.EscapeURL(completionString);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("Failed to send minigame stats: " + request.error);
            }
            else
            {
                Debug.Log("Minigame stats sent successfully: " + request.downloadHandler.text);
            }
        }
    }

    private void ClearBoard()
    {
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridTransform.GetChild(i).gameObject);
        }
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}