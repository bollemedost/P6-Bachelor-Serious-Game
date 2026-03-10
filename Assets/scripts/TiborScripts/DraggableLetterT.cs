using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableLetterT : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string letter;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [HideInInspector] public TMP_Text label;

    private GameObject dragVisual;

    // Added so the WHOLE box can move and then return back
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        label = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(string newLetter)
    {
        letter = newLetter;

        if (label != null)
            label.text = newLetter;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Keep your old variables so nothing else breaks
        dragVisual = gameObject;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalAnchoredPosition = rectTransform.anchoredPosition;

        // Move the WHOLE button to the top canvas while dragging
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        rectTransform.position = eventData.position;

        DragLetterDataT.CurrentLetter = this;
        DragLetterDataT.CurrentDragVisual = dragVisual;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Put the whole button back where it came from
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        DragLetterDataT.CurrentLetter = null;
        DragLetterDataT.CurrentDragVisual = null;
    }
}