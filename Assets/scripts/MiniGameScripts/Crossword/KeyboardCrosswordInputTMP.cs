using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardCrosswordInputTMP : MonoBehaviour
{
    [Header("Grid")]
    public Transform gridParent;   // Drag CrosswordGrid here
    public int columns = 10;       // Set grid width (e.g. 10)

    private Button[] cellButtons;
    private TMP_Text[] cellLetters;

    private int activeCell = -1;

    void Start()
    {
        if (gridParent == null)
        {
            Debug.LogError("gridParent is not assigned!");
            return;
        }

        int count = gridParent.childCount;
        cellButtons = new Button[count];
        cellLetters = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            Transform cell = gridParent.GetChild(i);

            // Button on the cell root
            cellButtons[i] = cell.GetComponent<Button>();
            if (cellButtons[i] == null)
            {
                Debug.LogError($"Cell {cell.name} is missing a Button component.");
                continue;
            }

            // TMP text specifically on child named "Letter"
            Transform letterChild = cell.Find("Letter");
            if (letterChild == null)
            {
                Debug.LogError($"Cell {cell.name} is missing a child named 'Letter'.");
                continue;
            }

            cellLetters[i] = letterChild.GetComponent<TMP_Text>();
            if (cellLetters[i] == null)
            {
                Debug.LogError($"'Letter' on {cell.name} is missing TMP_Text.");
                continue;
            }

            int index = i;
            cellButtons[i].onClick.RemoveAllListeners();
            cellButtons[i].onClick.AddListener(() => SelectCell(index));
        }
    }

    void Update()
    {
        if (activeCell == -1) return;

        foreach (char c in Input.inputString)
        {
            // Backspace
            if (c == '\b')
            {
                DeleteLetter();
                continue;
            }

            // Letters A-Z
            if (char.IsLetter(c))
            {
                cellLetters[activeCell].text = char.ToUpper(c).ToString();
                MoveNext();
            }
        }
    }

    void SelectCell(int index)
    {
        if (index < 0 || index >= cellLetters.Length) return;
        if (cellLetters[index] == null) return;

        activeCell = index;
        Debug.Log("Selected cell: " + activeCell);

        HighlightActiveCell();
    }

    void MoveNext()
    {
        int next = activeCell + 1;

        // stay in same row
        if (next < cellLetters.Length && (next / columns) == (activeCell / columns))
        {
            // skip cells that have no Letter assigned (if any)
            while (next < cellLetters.Length && (next / columns) == (activeCell / columns) && cellLetters[next] == null)
                next++;

            if (next < cellLetters.Length && (next / columns) == (activeCell / columns))
            {
                activeCell = next;
                HighlightActiveCell();
            }
        }
    }

    void DeleteLetter()
    {
        if (cellLetters[activeCell] == null) return;

        // If current has text, delete it
        if (!string.IsNullOrEmpty(cellLetters[activeCell].text))
        {
            cellLetters[activeCell].text = "";
            return;
        }

        // else go back one and delete there
        int prev = activeCell - 1;
        if (prev >= 0 && (prev / columns) == (activeCell / columns))
        {
            while (prev >= 0 && (prev / columns) == (activeCell / columns) && cellLetters[prev] == null)
                prev--;

            if (prev >= 0 && (prev / columns) == (activeCell / columns))
            {
                activeCell = prev;
                cellLetters[activeCell].text = "";
                HighlightActiveCell();
            }
        }
    }

    void HighlightActiveCell()
    {
        // Optional: highlight using button color tint
        for (int i = 0; i < cellButtons.Length; i++)
        {
            if (cellButtons[i] == null) continue;

            ColorBlock cb = cellButtons[i].colors;
            cb.normalColor = (i == activeCell) ? new Color(1f, 1f, 0.6f) : Color.white;
            cellButtons[i].colors = cb;
        }
    }
}