using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Optional Visuals")]
    [SerializeField] private Image targetImage;   // can be empty or have no sprite
    [SerializeField] private TMP_Text targetText; // optional text highlight

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;

    [Header("Scaling")]
    [SerializeField] private float scaleMultiplier = 1.08f;
    [SerializeField] private float scaleDuration = 0.15f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.15f;

    private Coroutine fadeRoutine;
    private Coroutine scaleRoutine;
    private Vector3 originalScale;

    private void Awake()
    {
        // grab fallback references automatically
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetText == null) targetText = GetComponentInChildren<TMP_Text>();

        originalScale = transform.localScale;

        if (targetImage != null)
            targetImage.color = normalColor;

        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(hoverColor);
        StartScale(originalScale * scaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(normalColor);
        StartScale(originalScale);
    }

    private void StartFade(Color target)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeColor(target));
    }

    private void StartScale(Vector3 target)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    private IEnumerator FadeColor(Color target)
    {
        float t = 0f;

        Color startImg = targetImage ? targetImage.color : Color.white;
        Color startTxt = targetText ? targetText.color : Color.white;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);

            if (targetImage) targetImage.color = Color.Lerp(startImg, target, eased);
            if (targetText) targetText.color = Color.Lerp(startTxt, target, eased);

            yield return null;
        }

        if (targetImage) targetImage.color = target;
        if (targetText) targetText.color = target;
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / scaleDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(start, target, eased);
            yield return null;
        }
        transform.localScale = target;
    }
}
