using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearSweepOnWaveClear : MonoBehaviour
{
    [SerializeField] private float delay = 0.8f;
    [SerializeField] private float duration = 0.5f;

    private void OnEnable()
    {
        GameEvents.OnWaveCleared += HandleWaveCleared;
    }

    private void OnDisable()
    {
        GameEvents.OnWaveCleared -= HandleWaveCleared;
    }

    private void HandleWaveCleared(int _)
    {
        StartCoroutine(PullAfterDelay());
    }

    private IEnumerator PullAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) yield break;

        var gears = new List<GameObject>(GameObject.FindGameObjectsWithTag("Gear"));
        var start = new Dictionary<GameObject, Vector3>();
        foreach (var g in gears) if (g) start[g] = g.transform.position;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            foreach (var g in gears)
            {
                if (!g) continue;
                var rb = g.GetComponent<Rigidbody2D>();
                if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
                g.transform.position = Vector3.Lerp(start[g], player.transform.position, k);
            }
            yield return null;
        }

        var inv = player.GetComponent<PlayerInventory>();
        foreach (var g in gears)
        {
            if (!g) continue;
            if (inv) inv.AddGears(1);
            Destroy(g);
        }
    }
}
