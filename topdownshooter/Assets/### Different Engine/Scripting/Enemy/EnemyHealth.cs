using System.Collections;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour, IHealth
{
    [Header("health")]
    [Min(1f)] public float baseMaxHealth = 10f;
    [SerializeField] private float currentHealth;

    [Tooltip("flat damage reduced from each hit")]
    [Min(0f)] public float armor = 0f;

    [Header("death & drops")]
    [SerializeField] private GameObject gearDropPrefab;
    [SerializeField, Min(0)] private int minGear = 1;
    [SerializeField, Min(0)] private int maxGear = 1;
    [SerializeField, Min(0)] private int baseGearsOnKill = 1;
    [Range(0f, 1f)][SerializeField] private float gearDropChance = 0.35f;
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private AudioClip deathSound;
    [Range(0f, 1f)][SerializeField] private float deathVolume = 1f;

    [Header("heal item drop")]
    [SerializeField] private GameObject healItemPrefab;
    [Range(0f, 1f)][SerializeField] private float healDropChance = 0.01f;

    [Header("damage feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)][SerializeField] private float hitVolume = 1f;
    [SerializeField] private AudioSource audioSource;

    [Header("debug")]
    public bool destroyOnDeath = true;

    public static Action OnEnemyDied;

    public float Max => baseMaxHealth;
    public float Current => currentHealth;
    public event Action<float, float> OnHealthChanged;

    private bool _dead;
    private Color _originalColor;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        if (!CompareTag("Enemy")) gameObject.tag = "Enemy";
        ResetHealth();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        EnemyRegistry.Register(gameObject);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    private void OnDisable()
    {
        EnemyRegistry.Unregister(gameObject);
    }

    public void ResetHealth()
    {
        _dead = false;
        currentHealth = Mathf.Max(1f, baseMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    public void SetScaled(float absoluteMaxHealth)
    {
        baseMaxHealth = Mathf.Max(1f, absoluteMaxHealth);
        currentHealth = baseMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    public bool TakeDamage(float rawDamage)
    {
        if (_dead) return false;

        float dmg = Mathf.Max(0f, rawDamage - armor);
        if (dmg <= 0f) return false;

        currentHealth -= dmg;

        if (spriteRenderer != null)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRed());
        }

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
            Die();
            return true;
        }

        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
        return false;
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = _originalColor;
    }

    private void Die()
    {
        if (_dead) return;
        _dead = true;

        try { OnEnemyDied?.Invoke(); } catch { }
        try { GameEvents.OnEnemyKilled?.Invoke(1); } catch { }

        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);

        if (baseGearsOnKill > 0)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent<PlayerInventory>(out var inv))
                inv.AddGears(baseGearsOnKill);
        }

        if (gearDropPrefab != null &&
            (minGear > 0 || maxGear > 0) &&
            UnityEngine.Random.value <= gearDropChance)
        {
            int count = Mathf.Clamp(UnityEngine.Random.Range(minGear, maxGear + 1), 0, 999);
            for (int i = 0; i < count; i++)
            {
                var p = transform.position;
                p.x += UnityEngine.Random.Range(-0.25f, 0.25f);
                p.y += UnityEngine.Random.Range(-0.25f, 0.25f);
                Instantiate(gearDropPrefab, p, Quaternion.identity);
            }
        }

        if (healItemPrefab != null && UnityEngine.Random.value <= healDropChance)
        {
            Vector3 dropPos = transform.position;
            dropPos.x += UnityEngine.Random.Range(-0.2f, 0.2f);
            dropPos.y += UnityEngine.Random.Range(-0.2f, 0.2f);
            Instantiate(healItemPrefab, dropPos, Quaternion.identity);
        }

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxGear < minGear) maxGear = minGear;
        baseMaxHealth = Mathf.Max(1f, baseMaxHealth);
        baseGearsOnKill = Mathf.Max(0, baseGearsOnKill);
        if (!Application.isPlaying)
            currentHealth = Mathf.Clamp(currentHealth, 0f, baseMaxHealth);
    }
#endif
}
