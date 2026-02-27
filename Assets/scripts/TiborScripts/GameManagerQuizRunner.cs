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
    [Tooltip("Drag your Player > YearLabel > Text (TMP) here")]
    public TMP_Text playerYearText;

    [Header("Gate option reveal")]
    [Tooltip("If true: only current gate's option texts are shown. Next gate appears after correct answer.")]
    public bool revealNextGateOptions = true;

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

        // ✅ Hide/show option text above gates at start
        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        UpdateUI();
        UpdatePlayerYearLabel(); // set starting label
    }

    public bool CheckAnswer(int gateIndex, Side chosenSide)
    {
        if (debugLogs)
            Debug.Log($"[GameManager] CheckAnswer gateIndex={gateIndex} chosenSide={chosenSide} currentIndex={currentIndex}");

        // Must answer in order
        if (gateIndex != currentIndex)
        {
            if (debugLogs)
                Debug.Log($"[GameManager] ❌ Wrong order. Expected gateIndex={currentIndex}");
            return false;
        }

        Side correct = questions[currentIndex].correctSide;

        if (debugLogs)
            Debug.Log($"[GameManager] Correct side for gate {currentIndex} is {correct}");

        if (chosenSide == correct)
        {
            currentIndex++;

            UpdateUI();
            UpdatePlayerYearLabel(); // update to next year label

            // ✅ Reveal the next gate's options
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
            if (debugLogs) Debug.Log("[GameManager] PlayerYearText set to 'Finished!'");
            return;
        }

        string nextLabel = questions[currentIndex].dateLabel;
        playerYearText.text = nextLabel;

        if (debugLogs)
            Debug.Log($"[GameManager] PlayerYearText set to '{nextLabel}' (currentIndex={currentIndex})");
    }

    void ApplyGateOptionVisibility()
    {
        if (gates == null || gates.Length == 0) return;

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] == null) continue;

            // Show only current gate's options (the one we are solving right now)
            bool shouldShow = (i == currentIndex);

            // If finished, hide all
            if (currentIndex >= questions.Length) shouldShow = false;

            gates[i].SetOptionsVisible(shouldShow);
        }

        if (debugLogs)
            Debug.Log($"[GameManager] Gate options visibility updated. currentIndex={currentIndex}");
    }

    void FinishGame()
    {
        if (debugLogs) Debug.Log("[GameManager] 🎉 Finished all questions!");

        if (finishedPanel != null)
            finishedPanel.SetActive(true);

        // Hide all gate options when done
        if (revealNextGateOptions)
            ApplyGateOptionVisibility();

        var player = FindFirstObjectByType<PlayerRunnerT>();
        if (player != null)
            player.enabled = false;
    }
}