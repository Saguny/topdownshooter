using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private BoxCollider2D[] mapBounds = new BoxCollider2D[4];
    [SerializeField] private DifficultyCurve curve;
    [SerializeField] private List<EnemyArchetype> pool;

    [Header("Spawn")]
    [SerializeField] private float desiredPerScreen = 12f;  // base density per screen
    [SerializeField] private float budgetPerSecond = 10f;   // base budget
    [SerializeField] private float spawnOffset = 2f;

    private float timeElapsed;
    private float budget;

    private float spawnCooldown;

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        GameEvents.OnRunTimeChanged?.Invoke(timeElapsed);

        float densityScale = curve != null ? curve.DensityAt(timeElapsed) : 1f;
        float target = desiredPerScreen * densityScale;
        int alive = CountAlive();

        // accumulate budget normally
        budget += budgetPerSecond * Time.deltaTime * densityScale;

        // don't overspend more than one "spawn packet" per frame
        int maxSpawnsPerFrame = 3;
        int spawnsThisFrame = 0;

        // short cooldown to spread out frames
        spawnCooldown -= Time.deltaTime;

        while (spawnsThisFrame < maxSpawnsPerFrame &&
               budget >= 1f &&
               alive < target &&
               spawnCooldown <= 0f)
        {
            var e = PickEnemy();
            if (e == null || e.prefab == null) break;
            if (budget < e.cost) break;

            Spawn(e);
            budget -= e.cost;
            alive++;
            spawnsThisFrame++;

            // reset short cooldown between spawns
            spawnCooldown = Random.Range(0.05f, 0.15f);
        }
    }


    private int CountAlive() => GameObject.FindGameObjectsWithTag("Enemy").Length;

    private EnemyArchetype PickEnemy()
    {
        if (pool == null || pool.Count == 0) return null;
        float sum = 0f; foreach (var p in pool) sum += Mathf.Max(0.0001f, p.weight);
        float r = Random.value * sum, acc = 0f;
        foreach (var p in pool)
        {
            acc += Mathf.Max(0.0001f, p.weight);
            if (r <= acc) return p;
        }
        return pool[pool.Count - 1];
    }

    private void Spawn(EnemyArchetype arch)
    {
        Vector2 pos = GetRandomOffscreenPosition();
        var go = Instantiate(arch.prefab, pos, Quaternion.identity);

        if (go.TryGetComponent(out EnemyHealth h))
            h.SetScaled(arch.baseHealth * (curve?.HealthAt(timeElapsed) ?? 1f));

        if (go.TryGetComponent(out EnemyMovement m))
            m.SetSpeedMultiplier((curve?.SpeedAt(timeElapsed) ?? 1f) * arch.baseSpeed);

        if (go.TryGetComponent(out EnemyDamage d))
            d.SetDamageMultiplier((curve?.DamageAt(timeElapsed) ?? 1f) * arch.baseDamage);
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        Vector3 bl = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float camMinX = bl.x, camMaxX = tr.x, camMinY = bl.y, camMaxY = tr.y;

        int side = Random.Range(0, 4);
        return side switch
        {
            0 => new Vector2(Random.Range(camMinX, camMaxX), camMaxY + spawnOffset),
            1 => new Vector2(Random.Range(camMinX, camMaxX), camMinY - spawnOffset),
            2 => new Vector2(camMinX - spawnOffset, Random.Range(camMinY, camMaxY)),
            _ => new Vector2(camMaxX + spawnOffset, Random.Range(camMinY, camMaxY)),
        };
    }
}
