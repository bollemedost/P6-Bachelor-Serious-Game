using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrosswordSlotT : MonoBehaviour
{
    public string correctLetter;
    public bool isFilledCorrectly = false;

    public TMP_Text letterText;
    public TMP_Text clueNumberText;
    public Image backgroundImage;

    private void Awake()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

    public void Setup(string targetLetter, string clueNumber = "")
    {
        correctLetter = targetLetter.ToUpper();
        isFilledCorrectly = false;

        if (letterText != null)
            letterText.text = "";

        if (clueNumberText != null)
            clueNumberText.text = clueNumber;
    }

    public bool TryPlaceLetter(string letter)
    {
        if (isFilledCorrectly)
            return false;

        if (letter.ToUpper() == correctLetter)
        {
            isFilledCorrectly = true;

            if (letterText != null)
                letterText.text = correctLetter;

            return true;
        }

        return false;
    }
}