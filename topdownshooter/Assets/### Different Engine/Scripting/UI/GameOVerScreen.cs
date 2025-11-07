using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI; // Canvas oder Panel, das angezeigt werden soll        

    // Wird von PlayerHealth.cs aufgerufen
    public void Setup(float currentHealth)
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true); // Zeigt den Game Over Screen an
        
        // optional Spiel pausieren
        Time.timeScale = 0f;
    }

    public void RestartButton()
    {
        Time.timeScale = 1f; // Zeit wieder starten
        SceneManager.LoadScene("MainTestGame");
    }

    public void ExitButton()
    {
        Time.timeScale = 1f; // Zeit wieder starten
        SceneManager.LoadScene("MainMenu");
    }
}
