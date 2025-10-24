using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 50f;
    private float _currentHealth;

    [Header("UI (optional)")]
    [SerializeField] private Image _healthBarFill;

    public static event Action OnEnemyDied; // notify when an enemy dies

    private void Awake()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        UpdateHealthBar();

        if (_currentHealth <= 0)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (_healthBarFill != null)
            _healthBarFill.fillAmount = _currentHealth / _maxHealth;
    }

    private void Die()
    {
        OnEnemyDied?.Invoke(); // fire event
        Destroy(gameObject);
    }
}
