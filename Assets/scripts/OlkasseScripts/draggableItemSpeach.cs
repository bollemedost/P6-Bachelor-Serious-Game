using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class draggableItemSpeach : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemID;  // Set in Inspector
    public Image image;

    [Header("Speech")]
    public AudioClip dragSpeechClip;
    public bool playSpeechOnBeginDrag = true;
    public bool stopSpeechOnEndDrag = false;

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

        if (image != null)
            image.raycastTarget = false;

        if (playSpeechOnBeginDrag && UlkassePart1AudioManager.Instance != null)
        {
            UlkassePart1AudioManager.Instance.PlayDraggedItemSpeech(dragSpeechClip);
        }
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

        if (image != null)
            image.raycastTarget = true;

        if (stopSpeechOnEndDrag && UlkassePart1AudioManager.Instance != null)
        {
            UlkassePart1AudioManager.Instance.StopDraggedItemSpeech();
        }
    }

    public void LockItem()
    {
        isLocked = true;
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;

        if (image != null)
            image.raycastTarget = false;
    }
}