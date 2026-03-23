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

    private AudioClip cardSound;
    private AudioSource audioSource;

    public bool isSelected;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnCardClick()
    {
        if (controller == null) return;
        controller.SetSelected(this);
    }

    public void SetData(string id, Sprite sprite, AudioClip sound)
    {
        matchId = id;
        frontSprite = sprite;
        cardSound = sound;
    }

    public void Show()
    {
        if (iconImage != null)
            iconImage.sprite = frontSprite;

        isSelected = true;

        if (cardSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cardSound);
        }
    }

    public void Hide()
    {
        if (iconImage != null)
            iconImage.sprite = hiddenIconSprite;

        isSelected = false;
    }
}