using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    [HideInInspector] public CardsController controller;

    [Header("Back of card")]
    public Sprite hiddenIconSprite;

    [HideInInspector] public string matchId;
    [HideInInspector] public Sprite frontSprite;

    public bool isSelected;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);
    }

    public void OnCardClick()
    {
        if (controller == null) return;
        controller.SetSelected(this);
    }

    public void SetData(string id, Sprite sprite)
    {
        matchId = id;
        frontSprite = sprite;
    }

    public void Show()
    {
        if (iconImage != null) iconImage.sprite = frontSprite;
        isSelected = true;
    }

    public void Hide()
    {
        if (iconImage != null) iconImage.sprite = hiddenIconSprite;
        isSelected = false;
    }
}