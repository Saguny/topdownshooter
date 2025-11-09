using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private BoxCollider2D[] mapBounds = new BoxCollider2D[4];
    [SerializeField] private DifficultyCurve curve;
    [SerializeField] private List<EnemyArchetype> pool;
    [SerializeField] private float desiredPerScreen = 12f;
    [SerializeField] private float budgetPerSecond = 10f;
    [SerializeField] private float spawnOffset = 2f;
    [SerializeField] private List<EnemyArchetype> chunkyPool;
    [SerializeField] private float finalRushHealthMul = 2f;
    [SerializeField] private float spawnrateIncreasePerWave = 2f;
    [SerializeField] private float offscreenBand = 1.0f;
    [SerializeField] private float boundsInset = 0.25f;
    [SerializeField] private float pauseAfterClear = 2f;
    [SerializeField] private float purgeStepDelay = 0.02f;

    [SerializeField] private float startingBudget = 3f;

    private float timeElapsed;
    private float budget;
    private float spawnCooldown;
    private bool finalRush;
    private float spawnPausedUntil;

    private void OnEnable()
    {
        GameEvents.OnFinalRushStarted += HandleFinalRushStart;
        GameEvents.OnFinalRushEnded += HandleFinalRushEnd;
        GameEvents.OnWaveCleared += HandleWaveCleared;
        GameEvents.OnPurgeEnemiesWithFx += HandlePurge;
    }

    private void OnDisable()
    {
        GameEvents.OnFinalRushStarted -= HandleFinalRushStart;
        GameEvents.OnFinalRushEnded -= HandleFinalRushEnd;
        GameEvents.OnWaveCleared -= HandleWaveCleared;
        GameEvents.OnPurgeEnemiesWithFx -= HandlePurge;
    }

    private void Start()
    {
        budget = startingBudget;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (Time.time < spawnPausedUntil) return;

        float densityScale = curve != null ? curve.DensityAt(timeElapsed) : 1f;
        float target = desiredPerScreen * (finalRush ? densityScale * 1.2f : densityScale);
        int alive = CountAlive();

        budget += budgetPerSecond * Time.deltaTime * (finalRush ? densityScale * 1.2f : densityScale);

        int maxSpawnsPerFrame = 3;
        int spawnsThisFrame = 0;
        spawnCooldown -= Time.deltaTime;

        while (spawnsThisFrame < maxSpawnsPerFrame &&
               budget >= 1f &&
               alive < target &&
               spawnCooldown <= 0f)
        {
            var arch = PickEnemy(finalRush);
            if (arch == null || arch.prefab == null) break;
            if (budget < arch.cost) break;

            Spawn(arch, finalRush);
            budget -= arch.cost;
            alive++;
            spawnsThisFrame++;
            spawnCooldown = Random.Range(0.05f, 0.15f);
        }
    }

    private EnemyArchetype PickEnemy(bool rush)
    {
        var src = (!rush || chunkyPool == null || chunkyPool.Count == 0) ? pool : chunkyPool;
        if (src == null || src.Count == 0) return null;
        float sum = 0f; foreach (var p in src) sum += Mathf.Max(0.0001f, p.weight);
        float r = Random.value * sum, acc = 0f;
        foreach (var p in src)
        {
            acc += Mathf.Max(0.0001f, p.weight);
            if (r <= acc) return p;
        }
        return src[src.Count - 1];
    }

    private void Spawn(EnemyArchetype arch, bool rush)
    {
        Vector2 pos = GetSpawnPositionNearOffscreenInsideBounds();
        var go = Instantiate(arch.prefab, pos, Quaternion.identity);

        if (go.TryGetComponent(out EnemyHealth h))
        {
            float hpMul = curve != null ? curve.HealthAt(timeElapsed) : 1f;
            if (rush) hpMul *= finalRushHealthMul;
            h.SetScaled(arch.baseHealth * hpMul);
        }

        if (go.TryGetComponent(out EnemyMovement m))
            m.SetSpeedMultiplier((curve != null ? curve.SpeedAt(timeElapsed) : 1f) * arch.baseSpeed);

        if (go.TryGetComponent(out EnemyDamage d))
            d.SetDamageMultiplier((curve != null ? curve.DamageAt(timeElapsed) : 1f) * arch.baseDamage);

        if (go.TryGetComponent(out EnemyContactDamage contact))
        {
            // pull tickrate from archetype
            contact.tickInterval = arch.contactTickInterval;

            // calculate per-tick damage based on curve and tickrate
            float dps = (curve != null ? curve.DamageAt(timeElapsed) : 1f) * arch.baseDamage;
            contact.damagePerTick = dps * contact.tickInterval;
        }

    }

    private int CountAlive()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    private Rect GetPlayRectFromBorders()
    {
        if (mapBounds == null || mapBounds.Length == 0) return new Rect(-9999, -9999, 19998, 19998);

        Vector2 centroid = Vector2.zero;
        int c = 0;
        foreach (var b in mapBounds) { if (!b) continue; centroid += (Vector2)b.bounds.center; c++; }
        if (c == 0) return new Rect(-9999, -9999, 19998, 19998);
        centroid /= c;

        float innerLeft = float.NegativeInfinity;
        float innerRight = float.PositiveInfinity;
        float innerBottom = float.NegativeInfinity;
        float innerTop = float.PositiveInfinity;

        foreach (var b in mapBounds)
        {
            if (!b) continue;
            var bo = b.bounds;
            Vector2 d = (Vector2)bo.center - centroid;
            if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            {
                if (d.x < 0f) innerLeft = Mathf.Max(innerLeft, bo.max.x);
                else innerRight = Mathf.Min(innerRight, bo.min.x);
            }
            else
            {
                if (d.y < 0f) innerBottom = Mathf.Max(innerBottom, bo.max.y);
                else innerTop = Mathf.Min(innerTop, bo.min.y);
            }
        }

        if (!float.IsFinite(innerLeft) || !float.IsFinite(innerRight) || !float.IsFinite(innerBottom) || !float.IsFinite(innerTop) || innerRight <= innerLeft || innerTop <= innerBottom)
            return new Rect(-9999, -9999, 19998, 19998);

        Rect r = new Rect(innerLeft, innerBottom, innerRight - innerLeft, innerTop - innerBottom);
        r.xMin += boundsInset; r.xMax -= boundsInset; r.yMin += boundsInset; r.yMax -= boundsInset;
        return r;
    }

    private Vector2 GetSpawnPositionNearOffscreenInsideBounds()
    {
        Rect play = GetPlayRectFromBorders();

        float h = mainCamera.orthographicSize;
        float w = h * mainCamera.aspect;

        float cx = mainCamera.transform.position.x;
        float cy = mainCamera.transform.position.y;

        float camLeft = cx - w;
        float camRight = cx + w;
        float camBottom = cy - h;
        float camTop = cy + h;

        float pad = 0.01f; // tiny nudge to ensure "off-screen"

        // build four thin bands *inside* play rect but just outside camera view
        float leftX0 = Mathf.Max(play.xMin, camLeft - offscreenBand);
        float leftX1 = Mathf.Min(play.xMax, camLeft - pad);
        float leftY0 = Mathf.Max(play.yMin, camBottom);
        float leftY1 = Mathf.Min(play.yMax, camTop);

        float rightX0 = Mathf.Max(play.xMin, camRight + pad);
        float rightX1 = Mathf.Min(play.xMax, camRight + offscreenBand);
        float rightY0 = Mathf.Max(play.yMin, camBottom);
        float rightY1 = Mathf.Min(play.yMax, camTop);

        float botX0 = Mathf.Max(play.xMin, camLeft);
        float botX1 = Mathf.Min(play.xMax, camRight);
        float botY0 = Mathf.Max(play.yMin, camBottom - offscreenBand);
        float botY1 = Mathf.Min(play.yMax, camBottom - pad);

        float topX0 = Mathf.Max(play.xMin, camLeft);
        float topX1 = Mathf.Min(play.xMax, camRight);
        float topY0 = Mathf.Max(play.yMin, camTop + pad);
        float topY1 = Mathf.Min(play.yMax, camTop + offscreenBand);

        // collect valid bands with their areas (note: fixed top width: topX1 - topX0)
        var rects = new List<(Rect r, float area)>();
        if (leftX1 > leftX0 && leftY1 > leftY0) rects.Add((new Rect(leftX0, leftY0, leftX1 - leftX0, leftY1 - leftY0), (leftX1 - leftX0) * (leftY1 - leftY0)));
        if (rightX1 > rightX0 && rightY1 > rightY0) rects.Add((new Rect(rightX0, rightY0, rightX1 - rightX0, rightY1 - rightY0), (rightX1 - rightX0) * (rightY1 - rightY0)));
        if (botX1 > botX0 && botY1 > botY0) rects.Add((new Rect(botX0, botY0, botX1 - botX0, botY1 - botY0), (botX1 - botX0) * (botY1 - botY0)));
        if (topX1 > topX0 && topY1 > topY0) rects.Add((new Rect(topX0, topY0, topX1 - topX0, topY1 - topY0), (topX1 - topX0) * (topY1 - topY0)));

        // normal path: weighted pick among valid off-screen bands (still inside play rect)
        if (rects.Count > 0)
        {
            float total = 0f; foreach (var it in rects) total += it.area;
            float pick = Random.value * total;
            float acc = 0f;
            foreach (var it in rects)
            {
                acc += it.area;
                if (pick <= acc)
                {
                    float rx = Random.Range(it.r.xMin, it.r.xMax);
                    float ry = Random.Range(it.r.yMin, it.r.yMax);
                    return new Vector2(rx, ry);
                }
            }
            var last = rects[rects.Count - 1].r;
            return new Vector2(Random.Range(last.xMin, last.xMax), Random.Range(last.yMin, last.yMax));
        }

        // fallback: camera covers (almost) entire play rect or bands collapsed.
        // choose the side with the most room *inside* the bounds and spawn just beyond the camera edge.
        float gapLeft = Mathf.Max(0f, camLeft - play.xMin);
        float gapRight = Mathf.Max(0f, play.xMax - camRight);
        float gapTop = Mathf.Max(0f, play.yMax - camTop);
        float gapBottom = Mathf.Max(0f, camBottom - play.yMin);

        // pick the side with max gap; if all zero, just pick a corner just beyond camera by pad but clamped inside play
        int side = 0; // 0=L,1=R,2=T,3=B
        float best = gapLeft; side = 0;
        if (gapRight > best) { best = gapRight; side = 1; }
        if (gapTop > best) { best = gapTop; side = 2; }
        if (gapBottom > best) { best = gapBottom; side = 3; }

        float x, y;
        switch (side)
        {
            case 0: // left
                x = Mathf.Clamp(camLeft - pad, play.xMin, play.xMax);
                y = Random.Range(Mathf.Max(play.yMin, camBottom), Mathf.Min(play.yMax, camTop));
                break;
            case 1: // right
                x = Mathf.Clamp(camRight + pad, play.xMin, play.xMax);
                y = Random.Range(Mathf.Max(play.yMin, camBottom), Mathf.Min(play.yMax, camTop));
                break;
            case 2: // top
                y = Mathf.Clamp(camTop + pad, play.yMin, play.yMax);
                x = Random.Range(Mathf.Max(play.xMin, camLeft), Mathf.Min(play.xMax, camRight));
                break;
            case 3: // bottom
            default:
                y = Mathf.Clamp(camBottom - pad, play.yMin, play.yMax);
                x = Random.Range(Mathf.Max(play.xMin, camLeft), Mathf.Min(play.xMax, camRight));
                break;
        }

        // final clamp to ensure we're strictly inside the playable rect
        x = Mathf.Clamp(x, play.xMin, play.xMax);
        y = Mathf.Clamp(y, play.yMin, play.yMax);

        // and make sure we are not accidentally on-screen due to precision — nudge once more if needed
        if (x > camLeft && x < camRight && y > camBottom && y < camTop)
        {
            if (side == 0) x = Mathf.Max(play.xMin, camLeft - pad);
            else if (side == 1) x = Mathf.Min(play.xMax, camRight + pad);
            else if (side == 2) y = Mathf.Min(play.yMax, camTop + pad);
            else y = Mathf.Max(play.yMin, camBottom - pad);
        }

        return new Vector2(x, y);
    }


    private void HandleFinalRushStart(int wave, int quota) { finalRush = true; }
    private void HandleFinalRushEnd(int wave) { finalRush = false; spawnPausedUntil = Time.time + pauseAfterClear; }
    private void HandleWaveCleared(int wave) { budgetPerSecond += spawnrateIncreasePerWave; }
    private void HandlePurge(GameObject fx) { StartCoroutine(PurgeInsideOut(fx)); }

    private IEnumerator PurgeInsideOut(GameObject fx)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) yield break;

        var enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        enemies.Sort((a, b) =>
        {
            float da = (a ? (a.transform.position - player.transform.position).sqrMagnitude : float.MaxValue);
            float db = (b ? (b.transform.position - player.transform.position).sqrMagnitude : float.MaxValue);
            return da.CompareTo(db);
        });

        foreach (var e in enemies)
        {
            if (!e) continue;
            if (fx) Instantiate(fx, e.transform.position, Quaternion.identity);
            Destroy(e);
            yield return new WaitForSeconds(purgeStepDelay);
        }
    }
}
