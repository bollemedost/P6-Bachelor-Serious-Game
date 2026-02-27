using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICoin : MonoBehaviour
{
    public float throwDuration = 0.6f;
    public float arcHeight = 100f;

    public float landPause = 0.1f;

    public Sprite flyingSprite;

    public float startScale = 0.3f;
    public float endScale = 1.2f;

    public float spinSpeed = 720f; // degrees per second

    RectTransform rectTransform;
    Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Throw()
    {
        StartCoroutine(AnimateThrow());
    }

    IEnumerator AnimateThrow()
{
    Vector3 startPos = rectTransform.position;
    Vector3 endPos = startPos + new Vector3(0, arcHeight, 0);

    float time = 0f;

    image.sprite = flyingSprite;
    rectTransform.localScale = Vector3.one * startScale;

    while (time < throwDuration)
    {
        float t = time / throwDuration;

        // Move upward only
        rectTransform.position = Vector3.Lerp(startPos, endPos, t);

        // Scale
        float scaleT = t * t;
        float scale = Mathf.Lerp(startScale, endScale, scaleT);
        rectTransform.localScale = Vector3.one * scale;

        // Spin
        rectTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        time += Time.deltaTime;
        yield return null;
    }

    yield return new WaitForSeconds(landPause);

    yield return StartCoroutine(Pop());

    Destroy(gameObject);
}

    IEnumerator Pop()
    {
        float popTime = 0.15f;
        float time = 0f;

        Vector3 startScaleVec = rectTransform.localScale;
        Vector3 popScale = startScaleVec * 1.3f;

        while (time < popTime)
        {
            float t = time / popTime;
            rectTransform.localScale = Vector3.Lerp(startScaleVec, popScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = popScale;
    }
}
