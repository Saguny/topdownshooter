using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;          
    public float fadeDuration = 1f;  
    public float blackHoldTime = 1f; 

    private bool isLoading = false;

    public void PlayGame()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneWithFade());
        }
    }

    private IEnumerator LoadSceneWithFade()
    {
        isLoading = true;

        
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        float t = 0f;

        
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = Color.Lerp(startColor, endColor, lerp);
            yield return null;
        }

        
        fadeImage.color = endColor;

        
        yield return new WaitForSecondsRealtime(blackHoldTime);

        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
