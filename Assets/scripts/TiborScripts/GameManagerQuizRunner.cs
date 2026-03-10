using UnityEngine;
using TMPro;

public class GameManagerQuizRunner : MonoBehaviour
{
    public enum Side { Left, Right }

    [System.Serializable]
    public class Question
    {
        public string dateLabel;
        [TextArea] public string leftText;
        [TextArea] public string rightText;
        public Side correctSide;
    }

    [Header("Data")]
    public Question[] questions;
    public GateChoice[] gates;

    [Header("UI")]
    public TMP_Text progressText;
    public GameObject finishedPanel;

    [Header("Year label above player")]
    public TMP_Text playerYearText;

    [Header("Gate text visibility")]
    public bool revealNextGateOptions = true;

    [Header("Coin Rewards")]
    public int correctGateReward = 5;
    public int wrongGatePenalty = 5;
    public int finishReward = 20;

    [Header("Finish Return")]
    public bool returnToPreviousSceneOnFinish = true;
    public bool useSubwayTReturnScene = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private int currentIndex = 0;
    private bool isFinished = false;

    void Start()
    {
        if (debugLogs)
            Debug.Log($"[GameManager] Start. questions={questions?.Length}, gates={gates?.Length}");

        if (finishedPanel != null)
            finishedPanel.SetActive(false);

        SetupAllGates();

        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        UpdateUI();
        UpdatePlayerYearLabel();
    }

    void SetupAllGates()
    {
        int count = Mathf.Min(questions.Length, gates.Length);

        for (int i = 0; i < count; i++)
        {
            if (gates[i] == null)
            {
                Debug.LogWarning($"[GameManager] gates[{i}] is NULL.");
                continue;
            }

            gates[i].Setup(this, i);
            gates[i].ResetSolvedState();
        }
    }

    public bool CheckAnswer(int gateIndex, Side chosenSide)
    {
        if (isFinished)
            return false;

        if (debugLogs)
            Debug.Log($"[GameManager] CheckAnswer gateIndex={gateIndex}, chosenSide={chosenSide}, currentIndex={currentIndex}");

        if (gateIndex != currentIndex)
        {
            if (debugLogs)
                Debug.Log($"[GameManager] Wrong order. Expected gate {currentIndex}, got {gateIndex}");

            LoseCoins();
            return false;
        }

        Side correct = questions[currentIndex].correctSide;

        if (chosenSide == correct)
        {
            GainCoins(correctGateReward);

            currentIndex++;

            UpdateUI();
            UpdatePlayerYearLabel();

            if (revealNextGateOptions)
                ApplyGateOptionVisibility();

            if (currentIndex >= questions.Length)
                FinishGame();

            return true;
        }

        LoseCoins();
        return false;
    }

    void GainCoins(int amount)
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoin(amount);

        if (CoinTextFeedback.Instance != null)
            CoinTextFeedback.Instance.FlashForChange(amount);

        if (debugLogs)
            Debug.Log($"[GameManager] +{amount} coins");
    }

    void LoseCoins()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoin(-wrongGatePenalty);

        if (CoinTextFeedback.Instance != null)
            CoinTextFeedback.Instance.FlashForChange(-wrongGatePenalty);

        if (debugLogs)
            Debug.Log($"[GameManager] -{wrongGatePenalty} coins");
    }

    public void ResetRun(PlayerRunnerT player)
    {
        if (debugLogs)
            Debug.Log("[GameManager] ResetRun called.");

        isFinished = false;
        currentIndex = 0;

        if (finishedPanel != null)
            finishedPanel.SetActive(false);

        if (gates != null)
        {
            for (int i = 0; i < gates.Length; i++)
            {
                if (gates[i] == null) continue;
                gates[i].ResetSolvedState();
            }
        }

        UpdateUI();
        UpdatePlayerYearLabel();

        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        if (player != null)
            player.RespawnAtStart();
    }

    void FinishGame()
    {
        if (isFinished) return;

        isFinished = true;

        if (debugLogs)
            Debug.Log("[GameManager] Finished all questions.");

        GainCoins(finishReward);

        if (finishedPanel != null)
            finishedPanel.SetActive(true);

        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        if (useSubwayTReturnScene)
        {
            SubwayTReturnToScene13T.ReturnNow();
            return;
        }

        if (returnToPreviousSceneOnFinish && ReturnToPreviousSceneT.HasReturnPoint())
        {
            ReturnToPreviousSceneT.ReturnNow();
            return;
        }

        PlayerRunnerT player = FindFirstObjectByType<PlayerRunnerT>();
        if (player != null)
            player.enabled = false;
    }

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Correct: {currentIndex}/{questions.Length}";
    }

    void UpdatePlayerYearLabel()
    {
        if (playerYearText == null) return;
        if (questions == null || questions.Length == 0) return;

        if (currentIndex >= questions.Length)
        {
            playerYearText.text = "Finished!";
            return;
        }

        playerYearText.text = questions[currentIndex].dateLabel;
    }

    void ApplyGateOptionVisibility()
    {
        if (gates == null || gates.Length == 0) return;

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] == null) continue;

            bool shouldShow = (i == currentIndex);

            if (currentIndex >= questions.Length)
                shouldShow = false;

            gates[i].SetOptionsVisible(shouldShow);
        }
    }

    public GateChoice GetCurrentGate()
    {
        if (isFinished) return null;
        if (gates == null) return null;
        if (currentIndex < 0 || currentIndex >= gates.Length) return null;

        return gates[currentIndex];
    }

    public bool IsFinished()
    {
        return isFinished;
    }
}