using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsController : MonoBehaviour
{
    [System.Serializable]
    public class SpritePair
    {
        public string matchId;
        public Sprite spriteA;
        public Sprite spriteB;
    }

    [System.Serializable]
    public class Level
    {
        public List<SpritePair> pairs = new List<SpritePair>();
    }

    [Header("Levels")]
    [SerializeField] private List<Level> levels = new List<Level>();
    [SerializeField] private float revealTime = 0.4f;
    [SerializeField] private float nextLevelDelay = 1f;

    [Header("Coin Rewards")]
    [SerializeField] private int coinsForCorrectMatch = 5;
    [SerializeField] private int coinsForWrongMatch = -1;

    [Header("Minigame Complete Reward")]
    [Tooltip("How many coins to award when ALL levels in this minigame are completed.")]
    [SerializeField] private int rewardCoinsOnComplete = 20;

    [Tooltip("Assign your MinigameCompleteUI (canvas script) here.")]
    [SerializeField] private MinigameCompleteUI completeUI;

    private int currentLevel = 0;

    [Header("Board")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;

    private List<CardData> cardsToSpawn;

    private Card firstSelected;
    private Card secondSelected;

    private bool canSelect = true;
    private int matchesFound = 0;

    private struct CardData
    {
        public string id;
        public Sprite sprite;

        public CardData(string id, Sprite sprite)
        {
            this.id = id;
            this.sprite = sprite;
        }
    }

    private void Start()
    {
        StartLevel(0);
    }

    private void StartLevel(int levelIndex)
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels set on CardsController. Add levels and pairs in the Inspector.", this);
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Level index {levelIndex} is out of range.", this);
            return;
        }

        currentLevel = levelIndex;
        matchesFound = 0;
        firstSelected = null;
        secondSelected = null;
        canSelect = true;

        ClearBoard();
        PrepareCardsForCurrentLevel();
        CreateCards();
    }

    private void PrepareCardsForCurrentLevel()
    {
        cardsToSpawn = new List<CardData>();

        var pairList = levels[currentLevel].pairs;

        for (int i = 0; i < pairList.Count; i++)
        {
            var p = pairList[i];

            if (p.spriteA == null || p.spriteB == null)
            {
                Debug.LogWarning($"Level {currentLevel} pair {i} ('{p.matchId}') is missing spriteA or spriteB. Skipping.", this);
                continue;
            }

            cardsToSpawn.Add(new CardData(p.matchId, p.spriteA));
            cardsToSpawn.Add(new CardData(p.matchId, p.spriteB));
        }

        Shuffle(cardsToSpawn);
    }

    private void CreateCards()
    {
        for (int i = 0; i < cardsToSpawn.Count; i++)
        {
            Card c = Instantiate(cardPrefab, gridTransform);
            c.controller = this;

            var d = cardsToSpawn[i];
            c.SetData(d.id, d.sprite);

            c.Hide();
        }
    }

    public void SetSelected(Card card)
    {
        if (!canSelect) return;
        if (card == null) return;
        if (card.isSelected) return;

        card.Show();

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        secondSelected = card;
        canSelect = false;
        StartCoroutine(CheckMatching(firstSelected, secondSelected));
    }

    private IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(revealTime);

        if (a != null && b != null && a.matchId == b.matchId)
        {
            matchesFound++;

            // ADD COINS (GREEN)
            CoinManager.EnsureExists().AddCoin(coinsForCorrectMatch);

            if (matchesFound >= GetExpectedMatchesThisLevel())
            {
                yield return new WaitForSeconds(nextLevelDelay);
                LoadNextLevel();
            }
        }
        else
        {
            // REMOVE COINS (RED)
            CoinManager.EnsureExists().AddCoin(coinsForWrongMatch);

            if (a != null) a.Hide();
            if (b != null) b.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        canSelect = true;
    }

    private int GetExpectedMatchesThisLevel()
    {
        int count = 0;
        var pairList = levels[currentLevel].pairs;

        for (int i = 0; i < pairList.Count; i++)
        {
            if (pairList[i].spriteA != null && pairList[i].spriteB != null)
                count++;
        }
        return count;
    }

    private void LoadNextLevel()
    {
        int next = currentLevel + 1;

        if (next >= levels.Count)
        {
            Debug.Log("All levels completed!");

            if (completeUI != null)
            {
                canSelect = false;
                completeUI.Show(rewardCoinsOnComplete);
            }
            return;
        }

        StartLevel(next);
    }

    private void ClearBoard()
    {
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridTransform.GetChild(i).gameObject);
        }
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}