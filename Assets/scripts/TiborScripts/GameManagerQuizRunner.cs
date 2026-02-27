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

    [Header("UI (optional)")]
    public TMP_Text progressText;
    public GameObject finishedPanel;

    [Header("Year label above player (TMP)")]
    public TMP_Text playerYearText;

    [Header("Gate option reveal")]
    public bool revealNextGateOptions = true;

    [Header("On Finish")]
    public bool returnToPreviousSceneOnFinish = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private int currentIndex = 0;

    void Start()
    {
        if (debugLogs)
            Debug.Log($"[GameManager] Start. questions={questions?.Length}, gates={gates?.Length}");

        if (finishedPanel != null)
            finishedPanel.SetActive(false);

        int count = Mathf.Min(questions.Length, gates.Length);
        for (int i = 0; i < count; i++)
        {
            if (gates[i] == null)
            {
                Debug.Log($"[GameManager] WARNING: gates[{i}] is NULL (not assigned).");
                continue;
            }
            gates[i].Setup(this, i);
        }

        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        UpdateUI();
        UpdatePlayerYearLabel();
    }

    public bool CheckAnswer(int gateIndex, Side chosenSide)
    {
        if (debugLogs)
            Debug.Log($"[GameManager] CheckAnswer gateIndex={gateIndex} chosenSide={chosenSide} currentIndex={currentIndex}");

        if (gateIndex != currentIndex)
        {
            if (debugLogs)
                Debug.Log($"[GameManager] ❌ Wrong order. Expected gateIndex={currentIndex}");
            return false;
        }

        Side correct = questions[currentIndex].correctSide;

        if (chosenSide == correct)
        {
            currentIndex++;

            UpdateUI();
            UpdatePlayerYearLabel();

            if (revealNextGateOptions)
                ApplyGateOptionVisibility();

            if (debugLogs)
                Debug.Log($"[GameManager] ✅ Correct! Progress now {currentIndex}/{questions.Length}");

            if (currentIndex >= questions.Length)
                FinishGame();

            return true;
        }

        if (debugLogs)
            Debug.Log("[GameManager] ❌ Wrong side!");
        return false;
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
            if (currentIndex >= questions.Length) shouldShow = false;

            gates[i].SetOptionsVisible(shouldShow);
        }
    }

    void FinishGame()
    {
        if (debugLogs) Debug.Log("[GameManager] 🎉 Finished all questions!");

        if (finishedPanel != null)
            finishedPanel.SetActive(true);

        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        // ✅ Return to the scene where you interacted with cube (and restore position)
        if (returnToPreviousSceneOnFinish && ReturnToPreviousSceneT.HasReturnPoint())
        {
            ReturnToPreviousSceneT.ReturnNow();
            return;
        }

        // fallback (original idea): stop player movement
        var player = FindFirstObjectByType<PlayerRunnerT>();
        if (player != null)
            player.enabled = false;
    }
}