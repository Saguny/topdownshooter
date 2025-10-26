using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _baseMaxHealth = 100f; // base health per enemy
    [SerializeField] private float _healthScale = 1f;      // multiplier set by WaveManager.cs -> see that file
    private float _currentHealth;

    [Header("UI (optional)")]
    [SerializeField] private Image _healthBarFill; 

    
    public static event Action OnEnemyDied;

    private void Awake()
    {
        _currentHealth = GetScaledMaxHealth();
        UpdateHealthBar();
    }

    // called by WaveManager to scale enemy health each wave
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

        if (_currentHealth <= 0)
            Die();
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
        OnEnemyDied?.Invoke();
        Destroy(gameObject);
    }
}
