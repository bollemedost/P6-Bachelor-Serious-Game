using UnityEngine;
using UnityEngine.EventSystems;

public class slot : MonoBehaviour, IDropHandler
{
    public string correctItemID;

    public GameObject tomatoPrefab;
    public RectTransform tomatoSpawnPoint;
    public bool acceptAnyItem = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount != 0)
            return;

        GameObject dropped = eventData.pointerDrag;
        draggableItemSpeach draggable = dropped.GetComponent<draggableItemSpeach>();

        if (acceptAnyItem)
        {
            draggable.parentAfterDrag = transform;
            return;
        }

        if (draggable.itemID == correctItemID)
        {
            draggable.parentAfterDrag = transform;
            draggable.LockItem();
        }
        else
        {
            draggable.parentAfterDrag = draggable.originalParent;
            ThrowTomato(dropped.GetComponent<RectTransform>());
        }
    }

    void ThrowTomato(RectTransform target)
    {
        GameObject tomato = Instantiate(tomatoPrefab, tomatoSpawnPoint.parent);

        RectTransform tomatoRect = tomato.GetComponent<RectTransform>();

        // EXACT world position match
        tomatoRect.position = tomatoSpawnPoint.position;

        UITomato uiTomato = tomato.GetComponent<UITomato>();

        uiTomato.Throw(target.position);
    }
}

