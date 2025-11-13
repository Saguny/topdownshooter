using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 4f;

    [Header("Damage")]
    public float damage = 20f;
    public float radius = 2f;

    [Header("VFX")]
    public GameObject impactEffectPrefab;
    public float impactEffectDuration = 0.5f;
    public float impactVisualScale = 1f;

    [Header("Visual")]
    public float spriteAngleOffset = 0f;

    private Vector3 _impactPosition;
    private Vector2 _direction;
    private bool _initialized;

    public void Setup(float damageAmount, float radius, Vector3 impactPosition, Vector2 direction)
    {
        damage = damageAmount;
        this.radius = radius;
        _impactPosition = impactPosition;
        _direction = direction.normalized;
        _initialized = true;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + spriteAngleOffset, Vector3.forward);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!_initialized) return;

        float distanceThisFrame = speed * Time.deltaTime;
        transform.position += (Vector3)(_direction * distanceThisFrame);

        Vector2 toImpact = _impactPosition - transform.position;
        if (Vector2.Dot(toImpact, _direction) <= 0f || toImpact.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
                enemy.TakeDamage(damage);
        }

        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * impactVisualScale;
            Destroy(fx, impactEffectDuration);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
