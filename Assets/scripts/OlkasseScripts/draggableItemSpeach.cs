using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class draggableItemSpeach : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemID;  // Set in Inspector
    public Image image;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform originalParent;

    private bool isLocked = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        originalParent = transform.parent;
        parentAfterDrag = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }

    public void LockItem()
    {
        isLocked = true;
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
        image.raycastTarget = false;
    }
}