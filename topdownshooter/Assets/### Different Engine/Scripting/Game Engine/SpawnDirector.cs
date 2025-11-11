using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Fast Phase")]
    [SerializeField] private EnemyArchetype fastPhaseArchetype;
    [SerializeField] private float fastPhaseDuration = 10f;
    [SerializeField] private float fastPhaseCheckInterval = 10f;
    [SerializeField, Range(0f, 1f)] private float fastPhaseChance = 0.01f;
    [SerializeField] private float fastPhaseSpawnrateMul = 3f;
    [SerializeField] private float fastPhaseDensityMul = 3f;
    [SerializeField] private float fastPhaseMinRunTime = 120f;
    [SerializeField] private float fastPhasePostClearDelay = 1f;

    [Header("Boss")]
    [SerializeField] private EnemyArchetype bossArchetype;

    [Header("Final Rush UI")]
    [SerializeField] private Slider finalRushSlider;

    private float fastPhaseUntil = -1f;
    private float nextFastPhaseCheck;
    private float timeElapsed;
    private float budget;
    private float spawnCooldown;
    private bool finalRush;
    private float spawnPausedUntil;
    private float runTime;
    private bool preparingFastPhase;
    private bool fastPhaseSequenceRunning;

    private int currentWave;
    private int bossesSpawnedThisRush;
    private List<GameObject> activeBosses = new List<GameObject>();

    private int finalRushQuota;
    private int finalRushKills;
    private bool finalRushEndTriggered;

    private void OnEnable()
    {
        GameEvents.OnFinalRushStarted += HandleFinalRushStart;
        GameEvents.OnFinalRushEnded += HandleFinalRushEnd;
        GameEvents.OnWaveCleared += HandleWaveCleared;
        GameEvents.OnPurgeEnemiesWithFx += HandlePurge;
        GameEvents.OnRunTimeChanged += HandleRunTimeChanged;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        GameEvents.OnFinalRushStarted -= HandleFinalRushStart;
        GameEvents.OnFinalRushEnded -= HandleFinalRushEnd;
        GameEvents.OnWaveCleared -= HandleWaveCleared;
        GameEvents.OnPurgeEnemiesWithFx -= HandlePurge;
        GameEvents.OnRunTimeChanged -= HandleRunTimeChanged;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        budget = startingBudget;
        nextFastPhaseCheck = Time.time + fastPhaseCheckInterval;
        UpdateFinalRushProgressBar();
    }

    private void HandleRunTimeChanged(float value)
    {
        runTime = value;
    }

    private void StartPreparingFastPhase()
    {
        preparingFastPhase = true;
        spawnCooldown = 0f;
    }

    private IEnumerator BeginFastPhaseAfterDelay()
    {
        fastPhaseSequenceRunning = true;
        yield return new WaitForSeconds(fastPhasePostClearDelay);
        preparingFastPhase = false;
        StartFastPhase(fastPhaseDuration);
        fastPhaseSequenceRunning = false;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (Time.time < spawnPausedUntil) return;

        bool inFastPhase = Time.time < fastPhaseUntil;

        if (!preparingFastPhase &&
            !inFastPhase &&
            runTime >= fastPhaseMinRunTime &&
            Time.time >= nextFastPhaseCheck)
        {
            nextFastPhaseCheck = Time.time + fastPhaseCheckInterval;
            if (Random.value < fastPhaseChance)
            {
                StartPreparingFastPhase();
            }
        }

        float densityScale = curve != null ? curve.DensityAt(timeElapsed) : 1f;
        float rushScale = finalRush ? densityScale * 1.2f : densityScale;

        int alive = CountAlive();

        if (preparingFastPhase)
        {
            if (alive <= 0 && !fastPhaseSequenceRunning)
            {
                StartCoroutine(BeginFastPhaseAfterDelay());
            }
            return;
        }

        float spawnrateMul = inFastPhase ? fastPhaseSpawnrateMul : 1f;
        float densityMul = inFastPhase ? fastPhaseDensityMul : 1f;
        float target = desiredPerScreen * densityMul * rushScale;

        budget += budgetPerSecond * spawnrateMul * Time.deltaTime * rushScale;

        int baseMaxSpawnsPerFrame = 3;
        int maxSpawnsPerFrame = inFastPhase ? baseMaxSpawnsPerFrame * 2 : baseMaxSpawnsPerFrame;

        int spawnsThisFrame = 0;
        spawnCooldown -= Time.deltaTime;

        while (spawnsThisFrame < maxSpawnsPerFrame &&
               budget >= 1f &&
               alive < target &&
               spawnCooldown <= 0f)
        {
            EnemyArchetype arch;

            if (finalRush && bossArchetype != null && bossArchetype.prefab != null)
            {
                int maxBosses = GetMaxBossCountForWave(currentWave);
                int aliveBosses = GetAliveBossCount();

                if (aliveBosses < maxBosses && bossesSpawnedThisRush < maxBosses)
                {
                    arch = bossArchetype;
                }
                else if (inFastPhase && fastPhaseArchetype != null)
                {
                    arch = fastPhaseArchetype;
                }
                else
                {
                    arch = PickEnemy(finalRush);
                }
            }
            else
            {
                if (inFastPhase && fastPhaseArchetype != null)
                {
                    arch = fastPhaseArchetype;
                }
                else
                {
                    arch = PickEnemy(finalRush);
                }
            }

            if (arch == null || arch.prefab == null) break;
            if (budget < arch.cost) break;

            var go = Spawn(arch, finalRush);
            if (go == null) break;

            budget -= arch.cost;
            alive++;
            spawnsThisFrame++;

            if (arch == bossArchetype)
            {
                activeBosses.Add(go);
                bossesSpawnedThisRush++;
            }

            float baseMinCd = 0.05f;
            float baseMaxCd = 0.15f;
            float cd = Random.Range(baseMinCd, baseMaxCd) / spawnrateMul;
            spawnCooldown = cd;
        }

        if (finalRush && !finalRushEndTriggered && finalRushQuota > 0)
        {
            bool bossAlive = IsBossAlive();
            bool quotaReached = finalRushKills >= finalRushQuota;

            if (!bossAlive && quotaReached)
            {
                finalRushEndTriggered = true;
                GameEvents.OnFinalRushEnded?.Invoke(currentWave);
            }
        }
    }

    public void StartFastPhase(float duration)
    {
        fastPhaseUntil = Time.time + duration;
        spawnCooldown = 0f;
    }

    private EnemyArchetype PickEnemy(bool rush)
    {
        var src = (!rush || chunkyPool == null || chunkyPool.Count == 0) ? pool : chunkyPool;
        if (src == null || src.Count == 0) return null;
        float sum = 0f;
        foreach (var p in src) sum += Mathf.Max(0.0001f, p.weight);
        float r = Random.value * sum;
        float acc = 0f;
        foreach (var p in src)
        {
            acc += Mathf.Max(0.0001f, p.weight);
            if (r <= acc) return p;
        }
        return src[src.Count - 1];
    }

    private GameObject Spawn(EnemyArchetype arch, bool rush)
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
            contact.tickInterval = arch.contactTickInterval;
            float dps = (curve != null ? curve.DamageAt(timeElapsed) : 1f) * arch.baseDamage;
            contact.damagePerTick = dps * contact.tickInterval;
        }

        return go;
    }

    private int CountAlive()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    private int GetMaxBossCountForWave(int wave)
    {
        if (wave >= 5) return 4;
        if (wave >= 3) return 2;
        return 1;
    }

    private int GetAliveBossCount()
    {
        activeBosses.RemoveAll(b => b == null);
        return activeBosses.Count;
    }

    public bool HasAliveBosses()
    {
        return GetAliveBossesByMarker() > 0;
    }

    private int GetAliveBossesByMarker()
    {
        int count = 0;
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (e && e.GetComponent<BossMarker>() != null)
                count++;
        }
        return count;
    }

    private bool IsBossAlive()
    {
        return GetAliveBossCount() > 0;
    }

    private Rect GetPlayRectFromBorders()
    {
        if (mapBounds == null || mapBounds.Length == 0) return new Rect(-9999, -9999, 19998, 19998);

        Vector2 centroid = Vector2.zero;
        int c = 0;
        foreach (var b in mapBounds)
        {
            if (!b) continue;
            centroid += (Vector2)b.bounds.center;
            c++;
        }
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
        r.xMin += boundsInset;
        r.xMax -= boundsInset;
        r.yMin += boundsInset;
        r.yMax -= boundsInset;
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

        float pad = 0.01f;

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

        var rects = new List<(Rect r, float area)>();
        if (leftX1 > leftX0 && leftY1 > leftY0) rects.Add((new Rect(leftX0, leftY0, leftX1 - leftX0, leftY1 - leftY0), (leftX1 - leftX0) * (leftY1 - leftY0)));
        if (rightX1 > rightX0 && rightY1 > rightY0) rects.Add((new Rect(rightX0, rightY0, rightX1 - rightX0, rightY1 - rightY0), (rightX1 - rightX0) * (rightY1 - rightY0)));
        if (botX1 > botX0 && botY1 > botY0) rects.Add((new Rect(botX0, botY0, botX1 - botX0, botY1 - botY0), (botX1 - botX0) * (botY1 - botY0)));
        if (topX1 > topX0 && topY1 > topY0) rects.Add((new Rect(topX0, topY0, topX1 - topX0, topY1 - topY0), (topX1 - topX0) * (topY1 - topY0)));

        if (rects.Count > 0)
        {
            float total = 0f;
            foreach (var it in rects) total += it.area;
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

        float gapLeft = Mathf.Max(0f, camLeft - play.xMin);
        float gapRight = Mathf.Max(0f, play.xMax - camRight);
        float gapTop = Mathf.Max(0f, play.yMax - camTop);
        float gapBottom = Mathf.Max(0f, camBottom - play.yMin);

        int side = 0;
        float best = gapLeft;
        side = 0;
        if (gapRight > best) { best = gapRight; side = 1; }
        if (gapTop > best) { best = gapTop; side = 2; }
        if (gapBottom > best) { best = gapBottom; side = 3; }

        float x;
        float y;
        switch (side)
        {
            case 0:
                x = Mathf.Clamp(camLeft - pad, play.xMin, play.xMax);
                y = Random.Range(Mathf.Max(play.yMin, camBottom), Mathf.Min(play.yMax, camTop));
                break;
            case 1:
                x = Mathf.Clamp(camRight + pad, play.xMin, play.xMax);
                y = Random.Range(Mathf.Max(play.yMin, camBottom), Mathf.Min(play.yMax, camTop));
                break;
            case 2:
                y = Mathf.Clamp(camTop + pad, play.yMin, play.yMax);
                x = Random.Range(Mathf.Max(play.xMin, camLeft), Mathf.Min(play.xMax, camRight));
                break;
            case 3:
            default:
                y = Mathf.Clamp(camBottom - pad, play.yMin, play.yMax);
                x = Random.Range(Mathf.Max(play.xMin, camLeft), Mathf.Min(play.xMax, camRight));
                break;
        }

        x = Mathf.Clamp(x, play.xMin, play.xMax);
        y = Mathf.Clamp(y, play.yMin, play.yMax);

        if (x > camLeft && x < camRight && y > camBottom && y < camTop)
        {
            if (side == 0) x = Mathf.Max(play.xMin, camLeft - pad);
            else if (side == 1) x = Mathf.Min(play.xMax, camRight + pad);
            else if (side == 2) y = Mathf.Min(play.yMax, camTop + pad);
            else y = Mathf.Max(play.yMin, camBottom - pad);
        }

        return new Vector2(x, y);
    }

    private void HandleFinalRushStart(int wave, int quota)
    {
        finalRush = true;
        currentWave = wave;
        bossesSpawnedThisRush = 0;
        activeBosses.Clear();

        finalRushQuota = Mathf.Max(1, quota);
        finalRushEndTriggered = false;
        ResetFinalRushProgress();

        TrySpawnInitialBoss();
    }

    private void HandleFinalRushEnd(int wave)
    {
        finalRush = false;
        spawnPausedUntil = Time.time + pauseAfterClear;
        activeBosses.Clear();

        finalRushQuota = 0;
        finalRushEndTriggered = true;
        UpdateFinalRushProgressBar();
    }

    private void TrySpawnInitialBoss()
    {
        if (bossArchetype == null || bossArchetype.prefab == null) return;

        int maxBosses = GetMaxBossCountForWave(currentWave);
        if (GetAliveBossesByMarker() >= maxBosses) return;

        var go = Spawn(bossArchetype, true);
        if (go != null)
        {
            activeBosses.Add(go);
            bossesSpawnedThisRush++;
            budget = Mathf.Max(0f, budget - bossArchetype.cost);
        }
    }

    private void HandleWaveCleared(int wave)
    {
        budgetPerSecond += spawnrateIncreasePerWave;
    }

    private void HandlePurge(GameObject fx)
    {
        StartCoroutine(PurgeInsideOut(fx));
    }

    private void HandleEnemyKilled(int id)
    {
        if (!finalRush || finalRushQuota <= 0) return;

        finalRushKills = Mathf.Min(finalRushKills + 1, finalRushQuota);
        UpdateFinalRushProgressBar();
    }

    private void ResetFinalRushProgress()
    {
        finalRushKills = 0;
        UpdateFinalRushProgressBar();
    }

    private void UpdateFinalRushProgressBar()
    {
        if (!finalRushSlider) return;

        if (!finalRush || finalRushQuota <= 0)
        {
            finalRushSlider.gameObject.SetActive(false);
            return;
        }

        finalRushSlider.gameObject.SetActive(true);

        finalRushSlider.minValue = 0f;
        finalRushSlider.maxValue = finalRushQuota;
        finalRushSlider.value = finalRushKills;
    }

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

            var eh = e.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                float lethalDamage = eh.Current + eh.Max + 1f;
                eh.TakeDamage(lethalDamage);
            }
            else
            {
                Destroy(e);
            }

            yield return new WaitForSeconds(purgeStepDelay);
        }
    }
}
