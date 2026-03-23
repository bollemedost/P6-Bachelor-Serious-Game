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

    [Header("Audio")]
    [SerializeField] private AudioClip flipSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        // Sørg for der er en AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D lyd (vigtigt!)
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
        if (iconImage != null)
            iconImage.sprite = frontSprite;

        isSelected = true;

        // 🔊 Spil flip-lyd
        PlayFlipSound();
    }

    public void Hide()
    {
        if (iconImage != null)
            iconImage.sprite = hiddenIconSprite;

        isSelected = false;
    }

    private void PlayFlipSound()
    {
        if (flipSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flipSound);
        }
    }
}