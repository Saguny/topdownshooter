using UnityEngine;

public class AutoAimService : MonoBehaviour
{
    [SerializeField] private float searchRadius = 12f;
    [SerializeField] private LayerMask enemyMask;
    private readonly Collider2D[] _buf = new Collider2D[64];

    [System.Obsolete]
    public Transform FindTarget(Vector2 origin)
    {
        int n = Physics2D.OverlapCircleNonAlloc(origin, searchRadius, _buf, enemyMask);

        float best = float.MaxValue;
        Transform t = null;

        for (int i = 0; i < n; i++)
        {
            var c = _buf[i];
            if (c == null) continue;

            float d = (c.transform.position - (Vector3)origin).sqrMagnitude;
            if (d < best)
            {
                best = d;
                t = c.transform;
            }
        }

        return t;
    }
}
