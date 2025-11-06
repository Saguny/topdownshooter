using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour, IHealth
{
    [Header("health")]
    [Min(1f)] public float baseMaxHealth = 10f;
    [SerializeField, ReadOnlyField] private float currentHealth;

    [Tooltip("flat damage reduced from each hit")]
    [Min(0f)] public float armor = 0f;

    [Header("death & drops")]
    [SerializeField] private GameObject gearDropPrefab;
    [SerializeField, Min(0)] private int minGear = 1;
    [SerializeField, Min(0)] private int maxGear = 1;
    [SerializeField] private GameObject deathVfxPrefab;

    [Header("debug")]
    public bool destroyOnDeath = true;

    // existing events you already used
    public static Action OnEnemyDied;

    // ---- IHealth implementation ----
    public float Max => baseMaxHealth;
    public float Current => currentHealth;
    public event Action<float, float> OnHealthChanged;

    private bool _dead;

    private void Awake()
    {
        if (!CompareTag("Enemy")) gameObject.tag = "Enemy";
        ResetHealth();
    }

    private void OnEnable()
    {
        // ensure health bars refresh if the enemy is pooled & re-enabled
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    public void ResetHealth()
    {
        _dead = false;
        currentHealth = Mathf.Max(1f, baseMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>Set absolute new max health (used by wave scaling).</summary>
    public void SetScaled(float absoluteMaxHealth)
    {
        baseMaxHealth = Mathf.Max(1f, absoluteMaxHealth);
        currentHealth = baseMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>Multiply current max health by scale (legacy helper).</summary>
    public void SetHealthScale(float scale)
    {
        scale = Mathf.Max(0.01f, scale);
        baseMaxHealth = Mathf.Max(1f, baseMaxHealth * scale);
        currentHealth = baseMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>Returns true if this hit killed the enemy.</summary>
    public bool TakeDamage(float rawDamage)
    {
        if (_dead) return false;

        float dmg = Mathf.Max(0f, rawDamage - armor);
        if (dmg <= 0f) return false;

        currentHealth -= dmg;
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

    public void Heal(float amount)
    {
        if (_dead) return;
        float prev = currentHealth;
        currentHealth = Mathf.Min(baseMaxHealth, currentHealth + Mathf.Max(0f, amount));
        if (!Mathf.Approximately(prev, currentHealth))
            OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    private void Die()
    {
        if (_dead) return;
        _dead = true;

        // fire public events
        try { OnEnemyDied?.Invoke(); } catch { }
        try { GameEvents.OnEnemyKilled?.Invoke(1); } catch { }

        // vfx
        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        // drops
        if (gearDropPrefab != null && (minGear > 0 || maxGear > 0))
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
        // keep inspector preview line in gizmos accurate while editing
        if (!Application.isPlaying)
            currentHealth = Mathf.Clamp(currentHealth, 0f, baseMaxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 a = transform.position + new Vector3(-0.4f, 0.6f, 0f);
        Vector3 b = transform.position + new Vector3(0.4f, 0.6f, 0f);
        Gizmos.DrawLine(a, b);

        float t = Application.isPlaying ? Mathf.Clamp01(currentHealth / Mathf.Max(1f, baseMaxHealth)) : 1f;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(a, Vector3.Lerp(a, b, t));
    }
#endif
}
public sealed class ReadOnlyFieldAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
public class ReadOnlyFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool prev = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = prev;
    }
}
#endif
