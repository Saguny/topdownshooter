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

    private Vector3 _impactPosition;
    private bool _hasImpactPosition;

    public void Setup(float damageAmount, float radius, Vector3 impactPosition)
    {
        this.damage = damageAmount;
        this.radius = radius;
        _impactPosition = impactPosition;
        _hasImpactPosition = true;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!_hasImpactPosition) return;

        Vector3 dir = _impactPosition - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.position += dir.normalized * distanceThisFrame;
        transform.up = dir.normalized;
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
            fx.transform.localScale = Vector3.one * (radius * 2f);
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
