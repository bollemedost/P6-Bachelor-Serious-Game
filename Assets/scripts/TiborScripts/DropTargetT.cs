using UnityEngine;
using UnityEngine.EventSystems;

public class DropTargetT : MonoBehaviour, IDropHandler
{
    private CrosswordSlotT slot;

    void Awake()
    {
        slot = GetComponent<CrosswordSlotT>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragLetterDataT.CurrentLetter == null || slot == null)
            return;

        bool correct = slot.TryPlaceLetter(DragLetterDataT.CurrentLetter.letter);

        if (correct)
        {
            CoinManager.EnsureExists().AddCoin(2);

            if (CoinTextFeedback.Instance != null)
                CoinTextFeedback.Instance.FlashForChange(2);

            CrosswordGameManagerT.Instance.CheckWin();
        }
        else
        {
            Debug.Log("Wrong letter");
        }
    }
}