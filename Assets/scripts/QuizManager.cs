using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public QuizData quizData;
    public GameObject letterFieldPrefab;
    public GameObject letterButtonPrefab;

    public Transform letterFieldParent;
    public Transform letterButtonsParent;

    private Text[] letterFields;
    private Button[] letterButtons;

    private int currentFieldIndex = 0;
    private int currentQuizIndex = 0;

    public Text quizText;

    void Start()
    {
       LoadQuiz(currentQuizIndex);
    }

    void LoadQuiz(int quizIndex)
    {
        ResetGame();
        Debug.Log("Loading Quiz: " + quizIndex);

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
    currentFieldIndex = 0;

    QuizData.Quiz quiz = quizData.quizzes[quizIndex];

    quizText.text = quiz.text;

    CreateLetterFields(quiz.correctWord.Length);
    CreateLetterButtons(quiz.correctWord);
    }

    void CreateLetterFields(int fieldCount)
    {
        Debug.Log("Creating Letter Fields: " + fieldCount);

    foreach(Transform child in letterFieldParent)
    {
        Destroy(child.gameObject);
    }

    letterFields = new Text[fieldCount];

    for(int i = 0; i < fieldCount; i++)
    {
        GameObject field = Instantiate(letterFieldPrefab, letterFieldParent);
        letterFields[i] = field.GetComponentInChildren<Text>();
        if(letterFields[i] == null)
        {
            Debug.LogError("Letter Field Prefab is missing a text component");
        }
    }
    }

    void CreateLetterButtons(string correctWord)
    {
    Debug.Log("Creating Letter Buttons");

    foreach (Transform child in letterButtonsParent)
        Destroy(child.gameObject);

    char[] correctLetters = correctWord.ToUpper().ToCharArray();

    int totalLetters = 16;

    if (correctLetters.Length > totalLetters)
    {
        Debug.LogError("Correct word is longer than 16 letters: " + correctWord);
        totalLetters = correctLetters.Length;
    }

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
        buttonObj.GetComponentInChildren<Text>().text = allLetters[i].ToString();

        int index = i;
        Button btn = buttonObj.GetComponent<Button>();
        btn.onClick.AddListener(() => OnLetterButtonClick(index));

        letterButtons[i] = btn;
    }
    }

    void OnLetterButtonClick(int buttonIndex)
    {
    if(currentFieldIndex < letterFields.Length)
        {
            string letter = letterButtons[buttonIndex].GetComponentInChildren<Text>().text;

            letterFields[currentFieldIndex].text = letter;

            letterButtons[buttonIndex].interactable = false;

            currentFieldIndex++;
        }
    }

    char[] GenerateRandomLetters(int count, char[] excludeLetters)
    {
    char[] randomLetters = new char[count];
    for(int i = 0; i < count; i++)
    {
        char randomLetter;

        do
        {
            randomLetter = (char)('A' + Random.Range(0, 26));
        }

        while(System.Array.Exists(excludeLetters, c => c == randomLetter));
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

    public void CheckReply()
    {
    string playerReply = "";

    foreach (Text field in letterFields)
    {
        if (field == null)
        {
            Debug.LogError("Letter field is null");
            return;
        }

        playerReply += field.text;
    }

    string correctWord = quizData.quizzes[currentQuizIndex].correctWord.ToUpper();

    if (playerReply.ToUpper() == correctWord)
    {
        Debug.Log("Correct!");

        foreach (Text field in letterFields)
        {
            field.color = Color.green;
        }

        Invoke("NextQuiz", 2f);   // waits 2 seconds
    }
    else
    {
        Debug.Log("Incorrect!");

        foreach (Text field in letterFields)
        {
            field.color = Color.red;
        }
    }
    }

    void NextQuiz()
    {
    Debug.Log("Loading Next Quiz");

    currentQuizIndex++;

    if (currentQuizIndex < quizData.quizzes.Length)
    {
        LoadQuiz(currentQuizIndex);
    }
    else
    {
        Debug.Log("Game Over, All Quizzes completed");

        // Optional: restart from beginning
        currentQuizIndex = 0;
        LoadQuiz(currentQuizIndex);
    }
    }

    public void DeleteLastLetter()
    {
    if (currentFieldIndex > 0)
    {
        currentFieldIndex--;

        string deletedLetter = letterFields[currentFieldIndex].text;

        foreach (Button button in letterButtons)
        {
            if (button.GetComponentInChildren<Text>().text == deletedLetter 
                && !button.interactable)
            {
                button.interactable = true;
                break;
            }
        }

        letterFields[currentFieldIndex].text = "";
    }
    }

    void ResetGame()
    {
    Debug.Log("Resetting Game");

    if (letterFields != null)
    {
        foreach (Text field in letterFields)
        {
            if (field != null)
            {
                field.text = "";
                field.color = Color.white;
            }
        }
    }
    }
}