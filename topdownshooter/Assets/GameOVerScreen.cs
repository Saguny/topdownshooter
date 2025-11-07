using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Für Text oder UI-Elemente

public class GameOVerScreen : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI; // Canvas oder Panel, das angezeigt werden soll
    [SerializeField] private Text healthText;        // (optional) Text, um HP anzuzeigen

    // Wird von PlayerHealth.cs aufgerufen
    public void Setup(float currentHealth)
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true); // Zeigt den Game Over Screen an

        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString("0");
        
        // Optional: Spiel pausieren
        Time.timeScale = 0f;
    }

    public void RestartButton()
    {
        Time.timeScale = 1f; // Zeit wieder starten
        SceneManager.LoadScene("MainGame");
    }

    public void ExitButton()
    {
        Time.timeScale = 1f; // Zeit wieder starten
        SceneManager.LoadScene("MainMenu");
    }
}
