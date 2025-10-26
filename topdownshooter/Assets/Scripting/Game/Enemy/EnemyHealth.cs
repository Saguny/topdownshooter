using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _baseMaxHealth = 100f;
    [SerializeField] private float _healthScale = 1f;
    private float _currentHealth;

    [Header("UI (optional)")]
    [SerializeField] private Image _healthBarFill;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _hitFlashDuration = 0.1f;
    [SerializeField] private Color _hitColor = Color.red;

    [Header("Death Effect")]
    [SerializeField] private GameObject _deathEffectPrefab;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _hitSound;           // Sound beim Treffer
    [SerializeField] private AudioClip _deathSound;         // Sound beim Tod
    [Range(0f, 1f)][SerializeField] private float _hitSoundVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float _deathSoundVolume = 1f;
    [SerializeField] private AudioSource _audioSource;      // optional – kann automatisch hinzugefügt werden

    public static event Action OnEnemyDied;

    private Color _originalColor;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        _currentHealth = GetScaledMaxHealth();
        UpdateHealthBar();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        // Falls kein AudioSource vorhanden ist → automatisch hinzufügen
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // Standard: 2D-Sound
        }
    }

    public void SetHealthScale(float scale)
    {
        _healthScale = scale;
        _currentHealth = GetScaledMaxHealth();
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        UpdateHealthBar();

        // 🔴 visueller Flash
        if (_spriteRenderer != null)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRed());
        }

        // 🔊 Treffer-Sound abspielen (wenn vorhanden)
        if (_hitSound != null)
            _audioSource.PlayOneShot(_hitSound, _hitSoundVolume);

        if (_currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRed()
    {
        _spriteRenderer.color = _hitColor;
        yield return new WaitForSeconds(_hitFlashDuration);
        _spriteRenderer.color = _originalColor;
    }

    private float GetScaledMaxHealth()
    {
        return _baseMaxHealth * _healthScale;
    }

    private void UpdateHealthBar()
    {
        if (_healthBarFill != null)
            _healthBarFill.fillAmount = Mathf.Clamp01(_currentHealth / GetScaledMaxHealth());
    }

    private void Die()
    {
        // 💥 Partikeleffekt spawnen
        if (_deathEffectPrefab != null)
            Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);

        // 🔊 Todessound separat abspielen, auch wenn Gegner sofort zerstört wird
        if (_deathSound != null)
        {
            GameObject soundObject = new GameObject("EnemyDeathSound");
            AudioSource tempSource = soundObject.AddComponent<AudioSource>();
            tempSource.clip = _deathSound;
            tempSource.volume = _deathSoundVolume;
            tempSource.spatialBlend = _audioSource.spatialBlend; // 2D/3D übernehmen
            tempSource.Play();
            Destroy(soundObject, _deathSound.length); // löscht sich selbst nach Ende
        }

        OnEnemyDied?.Invoke();

        // 🧨 Gegner sofort zerstören
        Destroy(gameObject);
    }
}
