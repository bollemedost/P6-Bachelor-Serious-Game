using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class slot : MonoBehaviour, IDropHandler
{
    public string correctItemID;

    public GameObject tomatoPrefab;
    public RectTransform tomatoSpawnPoint; // UI spawn position
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
            //Correct
            draggable.parentAfterDrag = transform;
            draggable.LockItem();
        }
        else
        {
            //Wrong
            draggable.parentAfterDrag = draggable.originalParent;

            ThrowTomato(dropped.GetComponent<RectTransform>());
        }
    }

    void ThrowTomato(RectTransform target)
    {
        GameObject tomato = Instantiate(tomatoPrefab, tomatoSpawnPoint.parent);

        RectTransform tomatoRect = tomato.GetComponent<RectTransform>();
        tomatoRect.anchoredPosition = tomatoSpawnPoint.anchoredPosition;

        UITomato uiTomato = tomato.GetComponent<UITomato>();

        uiTomato.Throw(target.anchoredPosition);
    }
}

