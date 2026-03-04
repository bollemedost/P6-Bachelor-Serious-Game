using UnityEngine;
using UnityEngine.UI;

public class CrosswordGridManager : MonoBehaviour
{
    public int width = 13;   // felter pr række
    public int height = 17;  // rækker

    public GameObject cellPrefab;
    public Transform gridParent;

    public QuizData quizData; // <-- træk din QuizData asset ind her i Inspector

    private Text[,] gridLetters;
    private Image[,] gridCells;

    private char?[,] solution;

    private int selectedX = -1;
    private int selectedY = -1;

    void Start()
    {
        CreateGrid();

        if (quizData == null)
        {
            Debug.LogError("QuizData is NOT assigned on CrosswordGridManager!");
            return;
        }

        foreach (var q in quizData.quizzes)
        {
            PlaceWord(q.correctWord, q.startX, q.startY, q.isAcross);
        }
    }

    void CreateGrid()
    {
        if (gridParent == null || cellPrefab == null)
        {
            Debug.LogError("gridParent or cellPrefab is missing!");
            return;
        }

        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        gridLetters = new Text[width, height];
        gridCells = new Image[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridParent);

                Image cellImage = cellObj.GetComponent<Image>();
                Text letterText = cellObj.GetComponentInChildren<Text>();

                cellImage.color = Color.black;
                letterText.text = "";
                letterText.color = Color.black;

                gridCells[x, y] = cellImage;
                gridLetters[x, y] = letterText;

                var cellClick = cellObj.GetComponent<CrosswordCell>();
                if (cellClick != null)
                {
                    cellClick.Setup(this, x, y);
                }
            }
        }

        solution = new char?[width, height];
        Debug.Log($"Grid created: {width} x {height} = {width * height} cells");
    }

    public void PlaceWord(string word, int startX, int startY, bool isAcross)
    {
        if (string.IsNullOrEmpty(word)) return;

        word = word.ToUpper();

        Debug.Log($"Placing '{word}' at ({startX},{startY}) across={isAcross}");

        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + (isAcross ? i : 0);
            int y = startY + (isAcross ? 0 : i);

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Debug.LogWarning($"Word '{word}' goes outside grid at ({x},{y}) in {width}x{height}");
                return;
            }

            // make this cell active/white
            gridCells[x, y].color = Color.white;

            solution[x, y] = word[i];
            gridLetters[x, y].text = "";
        }
    }

    public void SelectCell(int x, int y)
    {
        // Kun tillad klik på “aktive” (hvide) felter:
        if (solution != null && solution[x, y] == null)
            return;

        selectedX = x;
        selectedY = y;

        Debug.Log($"Selected cell: {x},{y}");
    }

    void Update()
    {
        if (selectedX < 0 || selectedY < 0)
            return;

        // Backspace sletter
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            gridLetters[selectedX, selectedY].text = "";
            return;
        }

        // A-Z skriver bogstav
        for (KeyCode k = KeyCode.A; k <= KeyCode.Z; k++)
        {
            if (Input.GetKeyDown(k))
            {
                char letter = k.ToString()[0];
                gridLetters[selectedX, selectedY].text = letter.ToString();
                return;
            }
        }
    }
}