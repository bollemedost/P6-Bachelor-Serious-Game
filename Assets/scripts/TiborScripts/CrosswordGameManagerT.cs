using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrosswordGameManagerT : MonoBehaviour
{
    public static CrosswordGameManagerT Instance;

    [System.Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public string letter;
        public string clueNumber;
    }

    [Header("References")]
    public RectTransform boardPanel;
    public RectTransform letterBankPanel;
    public GameObject crosswordCellPrefab;
    public GameObject letterButtonPrefab;

    [Header("Layout")]
    public float gridStep = 50f;
    public float visibleCellSize = 44f;
    public int totalColumns = 12;
    public int totalRows = 17;

    private Dictionary<Vector2Int, CrosswordSlotT> slotMap = new Dictionary<Vector2Int, CrosswordSlotT>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildBoardExact();
        SpawnAlphabet();
    }

    void BuildBoardExact()
    {
        slotMap.Clear();

        for (int i = boardPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(boardPanel.GetChild(i).gameObject);
        }

        List<CellData> cells = GetExactPuzzleLayout();

        float boardWidth = totalColumns * gridStep;
        float boardHeight = totalRows * gridStep;

        foreach (CellData data in cells)
        {
            GameObject newCell = Instantiate(crosswordCellPrefab, boardPanel);
            newCell.name = $"Cell_{data.x}_{data.y}_{data.letter}";

            RectTransform rt = newCell.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(visibleCellSize, visibleCellSize);

            float posX = (data.x * gridStep) - (boardWidth / 2f) + (gridStep / 2f);
            float posY = -(data.y * gridStep) + (boardHeight / 2f) - (gridStep / 2f);

            rt.anchoredPosition = new Vector2(posX, posY);

            CrosswordSlotT slot = newCell.GetComponent<CrosswordSlotT>();

            TMP_Text letterText = newCell.transform.Find("LetterText").GetComponent<TMP_Text>();
            TMP_Text clueNumberText = newCell.transform.Find("ClueNumberText").GetComponent<TMP_Text>();

            slot.letterText = letterText;
            slot.clueNumberText = clueNumberText;
            slot.Setup(data.letter, data.clueNumber);

            slotMap[new Vector2Int(data.x, data.y)] = slot;
        }
    }

    List<CellData> GetExactPuzzleLayout()
    {
        List<CellData> c = new List<CellData>();

        // 1
        AddWordVertical(c, 7, 0, "NIELSINE", "1");

        // 2
        AddWordVertical(c, 4, 2, "FEM", "2");

        // 3
        AddWordHorizontal(c, 0, 3, "FOLKEHOLD", "3");

        // left vertical
        AddWordVertical(c, 0, 3, "FEMTEN", "");

        // 4
        AddWordHorizontal(c, -1, 7, "FEMOGTYVE", "4");

        // 5
        AddWordVertical(c, 3, 7, "GRUNDLOVEN", "5");

        // 6
        AddWordHorizontal(c, 0, 11, "TREDIVE", "6");

        // 7
        AddWordHorizontal(c, 0, 15, "FRUENTIMMER", "7");

        return RemoveDuplicatesKeepFirst(c);
    }

    void AddWordHorizontal(List<CellData> cells, int startX, int startY, string word, string clueNumber)
    {
        for (int i = 0; i < word.Length; i++)
        {
            cells.Add(new CellData
            {
                x = startX + i,
                y = startY,
                letter = word[i].ToString(),
                clueNumber = (i == 0) ? clueNumber : ""
            });
        }
    }

    void AddWordVertical(List<CellData> cells, int startX, int startY, string word, string clueNumber)
    {
        for (int i = 0; i < word.Length; i++)
        {
            cells.Add(new CellData
            {
                x = startX,
                y = startY + i,
                letter = word[i].ToString(),
                clueNumber = (i == 0) ? clueNumber : ""
            });
        }
    }

    List<CellData> RemoveDuplicatesKeepFirst(List<CellData> input)
    {
        Dictionary<Vector2Int, CellData> unique = new Dictionary<Vector2Int, CellData>();

        foreach (CellData cell in input)
        {
            Vector2Int key = new Vector2Int(cell.x, cell.y);

            if (!unique.ContainsKey(key))
                unique[key] = cell;
        }

        return new List<CellData>(unique.Values);
    }

    void SpawnAlphabet()
    {
        for (int i = letterBankPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(letterBankPanel.GetChild(i).gameObject);
        }

        string danishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ���";

        foreach (char ch in danishAlphabet)
        {
            GameObject btn = Instantiate(letterButtonPrefab, letterBankPanel);
            btn.name = "Letter_" + ch;

            DraggableLetterT drag = btn.GetComponent<DraggableLetterT>();
            if (drag != null)
                drag.Setup(ch.ToString());
        }
    }

    public void CheckWin()
    {
        foreach (var slot in slotMap.Values)
        {
            if (!slot.isFilledCorrectly)
                return;
        }

        Debug.Log("CROSSWORD COMPLETED!");
    }
}