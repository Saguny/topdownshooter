using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class Aura : MonoBehaviour
{
    [Header("Gameplay")]
    public float radius = 2.5f;
    public float damage = 10f;
    public float damageInterval = 0.5f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string spawnAnimName = "Aura_Spawn";
    [SerializeField] private string loopAnimName = "Aura_Loop";

    private CircleCollider2D _collider;
    private float _nextDamageTime;
    private readonly Collider2D[] _hits = new Collider2D[16];
    private readonly HashSet<int> _hitThisPulse = new HashSet<int>();
    [SerializeField] private float _visualScaleBase = 1f;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
        if (!animator) animator = GetComponentInChildren<Animator>(true);
    }

    private void OnEnable()
    {
        _nextDamageTime = 0f;
        PlaySpawnOnce();
    }

    private void Update()
    {
        _collider.radius = radius;

        if (Time.time >= _nextDamageTime)
        {
            DamageWithinRadius();
            _nextDamageTime = Time.time + damageInterval;
        }

        if (animator == null || animator.runtimeAnimatorController == null)
            transform.localScale = Vector3.one * _visualScaleBase;
        else
            animator.transform.localScale = Vector3.one * _visualScaleBase;
    }

    public void IncreaseVisualScale(float factor)
    {
        _visualScaleBase *= factor;
    }

    public void OnRadiusUpgraded()
    {
        _visualScaleBase *= 1.15f;
        if (animator && !string.IsNullOrEmpty(loopAnimName))
            animator.Play(loopAnimName, 0, 0f);
    }

    private bool DamageWithinRadius()
    {
        _hitThisPulse.Clear();
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _hits, enemyMask);
        bool hitSomething = false;

        for (int i = 0; i < n; i++)
        {
            var c = _hits[i];
            _hits[i] = null;
            if (!c) continue;

            int id = c.GetInstanceID();
            if (_hitThisPulse.Contains(id)) continue;

            var eh = c.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
                _hitThisPulse.Add(id);
                hitSomething = true;
            }
        }

        return hitSomething;
    }

    private void PlaySpawnOnce()
    {
        if (animator && !string.IsNullOrEmpty(spawnAnimName))
            animator.Play(spawnAnimName, 0, 0f);
    }

    private void PlayLoopOnce()
    {
        if (animator && !string.IsNullOrEmpty(loopAnimName))
            animator.Play(loopAnimName, 0, 0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
