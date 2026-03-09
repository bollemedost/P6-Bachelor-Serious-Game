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
        dragVisual = Instantiate(gameObject, canvas.transform);
        dragVisual.name = gameObject.name + "_DragVisual";

        DraggableLetterT dragLetter = dragVisual.GetComponent<DraggableLetterT>();
        dragLetter.enabled = false;

        CanvasGroup cg = dragVisual.GetComponent<CanvasGroup>();
        if (cg == null) cg = dragVisual.AddComponent<CanvasGroup>();

        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        RectTransform dragRect = dragVisual.GetComponent<RectTransform>();
        dragRect.position = eventData.position;

        DragLetterDataT.CurrentLetter = this;
        DragLetterDataT.CurrentDragVisual = dragVisual;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragLetterDataT.CurrentDragVisual != null)
        {
            DragLetterDataT.CurrentDragVisual.GetComponent<RectTransform>().position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragLetterDataT.CurrentDragVisual != null)
            Destroy(DragLetterDataT.CurrentDragVisual);

        DragLetterDataT.CurrentLetter = null;
        DragLetterDataT.CurrentDragVisual = null;
    }
}