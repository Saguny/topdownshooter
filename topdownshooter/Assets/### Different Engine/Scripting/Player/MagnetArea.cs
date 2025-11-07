using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MagnetArea : MonoBehaviour
{
    [SerializeField] private float baseRadius = 1.5f;

    private CircleCollider2D cc;
    private StatContext stats;

    private void Awake()
    {
        cc = GetComponent<CircleCollider2D>();
        cc.isTrigger = true;
        stats = GetComponentInParent<StatContext>();
    }

    private void Update()
    {
        float mul = stats ? stats.pickupRadiusMul : 1f;
        cc.radius = baseRadius * mul;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Pickup p))
            p.PullTo(transform);
    }
}
