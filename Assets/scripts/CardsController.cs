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

    [Header("Cards (Single Level)")]
    [SerializeField] private List<SpritePair> pairs = new List<SpritePair>();

    [Header("Timing")]
    [SerializeField] private float revealTime = 0.4f;   // how long cards stay revealed before flipping back

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
        StartGame();
    }

    private void StartGame()
    {
        if (pairs == null || pairs.Count == 0)
        {
            Debug.LogError("No pairs set on CardsController. Add pairs in the Inspector.", this);
            return;
        }

        matchesFound = 0;
        firstSelected = null;
        secondSelected = null;
        canSelect = true;

        ClearBoard();
        PrepareCards();
        CreateCards();
    }

    private void PrepareCards()
    {
        cardsToSpawn = new List<CardData>();

        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];

            if (p.spriteA == null || p.spriteB == null)
            {
                Debug.LogWarning($"Pair {i} ('{p.matchId}') is missing spriteA or spriteB. Skipping.", this);
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

            c.Hide(); // start face-down
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

            // matched -> keep them shown
            if (matchesFound >= GetExpectedMatches())
            {
                Debug.Log("Game completed!");
                // Hvis du vil: her kan du vise en win-screen eller genstarte spillet
            }
        }
        else
        {
            // not matched -> flip back
            if (a != null) a.Hide();
            if (b != null) b.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        canSelect = true;
    }

    private int GetExpectedMatches()
    {
        // Count only valid pairs (both sprites present)
        int count = 0;

        for (int i = 0; i < pairs.Count; i++)
        {
            if (pairs[i].spriteA != null && pairs[i].spriteB != null)
                count++;
        }

        return count;
    }

    private void ClearBoard()
    {
        if (gridTransform == null) return;

        // Destroy existing children under the grid
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