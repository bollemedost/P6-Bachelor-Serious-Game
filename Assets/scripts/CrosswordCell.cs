using UnityEngine;
using UnityEngine.EventSystems;

public class CrosswordCell : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;

    private CrosswordGridManager manager;

    public void Setup(CrosswordGridManager mgr, int cx, int cy)
    {
        manager = mgr;
        x = cx;
        y = cy;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CLICK {x},{y}");
        manager.SelectCell(x, y);
    }
}