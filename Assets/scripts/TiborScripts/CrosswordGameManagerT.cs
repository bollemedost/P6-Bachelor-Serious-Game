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

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource pickUpSource;

    public AudioClip pickUpLetterSound;
    public AudioClip correctPlaceLetterSound;
    public AudioClip wrongPlaceLetterSound;

    [Range(0.5f, 2f)]
    public float pickUpPitch = 1.3f;

    [Header("Completion UI")]
    public MinigameCompleteUI completeUI;
    public int completionCoins = 20;

    [Header("Hint System")]
    public bool hintsPickRandomUnfilledSlot = true;
    public int hintCost = 1;

    [Header("Layout")]
    public float gridStep = 50f;
    public float visibleCellSize = 44f;
    public int totalColumns = 12;
    public int totalRows = 17;

    private Dictionary<Vector2Int, CrosswordSlotT> slotMap = new Dictionary<Vector2Int, CrosswordSlotT>();
    private Dictionary<Vector2Int, string> answerMap = new Dictionary<Vector2Int, string>();
    private bool hasCompleted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildBoardExact();
        PreFillLetters();
        SpawnAlphabet();

        if (completeUI == null)
            completeUI = FindObjectOfType<MinigameCompleteUI>();

        if (completeUI != null && completeUI.root != null)
            completeUI.root.SetActive(false);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (pickUpSource == null)
            pickUpSource = sfxSource;
    }

    public void PlayPickUpSound()
    {
        if (pickUpSource != null && pickUpLetterSound != null)
        {
            pickUpSource.pitch = pickUpPitch;
            pickUpSource.PlayOneShot(pickUpLetterSound);
        }
    }

    public void PlayCorrectPlaceSound()
    {
        if (sfxSource != null && correctPlaceLetterSound != null)
            sfxSource.PlayOneShot(correctPlaceLetterSound);
    }

    public void PlayWrongPlaceSound()
    {
        if (sfxSource != null && wrongPlaceLetterSound != null)
            sfxSource.PlayOneShot(wrongPlaceLetterSound);
    }

    void BuildBoardExact()
    {
        slotMap.Clear();
        answerMap.Clear();

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

            RectTransform letterRT = letterText.GetComponent<RectTransform>();
            letterRT.anchorMin = Vector2.zero;
            letterRT.anchorMax = Vector2.one;
            letterRT.pivot = new Vector2(0.5f, 0.5f);
            letterRT.anchoredPosition = Vector2.zero;
            letterRT.offsetMin = Vector2.zero;
            letterRT.offsetMax = Vector2.zero;

            letterText.alignment = TextAlignmentOptions.Center;
            letterText.verticalAlignment = VerticalAlignmentOptions.Middle;

            if (clueNumberText != null)
            {
                clueNumberText.text = "";
                clueNumberText.gameObject.SetActive(false);
            }

            slot.letterText = letterText;
            slot.clueNumberText = clueNumberText;
            slot.Setup(data.letter, "");

            Vector2Int key = new Vector2Int(data.x, data.y);
            slotMap[key] = slot;
            answerMap[key] = data.letter;
        }
    }

    void PreFillLetters()
    {
        foreach (var pair in slotMap)
        {
            Vector2Int pos = pair.Key;
            CrosswordSlotT slot = pair.Value;
            string correctLetter = answerMap[pos];

            int x = pos.x;
            int y = pos.y;

            if (x == 7 && (y == 0 || y == 3 || y == 7))
                slot.TryPlaceLetter(correctLetter);

            if (x == 4 && y == 3)
                slot.TryPlaceLetter(correctLetter);

            if (y == 3 && (x == 0 || x == 8))
                slot.TryPlaceLetter(correctLetter);

            if (y == 7 && (x == 2 || x == 3))
                slot.TryPlaceLetter(correctLetter);

            if (x == 3 && (y == 11 || y == 13))
                slot.TryPlaceLetter(correctLetter);

            if (y == 11 && (x == 1 || x == 4 || x == 5))
                slot.TryPlaceLetter(correctLetter);

            if (y == 15 && (x == 2 || x == 7 || x == 8))
                slot.TryPlaceLetter(correctLetter);
        }
    }

    List<CellData> GetExactPuzzleLayout()
    {
        List<CellData> c = new List<CellData>();

        AddWordVertical(c, 7, 0, "NIELSINE", "1");
        AddWordVertical(c, 4, 2, "FEM", "2");
        AddWordHorizontal(c, 0, 3, "FOLKEHOLD", "3");
        AddWordHorizontal(c, -1, 7, "FEMOGTYVE", "4");
        AddWordVertical(c, 3, 7, "GRUNDLOVEN", "5");
        AddWordHorizontal(c, 0, 11, "TREDIVE", "6");
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
            {
                unique[key] = cell;
            }
            else
            {
                if (unique[key].letter != cell.letter)
                {
                    Debug.LogWarning(
                        $"Crossword conflict at position {key}. " +
                        $"Existing letter: {unique[key].letter}, New letter: {cell.letter}"
                    );
                }
            }
        }

        return new List<CellData>(unique.Values);
    }

    void SpawnAlphabet()
    {
        for (int i = letterBankPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(letterBankPanel.GetChild(i).gameObject);
        }

        string danishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ";

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
        if (hasCompleted)
            return;

        foreach (var slot in slotMap.Values)
        {
            if (!slot.isFilledCorrectly)
                return;
        }

        hasCompleted = true;
        Debug.Log("CROSSWORD COMPLETED!");

        if (completeUI != null)
        {
            completeUI.Show(completionCoins);
        }
        else
        {
            Debug.LogWarning("Crossword completed, but no MinigameCompleteUI is assigned.");
        }
    }

    public void OnHintButtonPressed()
    {
        if (hasCompleted)
            return;

        bool placed = PlaceOneHintLetter();

        if (placed)
        {
            CoinManager.EnsureExists().AddCoin(-hintCost);

            if (CoinTextFeedback.Instance != null)
                CoinTextFeedback.Instance.FlashForChange(-hintCost);
        }
        else
        {
            Debug.Log("No more hint letters to place.");
        }
    }

    bool PlaceOneHintLetter()
    {
        List<Vector2Int> availableHints = new List<Vector2Int>();

        foreach (var pair in slotMap)
        {
            CrosswordSlotT slot = pair.Value;

            if (slot != null && !slot.isFilledCorrectly)
            {
                availableHints.Add(pair.Key);
            }
        }

        if (availableHints.Count == 0)
        {
            CheckWin();
            return false;
        }

        Vector2Int chosenKey;

        if (hintsPickRandomUnfilledSlot)
        {
            int randomIndex = Random.Range(0, availableHints.Count);
            chosenKey = availableHints[randomIndex];
        }
        else
        {
            chosenKey = availableHints[0];
        }

        CrosswordSlotT chosenSlot = slotMap[chosenKey];
        string correctLetter = answerMap[chosenKey];

        bool placedCorrectly = chosenSlot.TryPlaceLetter(correctLetter);

        if (placedCorrectly)
        {
            CheckWin();
            return true;
        }

        Debug.LogWarning("Hint tried to place a letter, but TryPlaceLetter returned false.");
        return false;
    }
}