using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Fade Settings")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadePanel != null)
            {
                fadePanel.alpha = 0f;
                fadePanel.interactable = false;
                fadePanel.blocksRaycasts = false;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void FadeToScene(string sceneName)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade Panel not assigned! Loading scene without fade.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration, true));
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration, false));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration, bool blockRaycasts)
    {
        if (fadePanel == null) yield break;

        fadePanel.interactable = blockRaycasts;
        fadePanel.blocksRaycasts = blockRaycasts;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadePanel.alpha = Mathf.Lerp(startAlpha, endAlpha, EaseInOutQuad(t));
            yield return null;
        }

        fadePanel.alpha = endAlpha;

        if (endAlpha <= 0.01f)
        {
            fadePanel.interactable = false;
            fadePanel.blocksRaycasts = false;
        }
        else
        {
            fadePanel.interactable = true;
            fadePanel.blocksRaycasts = true;
        }
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    public void LoadSceneFromButton(string sceneName)
    {
        FadeToScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            fadePanel.interactable = true;
            fadePanel.blocksRaycasts = true;
            StartCoroutine(Fade(1f, 0f, fadeDuration, false));
        }
    }
}