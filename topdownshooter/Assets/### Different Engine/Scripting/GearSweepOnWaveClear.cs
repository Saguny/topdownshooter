using System.Collections;
using UnityEngine;

public class GearSweepOnWaveClear : MonoBehaviour
{
    [SerializeField] private float delayBeforePull = 0.8f;

    private void OnEnable() => GameEvents.OnWaveCleared += HandleWaveClear;
    private void OnDisable() => GameEvents.OnWaveCleared -= HandleWaveClear;

    private void HandleWaveClear(int waveIndex)
    {
        StartCoroutine(PullAfterDelay());
    }

    private IEnumerator PullAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforePull);
        PullAllGearsToPlayer();
    }

    private void PullAllGearsToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        var gears = GameObject.FindGameObjectsWithTag("Gear");
        foreach (var g in gears)
        {
            if (g == null) continue;
            StartCoroutine(MoveGearToPlayer(g.transform, player));
        }
    }

    private IEnumerator MoveGearToPlayer(Transform gear, Transform player)
    {
        float duration = 0.6f, t = 0f;
        Vector3 start = gear.position;

        while (t < duration && gear != null && player != null)
        {
            t += Time.deltaTime;
            float p = t / duration;
            gear.position = Vector3.Lerp(start, player.position, p * p);
            yield return null;
        }

        if (gear != null && player != null)
        {
            var inv = player.GetComponent<PlayerInventory>();
            if (inv != null) inv.AddGears(1);
            Destroy(gear.gameObject);
        }
    }
}
