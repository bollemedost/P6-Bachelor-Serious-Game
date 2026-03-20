using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonSceneManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Button Unlock After Audio")]
    [SerializeField] private Button targetButton;
    [SerializeField] private AudioSource introAudioSource;

    private void Start()
    {
        // If no button is assigned, try to get it from this object
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        // Safety check
        if (targetButton != null)
        {
            targetButton.interactable = false;
        }

        StartCoroutine(EnableButtonWhenAudioEnds());
    }

    private IEnumerator EnableButtonWhenAudioEnds()
    {
        // If there is no audio source assigned, enable button immediately
        if (introAudioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned to ButtonSceneManager. Button will be enabled immediately.");

            if (targetButton != null)
            {
                targetButton.interactable = true;
            }

            yield break;
        }

        // Wait until the audio actually starts playing
        while (!introAudioSource.isPlaying)
        {
            yield return null;
        }

        // Wait until the audio has finished
        while (introAudioSource.isPlaying)
        {
            yield return null;
        }

        // Enable the button
        if (targetButton != null)
        {
            targetButton.interactable = true;
        }
    }

    public void LoadScene()
    {
        // Extra protection so it cannot be pressed too early from code or weird UI state
        if (targetButton != null && !targetButton.interactable)
        {
            return;
        }

        if (SceneFadeIn.instance != null)
        {
            SceneFadeIn.instance.FadeOutAndLoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneFadeIn instance not found. Loading without fade.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}