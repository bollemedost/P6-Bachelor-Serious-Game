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

    private void OnEnable()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        if (targetButton != null)
        {
            targetButton.interactable = false;
        }

        StartCoroutine(EnableButtonWhenAudioEnds());
    }

    private IEnumerator EnableButtonWhenAudioEnds()
    {
        if (introAudioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned to ButtonSceneManager. Button will be enabled immediately.");

            if (targetButton != null)
            {
                targetButton.interactable = true;
            }

            yield break;
        }

        introAudioSource.Play();

        while (introAudioSource.isPlaying)
        {
            yield return null;
        }

        if (targetButton != null)
        {
            targetButton.interactable = true;
        }
    }

    public void LoadScene()
    {
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