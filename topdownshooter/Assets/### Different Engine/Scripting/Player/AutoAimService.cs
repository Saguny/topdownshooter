using UnityEngine;
using System.Collections.Generic;

public class AutoAimService : MonoBehaviour
{
    [SerializeField] private float searchRadius = 12f;
    [SerializeField] private LayerMask enemyMask;
    private readonly Collider2D[] buf = new Collider2D[64];

    public Transform FindTarget(Vector2 origin)
    {
        int n = Physics2D.OverlapCircleNonAlloc(origin, searchRadius, buf, enemyMask);
        float best = float.MaxValue;
        Transform t = null;
        for (int i = 0; i < n; i++)
        {
            var c = buf[i];
            if (!c) continue;
            float d = (c.transform.position - (Vector3)origin).sqrMagnitude;
            if (d < best) { best = d; t = c.transform; }
        }
        return t;
    }

    public List<Transform> FindTargets(Vector2 origin, int maxCount)
    {
        int n = Physics2D.OverlapCircleNonAlloc(origin, searchRadius, buf, enemyMask);
        var list = new List<(Transform t, float d)>(n);
        for (int i = 0; i < n; i++)
        {
            var c = buf[i];
            if (!c) continue;
            float d = (c.transform.position - (Vector3)origin).sqrMagnitude;
            list.Add((c.transform, d));
        }
        list.Sort((a, b) => a.d.CompareTo(b.d));
        var outList = new List<Transform>(Mathf.Max(1, maxCount));
        for (int i = 0; i < list.Count && outList.Count < maxCount; i++)
            outList.Add(list[i].t);
        return outList;
    }
}
