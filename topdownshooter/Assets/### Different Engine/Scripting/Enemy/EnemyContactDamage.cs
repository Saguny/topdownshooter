using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    public float damagePerTick = 5f;
    public float tickInterval = 0.5f;

    private PlayerHealth _cachedPlayerHealth;
    private bool _touchingPlayer;
    private float _nextTickTime;

    private EnemyMovement _movement;
    private Animator _animator;
    private readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        _animator = GetComponent<Animator>();
        if (_animator) _animator.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) { TryBeginTouch(collision.collider); }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_touchingPlayer && _cachedPlayerHealth != null && Time.time >= _nextTickTime)
        {
            _cachedPlayerHealth.TakeDamage(damagePerTick);
            _nextTickTime = Time.time + tickInterval;
        }
    }
    private void OnCollisionExit2D(Collision2D collision) { TryEndTouch(collision.collider); }

    private void TryBeginTouch(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        if (_cachedPlayerHealth == null)
        {
            if (!other.TryGetComponent(out _cachedPlayerHealth))
                _cachedPlayerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        if (_cachedPlayerHealth == null) return;

        _touchingPlayer = true;

        if (_movement) _movement.enabled = false;
        if (_animator && _animator.isActiveAndEnabled) _animator.SetBool(HashIsRunning, false);

        _nextTickTime = Time.time + tickInterval;
    }

    private void TryEndTouch(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _touchingPlayer = false;

        if (_movement) _movement.enabled = true;
        if (_animator && _animator.isActiveAndEnabled) _animator.SetBool(HashIsRunning, true);
    }

    private bool IsPlayer(Collider2D col) { return col.CompareTag("Player"); }
}
