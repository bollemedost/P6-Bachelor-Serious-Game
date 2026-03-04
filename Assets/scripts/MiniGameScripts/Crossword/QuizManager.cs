using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public QuizData quizData;

    [Header("UI")]
    public Text quizText; // assign Canvas/Quiz Text

    [Header("Grid")]
    public Transform crosswordGridParent; // assign Canvas/CrosswordGrid

    [Header("Letter Buttons")]
    public GameObject letterButtonPrefab; // Button (Legacy) prefab
    public Transform letterButtonsParent; // parent that holds the 16 letter buttons

    private Button[] gridCells;
    private Text[] gridCellTexts;

    private Button[] letterButtons;

    private int currentQuizIndex = 0;
    private int activeCellIndex = -1;

    void Start()
    {
        CacheGridCells();
        LoadQuiz(currentQuizIndex);
    }

    void CacheGridCells()
    {
        if (crosswordGridParent == null)
        {
            Debug.LogError("crosswordGridParent is not assigned!");
            return;
        }

        int cellCount = crosswordGridParent.childCount;
        gridCells = new Button[cellCount];
        gridCellTexts = new Text[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            Transform cell = crosswordGridParent.GetChild(i);

            Button cellBtn = cell.GetComponent<Button>();
            Text cellText = cell.GetComponentInChildren<Text>();

            if (cellBtn == null)
                Debug.LogError($"Cell {cell.name} is missing a Button component!");
            if (cellText == null)
                Debug.LogError($"Cell {cell.name} is missing a Text (Legacy) component!");

            gridCells[i] = cellBtn;
            gridCellTexts[i] = cellText;

            int index = i;
            cellBtn.onClick.RemoveAllListeners();
            cellBtn.onClick.AddListener(() => OnCellClicked(index));
        }
    }

    void OnCellClicked(int index)
    {
        activeCellIndex = index;
        Debug.Log("Active cell = " + index);

        // Optional visual: highlight active cell
        for (int i = 0; i < gridCells.Length; i++)
        {
            if (gridCells[i] == null) continue;
            ColorBlock cb = gridCells[i].colors;
            cb.normalColor = (i == activeCellIndex) ? new Color(1f, 1f, 0.6f) : Color.white;
            gridCells[i].colors = cb;
        }
    }

    void LoadQuiz(int quizIndex)
    {
        if (quizData == null || quizData.quizzes == null || quizData.quizzes.Length == 0)
        {
            Debug.LogError("Quiz Data or quizzes is not set up correctly!");
            return;
        }

        if (quizIndex < 0 || quizIndex >= quizData.quizzes.Length)
        {
            Debug.LogError("Invalid quiz index: " + quizIndex);
            return;
        }

        if (quizText == null)
        {
            Debug.LogError("Quiz Text reference is missing (assign it in the Inspector).");
            return;
        }

        currentQuizIndex = quizIndex;
        activeCellIndex = -1;

        // Clear grid letters
        for (int i = 0; i < gridCellTexts.Length; i++)
        {
            if (gridCellTexts[i] != null)
            {
                gridCellTexts[i].text = "";
                gridCellTexts[i].color = Color.black;
            }
        }

        var quiz = quizData.quizzes[quizIndex];
        quizText.text = quiz.text;

        CreateLetterButtons(quiz.correctWord);
    }

    void CreateLetterButtons(string correctWord)
    {
        foreach (Transform child in letterButtonsParent)
            Destroy(child.gameObject);

        char[] correctLetters = correctWord.ToUpper().ToCharArray();

        int totalLetters = 16;
        if (correctLetters.Length > totalLetters)
            totalLetters = correctLetters.Length;

        letterButtons = new Button[totalLetters];

        int wrongLetterCount = totalLetters - correctLetters.Length;
        char[] wrongLetters = GenerateRandomLetters(wrongLetterCount, correctLetters);

        char[] allLetters = new char[totalLetters];
        correctLetters.CopyTo(allLetters, 0);
        wrongLetters.CopyTo(allLetters, correctLetters.Length);

        ShuffleLetters(allLetters);

        for (int i = 0; i < allLetters.Length; i++)
        {
            GameObject buttonObj = Instantiate(letterButtonPrefab, letterButtonsParent);

            Text btnText = buttonObj.GetComponentInChildren<Text>();
            if (btnText == null)
                Debug.LogError("Letter Button Prefab is missing a Text (Legacy) component!");
            else
                btnText.text = allLetters[i].ToString();

            Button btn = buttonObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("Letter Button Prefab is missing a Button component!");
                continue;
            }

            int index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnLetterButtonClick(index));

            letterButtons[i] = btn;
        }
    }

    void OnLetterButtonClick(int buttonIndex)
    {
        if (activeCellIndex < 0)
        {
            Debug.Log("Click a grid cell first.");
            return;
        }

        Text btnText = letterButtons[buttonIndex].GetComponentInChildren<Text>();
        if (btnText == null) return;

        // Write letter into active cell
        gridCellTexts[activeCellIndex].text = btnText.text;

        // Optional: disable letter button so it can’t be reused
        letterButtons[buttonIndex].interactable = false;

        // Optional: auto-move to next cell
        int next = activeCellIndex + 1;
        if (next < gridCellTexts.Length)
            activeCellIndex = next;
    }

    public void DeleteActiveCellLetter()
    {
        if (activeCellIndex < 0) return;
        gridCellTexts[activeCellIndex].text = "";
    }

    char[] GenerateRandomLetters(int count, char[] excludeLetters)
    {
        char[] randomLetters = new char[count];

        for (int i = 0; i < count; i++)
        {
            char randomLetter;
            do
            {
                randomLetter = (char)('A' + Random.Range(0, 26));
            }
            while (System.Array.Exists(excludeLetters, c => c == randomLetter));

            randomLetters[i] = randomLetter;
        }

        return randomLetters;
    }

    void ShuffleLetters(char[] letters)
    {
        for (int i = 0; i < letters.Length; i++)
        {
            int randomIndex = Random.Range(i, letters.Length);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }
    }
}