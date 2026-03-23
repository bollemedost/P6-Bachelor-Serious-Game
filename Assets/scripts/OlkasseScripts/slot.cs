using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class slot : MonoBehaviour, IDropHandler
{
    // List of valid IDs this slot can accept
    public List<string> correctItemIDs = new List<string>();

    [Header("Prefabs")]
    public GameObject tomatoPrefab;
    public GameObject coinPrefab;

    [Header("Spawn Points")]
    public RectTransform tomatoSpawnPoint;
    public RectTransform coinSpawnPoint;

    [Header("Sprite Change")]
    public Image slotImage;
    public Sprite correctSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip dropSound;

    [Tooltip("Delay før forkert lyd afspilles")]
    public float wrongSoundDelay = 0.2f;

    [Header("Coin Rewards/Penalties")]
    public int correctCoinReward = 10;
    public int wrongCoinPenalty = 5;

    public bool acceptAnyItem = false;

    [Header("Item Scale Settings")]
    public float anySlotScale = 2f;
    public float specificSlotScale = 1f;

    [Header("State (Read Only)")]
    [SerializeField] private bool isCorrectPlaced = false;
    public bool IsCorrectPlaced => isCorrectPlaced;

    public void OnDrop(PointerEventData eventData)
{
    if (transform.childCount != 0)
        return;

    GameObject dropped = eventData.pointerDrag;
    draggableItemSpeach draggable = dropped.GetComponent<draggableItemSpeach>();

    if (draggable == null)
        return;

    if (audioSource != null && dropSound != null)
    {
        audioSource.PlayOneShot(dropSound);
    }

    RectTransform droppedRect = dropped.GetComponent<RectTransform>();

    if (acceptAnyItem)
    {
        draggable.parentAfterDrag = transform;
        ApplyItemTransform(droppedRect, anySlotScale);
        return;
    }

    if (correctItemIDs.Contains(draggable.itemID))
    {
        draggable.parentAfterDrag = transform;
        draggable.LockItem();

        ApplyItemTransform(droppedRect, specificSlotScale);
        HandleCorrect();
    }
    else
    {
        draggable.parentAfterDrag = draggable.originalParent;
        ThrowTomato(droppedRect);
        HandleWrong();
    }
}

    void ApplyItemTransform(RectTransform rect, float scale)
    {
        if (rect == null) return;

        rect.SetParent(transform);
        rect.localScale = Vector3.one * scale;
        rect.localPosition = Vector3.zero;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void HandleCorrect()
    {
        isCorrectPlaced = true;

        // Change sprite
        if (slotImage != null && correctSprite != null)
        {
            slotImage.sprite = correctSprite;
        }

        // Play correct sound (ingen delay)
        if (audioSource != null && correctSound != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        // Give coins
        CoinManager.EnsureExists().AddCoin(correctCoinReward);

        // Throw coin visual
        ThrowCoin();
    }

    void HandleWrong()
    {
        // Play wrong sound med delay
        if (audioSource != null && wrongSound != null)
        {
            StartCoroutine(PlayWrongSoundDelayed());
        }

        // Remove coins
        CoinManager.EnsureExists().AddCoin(-wrongCoinPenalty);
    }

    IEnumerator PlayWrongSoundDelayed()
    {
        yield return new WaitForSeconds(wrongSoundDelay);
        audioSource.PlayOneShot(wrongSound);
    }

    void ThrowTomato(RectTransform target)
    {
        if (tomatoPrefab == null || tomatoSpawnPoint == null)
            return;

        GameObject tomato = Instantiate(tomatoPrefab, tomatoSpawnPoint.parent);

        RectTransform tomatoRect = tomato.GetComponent<RectTransform>();
        tomatoRect.position = tomatoSpawnPoint.position;

        UITomato uiTomato = tomato.GetComponent<UITomato>();
        uiTomato.Throw(target.position);
    }

    void ThrowCoin()
    {
        if (coinPrefab == null || coinSpawnPoint == null)
            return;

        GameObject coin = Instantiate(coinPrefab, coinSpawnPoint.parent);

        RectTransform coinRect = coin.GetComponent<RectTransform>();
        coinRect.position = coinSpawnPoint.position;

        UICoin uiCoin = coin.GetComponent<UICoin>();
        uiCoin.Throw();
    }
}