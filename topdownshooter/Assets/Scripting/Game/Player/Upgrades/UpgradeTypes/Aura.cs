using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Aura : MonoBehaviour
{
    [Header("Aura Settings")]
    public float radius = 2.5f;
    public float damage = 10f;
    public float damageInterval = 0.5f; // pulse rate in seconds

    private float _nextDamageTime;
    private CircleCollider2D _collider;
    private ParticleSystem _particles;

    // buffer for detected colliders (tweak size if needed)
    private readonly Collider2D[] _hits = new Collider2D[50];

    // pulse animation timing
    private float _pulseScaleTime;
    private const float _pulseDuration = 0.15f;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;

        _particles = GetComponentInChildren<ParticleSystem>(true);
        UpdateAuraVisuals();
    }

    private void OnEnable()
    {
        _nextDamageTime = 0f;
        UpdateAuraVisuals();
    }

    private void Update()
    {
        // always sync collider + visual radius
        _collider.radius = radius;

        if (_particles != null)
        {
            var shape = _particles.shape;
            shape.radius = radius;
        }

        // pulse tick
        if (Time.time >= _nextDamageTime)
        {
            DamageAllInside();
            _nextDamageTime = Time.time + damageInterval;
        }

        // scale pulse animation
        if (_pulseScaleTime > 0f)
        {
            _pulseScaleTime -= Time.deltaTime;
            float t = 1f - (_pulseScaleTime / _pulseDuration);
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f; // pop effect
            transform.localScale = new Vector3(scale, scale, 1f);
        }
        else if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
        }
    }

    private void DamageAllInside()
    {
        // fill buffer without allocating new array
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _hits);

        bool hitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _hits[i];
            if (hit == null) continue;

            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    hitSomething = true;
                }
            }

            // cleanup ref for safety
            _hits[i] = null;
        }

        // only trigger visuals if something was hit
        if (hitSomething)
            DoPulseEffect();
    }

    private void DoPulseEffect()
    {
        // particle burst
        if (_particles != null)
            _particles.Emit(20);

        // scale pop
        _pulseScaleTime = _pulseDuration;
    }

    private void UpdateAuraVisuals()
    {
        if (_particles != null)
        {
            var shape = _particles.shape;
            shape.radius = radius;

            var emission = _particles.emission;
            emission.rateOverTime = 20;

            _particles.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
