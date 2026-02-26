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

        UpdateUI();
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

    void FinishGame()
    {
        if (debugLogs) Debug.Log("[GameManager] 🎉 Finished all questions!");

        if (finishedPanel != null)
            finishedPanel.SetActive(true);

        var player = FindFirstObjectByType<PlayerRunnerT>();
        if (player != null)
            player.enabled = false;
    }
}