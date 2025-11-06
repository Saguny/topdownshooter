using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour, IHealth
{
    [Header("Health")]
    [Min(1f)] public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Tooltip("Seconds of invulnerability after being hit")]
    [Min(0f)] public float invulnTimeOnHit = 0.15f;

    [Header("UI (optional)")]
    [Tooltip("Fill image for a HUD bar; left->right fill")]
    public Image healthBarFill;

    [Tooltip("Color over health percent (0=red, 0.5=yellow, 1=green)")]
    public Gradient hudGradient;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    [Min(0f)] public float hitFlashDuration = 0.1f;

    [Header("Audio (optional)")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitSoundVolume = 0.7f;

    // ----- IHealth -----
    public float Max => maxHealth;
    public float Current => currentHealth;
    public event Action<float, float> OnHealthChanged;

    // Events you can hook into
    public static Action OnPlayerDied;
    public static Action<float, float> OnPlayerHealed;   // (amount, newHealth)
    public static Action<float, float> OnPlayerDamaged;  // (amount, newHealth)

    // internals
    private AudioSource _audio;
    private Color _originalColor = Color.white;
    private Coroutine _flashRoutine;
    private float _invulnTimer;

    private void Reset()
    {
        
        hudGradient = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.90f, 0.15f, 0.15f), 0f), // red
                new GradientColorKey(new Color(0.95f, 0.85f, 0.20f), 0.5f), // yellow
                new GradientColorKey(new Color(0.18f, 0.78f, 0.20f), 1f), // green
            },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };
    }

    private void Awake()
    {
        currentHealth = Mathf.Max(1f, maxHealth);

        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;

        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        PushHealthChanged();
        UpdateHudBar();
    }

    private void OnEnable()
    {
        
        PushHealthChanged();
        UpdateHudBar();
        _invulnTimer = 0f;
    }

    private void Update()
    {
        if (_invulnTimer > 0f) _invulnTimer -= Time.deltaTime;
    }

    

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        float prev = currentHealth;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (!Mathf.Approximately(prev, currentHealth))
        {
            OnPlayerHealed?.Invoke(currentHealth - prev, currentHealth);
            PushHealthChanged();
            UpdateHudBar();
        }
    }

    public void SetMax(float newMax, bool fillToMax = false)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (fillToMax) currentHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        PushHealthChanged();
        UpdateHudBar();
    }

    
    public bool TakeDamage(float amount)
    {
        if (amount <= 0f) return false;
        if (_invulnTimer > 0f) return false;

        float prev = currentHealth;
        currentHealth -= amount;
        _invulnTimer = invulnTimeOnHit;

        // visual flash
        if (spriteRenderer != null)
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        // sound
        if (hitSound != null)
            _audio.PlayOneShot(hitSound, hitSoundVolume);

        OnPlayerDamaged?.Invoke(prev - Mathf.Max(currentHealth, 0f), Mathf.Max(currentHealth, 0f));
        PushHealthChanged();
        UpdateHudBar();

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnPlayerDied?.Invoke();
            // TODO: trigger game over / respawn flow here
            return true;
        }

        return false;
    }

    

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;
    }

    private void UpdateHudBar()
    {
        if (healthBarFill == null) return;

        float t = Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth));
        healthBarFill.fillAmount = t;

        // color from gradient (green→yellow→red)
        if (hudGradient.colorKeys != null && hudGradient.colorKeys.Length > 0)
            healthBarFill.color = hudGradient.Evaluate(t);
        else
            healthBarFill.color = Color.Lerp(Color.red, Color.green, t);
    }

    private void PushHealthChanged() => OnHealthChanged?.Invoke(currentHealth, maxHealth);
}
