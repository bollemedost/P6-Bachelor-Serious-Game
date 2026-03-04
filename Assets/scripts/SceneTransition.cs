using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Fade Settings")]
    public CanvasGroup fadePanel;        // Assign a CanvasGroup on a full-screen black panel
    public float fadeDuration = 1f;      // Duration for fade in/out

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure fadePanel starts fully transparent
            if (fadePanel != null)
                fadePanel.alpha = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this to transition to a new scene
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade Panel not assigned!");
            SceneManager.LoadScene(sceneName); // fallback
            return;
        }

        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration, EaseInOutQuad));

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // Wait a frame to ensure scene fully loads
        yield return null;

        // Optional tiny delay for smoothness
        yield return new WaitForSeconds(0.05f);

        // Fade back from black
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

    // Ease in/out quadratic function for smoother fade
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    public void LoadSceneFromButton(string sceneName)
    {
        FadeToScene(sceneName);
    }
}