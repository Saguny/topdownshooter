using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Aura : MonoBehaviour
{
    [Header("Aura Settings")]
    public float radius = 2.5f;
    public float damage = 10f;
    public float damageInterval = 0.5f; // pulse rate in seconds

    [Header("Particle-driven Damage")]
    [SerializeField] private float perParticleHitRadius = 0.25f; // the “size” of each particle’s hit
    [SerializeField] private int maxParticlesToSample = 64;       // cap how many particles we test per tick
    [SerializeField] private LayerMask enemyMask;                  // set to your Enemy layer

    private float _nextDamageTime;
    private CircleCollider2D _collider;
    private ParticleSystem _particles;

    // particle / hit buffers
    private ParticleSystem.Particle[] _particleBuf;
    private readonly Collider2D[] _miniHits = new Collider2D[8];

    // pulse animation timing
    private float _pulseScaleTime;
    private const float _pulseDuration = 0.15f;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;

        _particles = GetComponentInChildren<ParticleSystem>(true);
        _particleBuf = new ParticleSystem.Particle[Mathf.Max(8, maxParticlesToSample)];

        UpdateAuraVisuals();
    }

    private void OnEnable()
    {
        _nextDamageTime = 0f;
        UpdateAuraVisuals();
    }

    private void Update()
    {
        // keep collider + visual radius in sync (visual only; collider is trigger)
        _collider.radius = radius;

        if (_particles != null)
        {
            var shape = _particles.shape;
            shape.radius = radius;
        }

        // pulse tick
        if (Time.time >= _nextDamageTime)
        {
            DamageByParticles();
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

    /// <summary>
    /// Uses alive particle positions as sampling points and applies small 2D overlap checks.
    /// </summary>
    private void DamageByParticles()
    {
        if (_particles == null) return;

        int alive = _particles.GetParticles(_particleBuf);
        if (alive <= 0) return;

        // cap sampling count for perf
        int count = Mathf.Min(alive, maxParticlesToSample);

        // figure out if particle positions are in local or world space
        var main = _particles.main;
        bool isLocal = main.simulationSpace == ParticleSystemSimulationSpace.Local;
        Transform psTransform = _particles.transform;

        bool hitSomething = false;

        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = _particleBuf[i].position;
            if (isLocal) worldPos = psTransform.TransformPoint(worldPos);

            int n = Physics2D.OverlapCircleNonAlloc(worldPos, perParticleHitRadius, _miniHits, enemyMask);

            for (int j = 0; j < n; j++)
            {
                var c = _miniHits[j];
                if (!c) continue;

                // optional: tag check if you want the extra safety
                // if (!c.CompareTag("Enemy")) { _miniHits[j] = null; continue; }

                var eh = c.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage(damage);
                    hitSomething = true;
                }

                _miniHits[j] = null; // clear slot
            }
        }

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
