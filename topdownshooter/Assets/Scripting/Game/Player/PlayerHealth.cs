using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Image healthBarFill;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;      // dein Player-Sprite
    public Color hitColor = Color.red;         // Farbe beim Treffer
    public float hitFlashDuration = 0.1f;      // Dauer des Aufleuchtens

    [Header("Audio")]
    public AudioClip hitSound;                 // optionaler Treffer-Sound
    public float hitSoundVolume = 0.7f;
    private AudioSource audioSource;

    private Color originalColor;
    private Coroutine flashRoutine;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // AudioSource hinzufügen, falls nicht vorhanden
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();

        // 🔴 visueller Flash
        if (spriteRenderer != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRed());
        }

        // 🔊 Treffer-Sound
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound, hitSoundVolume);

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            // Hier kannst du den Tod implementieren
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
