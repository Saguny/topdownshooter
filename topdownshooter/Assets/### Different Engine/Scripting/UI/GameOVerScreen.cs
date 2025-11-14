using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;

    [Header("Texts")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;

    [Header("Optional Icons")]
    [SerializeField] private Image levelIcon;
    [SerializeField] private Image timeIcon;
    [SerializeField] private Image killsIcon;

    public void Setup(float currentHealth)
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // Player inventory → level + kills
        var inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
        {
            if (levelText != null)
                levelText.text = "Level " + inv.CurrentLevel;

            if (killsText != null)
                killsText.text = " " + inv.totalKills;
        }

        // Show / hide icons based on assignment
        if (levelIcon != null)
            levelIcon.gameObject.SetActive(levelIcon.sprite != null);

        if (killsIcon != null)
            killsIcon.gameObject.SetActive(killsIcon.sprite != null);

        // SpawnDirector → runtime
        var dir = FindObjectOfType<SpawnDirector>();
        if (dir != null && timeText != null)
        {
            float t = dir.GetRunTime();
            timeText.text = " " + FormatTime(t);
        }

        if (timeIcon != null)
            timeIcon.gameObject.SetActive(timeIcon.sprite != null);

        // pause the game
        Time.timeScale = 0f;
    }

    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainTestGame");
    }

    public void ExitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min:00}:{sec:00}";
    }
}
