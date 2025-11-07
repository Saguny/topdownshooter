using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    [Min(0f)] public float damagePerTick = 5f;
    [Min(0.05f)] public float tickInterval = 0.5f;

    [Header("Filters")]
    [Tooltip("Only damage objects with this tag.")]
    public string playerTag = "Player";
    [Tooltip("Optional: restrict to this layer mask.")]
    public LayerMask playerLayers;

    // internal
    private float _nextTickTime = 0f;
    private bool _touchingPlayer = false;
    private PlayerHealth _cachedPlayerHealth;

    private void OnEnable()
    {
        _nextTickTime = 0f;
        _touchingPlayer = false;
        _cachedPlayerHealth = null;
    }

    private void Update()
    {
        if (!_touchingPlayer || _cachedPlayerHealth == null) return;

        if (Time.time >= _nextTickTime)
        {
            // apply damage; PlayerHealth already handles invulnerability if you set it there
            _cachedPlayerHealth.TakeDamage(damagePerTick);
            _nextTickTime = Time.time + tickInterval;
        }
    }

    // ---------- Trigger path ----------
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryBeginTouch(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // keep touch alive (covers brief physics separations)
        if (_touchingPlayer && _cachedPlayerHealth != null) return;
        TryBeginTouch(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryEndTouch(other);
    }

    // ---------- Collision (non-trigger) path ----------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBeginTouch(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_touchingPlayer && _cachedPlayerHealth != null) return;
        TryBeginTouch(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TryEndTouch(collision.collider);
    }

    // ---------- Helpers ----------
    private void TryBeginTouch(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // cache PlayerHealth once
        if (_cachedPlayerHealth == null)
        {
            // playerHealth might be on root while collider is on a child
            if (!other.TryGetComponent(out _cachedPlayerHealth))
                _cachedPlayerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (_cachedPlayerHealth == null) return;

        _touchingPlayer = true;

        // if we just started touching, allow an immediate tick
        if (Time.time >= _nextTickTime)
            _nextTickTime = Time.time; // tick will happen in Update() this frame
    }

    private void TryEndTouch(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // only end touch when the exiting collider belongs to the same player we cached
        var ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null && ph == _cachedPlayerHealth)
        {
            _touchingPlayer = false;
            _nextTickTime = 0f; // reset, so next touch can tick immediately
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        if (!other || !other.gameObject.activeInHierarchy) return false;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return false;
        if (playerLayers.value != 0 && (playerLayers.value & (1 << other.gameObject.layer)) == 0) return false;
        return true;
    }
}
