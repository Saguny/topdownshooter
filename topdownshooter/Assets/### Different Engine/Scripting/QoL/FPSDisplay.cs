using UnityEngine;
using TMPro;
using System.Text;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    float deltaTime;
    float minFps = float.MaxValue;
    float maxFps;
    float avgAccum;
    int avgCount;

    float avgWindow = 2f;
    float windowTimer;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        float frameMs = deltaTime * 1000f;

        windowTimer += Time.unscaledDeltaTime;
        avgAccum += fps;
        avgCount++;

        if (fps < minFps) minFps = fps;
        if (fps > maxFps) maxFps = fps;

        if (windowTimer >= avgWindow)
        {
            minFps = float.MaxValue;
            maxFps = 0f;
            avgAccum = 0f;
            avgCount = 0;
            windowTimer = 0f;
        }

        int enemyCount = 0;
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies != null) enemyCount = enemies.Length;

        int projectileCount = 0;
        var projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        if (projectiles != null) projectileCount = projectiles.Length;

        Vector3 playerPos = Vector3.zero;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerPos = player.transform.position;

        float runTime = Time.timeSinceLevelLoad;
        int runMinutes = Mathf.FloorToInt(runTime / 60f);
        int runSeconds = Mathf.FloorToInt(runTime % 60f);

        long memBytes = System.GC.GetTotalMemory(false);
        float memMb = memBytes / (1024f * 1024f);

        float avgFps = avgCount > 0 ? avgAccum / avgCount : fps;

        string colorStart;
        if (fps >= 60f) colorStart = "<color=#00FF00>";
        else if (fps >= 30f) colorStart = "<color=#FFFF00>";
        else colorStart = "<color=#FF0000>";

        var sb = new StringBuilder(128);
        sb.AppendFormat("{0}{1:0.} FPS ({2:0.0} ms)</color>\n", colorStart, fps, frameMs);
        sb.AppendFormat("avg {0:0.} | min {1:0.} | max {2:0.}\n", avgFps, minFps == float.MaxValue ? 0f : minFps, maxFps);
        sb.AppendFormat("time {0:00}:{1:00}  x{2:0.00}\n", runMinutes, runSeconds, Time.timeScale);
        sb.AppendFormat("enemies {0}  proj {1}\n", enemyCount, projectileCount);
        sb.AppendFormat("player ({0:0.0}, {1:0.0})\n", playerPos.x, playerPos.y);
        sb.AppendFormat("mem {0:0.0} MB", memMb);

        fpsText.text = sb.ToString();
    }
}
