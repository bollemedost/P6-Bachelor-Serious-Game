using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UITomato : MonoBehaviour
{
    public float throwDuration = 0.6f;
    public float arcHeight = 100f;

    public float smashPause = 0.15f;

    public float slideDuration = 0.8f;
    public float slideDistance = 300f;

    public Sprite flyingSprite;
    public Sprite smashedSprite;

    public float startScale = 0.3f;
    public float endScale = 1.4f;

    RectTransform rectTransform;
    Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Throw(Vector3 targetWorldPos)
    {
        StartCoroutine(AnimateThrow(targetWorldPos));
    }

    IEnumerator AnimateThrow(Vector3 targetWorldPos)
    {
        Vector3 startPos = rectTransform.position;

        float time = 0f;

        image.sprite = flyingSprite;
        rectTransform.localScale = Vector3.one * startScale;

        // ===== THROW ARC WITH SCALE =====
        while (time < throwDuration)
        {
            float t = time / throwDuration;

            // Position (world space)
            Vector3 pos = Vector3.Lerp(startPos, targetWorldPos, t);

            // Arc
            pos.y += arcHeight * Mathf.Sin(t * Mathf.PI);

            rectTransform.position = pos;

            // Scale (ease-in)
            float scaleT = t * t;
            float scale = Mathf.Lerp(startScale, endScale, scaleT);
            rectTransform.localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = targetWorldPos;
        rectTransform.localScale = Vector3.one * endScale;

        // ===== SMASH =====
        image.sprite = smashedSprite;

        yield return new WaitForSeconds(smashPause);

        yield return StartCoroutine(SlideDown());

        Destroy(gameObject);
    }

    IEnumerator SlideDown()
    {
        Vector3 start = rectTransform.position;
        Vector3 end = start - new Vector3(0, slideDistance, 0);

        float time = 0f;

        while (time < slideDuration)
        {
            float t = time / slideDuration;

            float easedT = t * t;

            rectTransform.position = Vector3.Lerp(start, end, easedT);

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = end;
    }
}
