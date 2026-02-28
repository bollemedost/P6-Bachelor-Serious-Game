using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;       // Black overlay
    public float fadeDuration = 1f;           // Duration of scene fade
    public float startDelay = 0.1f;           // Delay before starting fade

    [Header("UI Canvases to Fade In")]
    public CanvasGroup[] uiCanvasGroups;      // Assign all player/UI canvases here
    public float uiFadeDuration = 0.5f;       // How long each UI fades in
    public float uiFadeDelay = 0.2f;          // Optional delay after scene fade before UI fade

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            // Start fully black
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.gameObject.SetActive(true);

            // Start all UI canvases fully transparent and active
            foreach (CanvasGroup cg in uiCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.gameObject.SetActive(true);
                }
            }

            StartCoroutine(FadeInSceneAndUI());
        }
    }

    private IEnumerator FadeInSceneAndUI()
    {
        // Wait initial start delay
        yield return new WaitForSeconds(startDelay);

        // --- Fade out black overlay ---
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);

        // Optional delay before UI fade
        yield return new WaitForSeconds(uiFadeDelay);

        // --- Fade in all UI canvases ---
        foreach (CanvasGroup cg in uiCanvasGroups)
        {
            if (cg != null)
            {
                elapsed = 0f;
                while (elapsed < uiFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }
    }
}