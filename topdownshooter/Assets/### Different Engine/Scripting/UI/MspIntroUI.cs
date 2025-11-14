using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapIntroUI : MonoBehaviour
{
    [Header("ui references")]
    [SerializeField] private CanvasGroup canvasGroup;   
    [SerializeField] private TMP_Text mapNameText;      
    [SerializeField] private Image backgroundImage;     

    [Header("map info")]
    [SerializeField] private string mapName = "n e x o n i a";

    [Header("timing (seconds)")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("sound")]
    [SerializeField] private AudioSource audioSource;   
    [SerializeField] private AudioClip appearSfx;       

    private bool hasPlayed = false; 

    private void Start()
    {
        
        if (hasPlayed) return;
        hasPlayed = true;

        if (mapNameText != null)
            mapNameText.text = mapName;

        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        
        gameObject.SetActive(true);

        StartCoroutine(FadeRoutine());
    }

    private System.Collections.IEnumerator FadeRoutine()
    {
        
        if (audioSource != null && appearSfx != null)
        {
            audioSource.PlayOneShot(appearSfx);
        }

        
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeInDuration);
            if (canvasGroup != null)
                canvasGroup.alpha = normalized;
            yield return null;
        }

        
        yield return new WaitForSeconds(holdDuration);

        
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeOutDuration);
            if (canvasGroup != null)
                canvasGroup.alpha = 1f - normalized;
            yield return null;
        }

        
        gameObject.SetActive(false);
    }
}
