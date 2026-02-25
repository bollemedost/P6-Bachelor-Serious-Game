using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Ease in: fade to black
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration, EaseInOutQuad));

        // Load new scene
        SceneManager.LoadScene(sceneName);

        // Optional: wait a frame
        yield return null;

        // Ease out: fade from black
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration, EaseInOutQuad));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration, System.Func<float, float> easing)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadePanel.alpha = Mathf.Lerp(startAlpha, endAlpha, easing(t));
            yield return null;
        }

        fadePanel.alpha = endAlpha;
    }

    // Ease in/out quadratic
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}