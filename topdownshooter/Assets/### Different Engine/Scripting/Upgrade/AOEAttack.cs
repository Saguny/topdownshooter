using System.Collections;
using System.Linq;
using UnityEngine;

public class AOEAttack : MonoBehaviour
{
    [Header("AOE Base Settings")]
    public float radius = 3f;
    public float damage = 20f;
    public float interval = 5f;
    public float projectileSpeed = 10f;

    [Header("Visuals")]
    public GameObject projectilePrefab;   // Projektil, das zum Ziel fliegt
    public GameObject aoeEffectPrefab;    // Kreis-Effekt beim Einschlag (halbtransparent weiß)
    public Color aoeColor = new Color(1f, 1f, 1f, 0.5f);

    private bool isActive = false;
    private bool isRunning = false;

    private void OnEnable()
    {
        if (isActive && !isRunning)
            StartCoroutine(AOERoutine());
    }

    public void Activate(float newRadius, float newDamage, float newInterval)
    {
        radius = newRadius;
        damage = newDamage;
        interval = newInterval;

        isActive = true;

        if (!isRunning)
            StartCoroutine(AOERoutine());
    }

    private IEnumerator AOERoutine()
    {
        isRunning = true;

        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (!isActive) continue;

            var enemy = FindClosestEnemy();
            if (enemy != null)
            {
                FireProjectile(enemy.transform);
            }
        }
    }

    private GameObject FindClosestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject closest = enemies
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        return closest;
    }

    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null)
        {
            // Fallback: falls kein Prefab gesetzt ist, fliege einfach instant
            DoBlast(target.position);
            return;
        }

        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var rb = proj.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = proj.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * projectileSpeed;

        // Projektil zerstören und AOE auslösen beim Einschlag
        var hit = proj.AddComponent<AOEProjectile>();
        hit.Setup(this);
    }

    public void DoBlast(Vector3 position)
    {
        // 1️⃣ visueller Effekt
        if (aoeEffectPrefab != null)
        {
            Instantiate(aoeEffectPrefab, position, Quaternion.identity);
        }
        else
        {
            // einfacher Notfallkreis
            var circle = new GameObject("AOE_Circle");
            var sr = circle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = aoeColor;
            sr.sortingOrder = 10;
            circle.transform.position = position;
            circle.transform.localScale = Vector3.one * radius * 2f;
            Destroy(circle, 0.5f);
        }

        // 2️⃣ Schaden an Gegnern
        var hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.TryGetComponent(out EnemyHealth eh))
            {
                eh.TakeDamage(damage);
            }
        }
    }

    // Erstellt einen einfachen weißen Kreis zur Laufzeit
    private Sprite CreateCircleSprite()
    {
        var tex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[64 * 64];
        Vector2 center = new Vector2(32, 32);
        float r = 31f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(center, new Vector2(x, y));
                bool inside = dist <= r;
                pixels[y * 64 + x] = inside ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 1f, 1f, 0f);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
}
