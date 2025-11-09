using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damagePerTick = 5f;
    public float tickInterval = 0.5f;

    private PlayerHealth _cachedPlayerHealth;
    private bool _touchingPlayer;

    private float _nextTickTime;   // when the next tick is allowed
    private float _lastHitTime;    // when we last actually dealt damage

    private EnemyMovement _movement;
    private Animator _animator;
    private readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        _animator = GetComponent<Animator>();

        if (_animator) _animator.enabled = true;

        _lastHitTime = -9999f; // so first contact is always allowed
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBeginTouch(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_touchingPlayer || _cachedPlayerHealth == null)
            return;

        if (Time.time >= _nextTickTime)
        {
            DealDamageTick();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TryEndTouch(collision.collider);
    }

    private void TryBeginTouch(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        if (_cachedPlayerHealth == null)
        {
            if (!other.TryGetComponent(out _cachedPlayerHealth))
                _cachedPlayerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (_cachedPlayerHealth == null)
            return;

        _touchingPlayer = true;

        // stop movement / running anim while in contact
        if (_movement) _movement.enabled = false;
        if (_animator && _animator.isActiveAndEnabled)
            _animator.SetBool(HashIsRunning, false);

        // cooldown check for initial hit:
        // only do an instant hit if the last hit was >= tickInterval ago
        float earliestNextAllowed = _lastHitTime + tickInterval;

        if (Time.time >= earliestNextAllowed)
        {
            // instant hit on first valid contact
            DealDamageTick();
        }
        else
        {
            // still on cooldown: schedule next tick after remaining time
            _nextTickTime = earliestNextAllowed;
        }
    }

    private void TryEndTouch(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        _touchingPlayer = false;

        if (_movement) _movement.enabled = true;
        if (_animator && _animator.isActiveAndEnabled)
            _animator.SetBool(HashIsRunning, true);
    }

    private void DealDamageTick()
    {
        if (_cachedPlayerHealth == null)
            return;

        _cachedPlayerHealth.TakeDamage(damagePerTick);

        _lastHitTime = Time.time;
        _nextTickTime = Time.time + tickInterval;
    }

    private bool IsPlayer(Collider2D col)
    {
        return col.CompareTag("Player");
    }
}
