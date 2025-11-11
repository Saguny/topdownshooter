using System.Collections.Generic;
using UnityEngine;

public class BossIndicatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;                     // main camera
    [SerializeField] private Canvas canvas;                  // UI canvas (Screen Space - Overlay)
    [SerializeField] private RectTransform indicatorPrefab;  // BossIndicatorUI prefab

    [Header("Settings")]
    [SerializeField] private float edgePadding = 40f;        // distance from screen edge in pixels
    [SerializeField] private float arrowRadius = 35f;        // distance of arrow from skull in pixels
    [SerializeField] private float arrowBaseRotation = -45f; // your arrow's default Z rotation

    private RectTransform canvasRect;

    // each boss transform gets its own indicator
    private readonly Dictionary<Transform, RectTransform> indicators = new Dictionary<Transform, RectTransform>();

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (cam == null || canvasRect == null)
            return;

        float left = edgePadding;
        float right = Screen.width - edgePadding;
        float bottom = edgePadding;
        float top = Screen.height - edgePadding;

        // go through all registered bosses
        foreach (var kvp in indicators)
        {
            Transform boss = kvp.Key;
            RectTransform indicator = kvp.Value;

            if (boss == null || indicator == null)
                continue;

            // ---------- 1) world → screen for this boss ----------
            Vector3 bossScreen3 = cam.WorldToScreenPoint(boss.position);

            // handle "behind the camera"
            if (bossScreen3.z < 0f)
            {
                bossScreen3.x = Screen.width - bossScreen3.x;
                bossScreen3.y = Screen.height - bossScreen3.y;
                bossScreen3.z = 0.1f;
            }

            Vector2 bossScreen = new Vector2(bossScreen3.x, bossScreen3.y);

            bool onScreen =
                bossScreen3.z > 0f &&
                bossScreen3.x >= 0f && bossScreen3.x <= Screen.width &&
                bossScreen3.y >= 0f && bossScreen3.y <= Screen.height;

            // if boss is visible → hide indicator object
            if (onScreen)
            {
                if (indicator.gameObject.activeSelf)
                    indicator.gameObject.SetActive(false);
                continue;
            }

            // boss off-screen → make sure indicator is visible
            if (!indicator.gameObject.activeSelf)
                indicator.gameObject.SetActive(true);

            // ---------- 2) skull position: clamp boss to screen edges ----------
            Vector2 edgeScreenPos = bossScreen;
            edgeScreenPos.x = Mathf.Clamp(edgeScreenPos.x, left, right);
            edgeScreenPos.y = Mathf.Clamp(edgeScreenPos.y, bottom, top);

            // convert clamped screen pos → canvas local pos
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                edgeScreenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out Vector2 localPos
            );

            indicator.anchoredPosition = localPos;

            // children: 0 = Skull, 1 = Arrow
            RectTransform skull = indicator.GetChild(0).GetComponent<RectTransform>();
            RectTransform arrow = indicator.GetChild(1).GetComponent<RectTransform>();

            // skull centered & upright
            skull.anchoredPosition = Vector2.zero;
            skull.localRotation = Quaternion.identity;

            // ---------- 3) direction from skull → REAL boss position ----------
            Vector2 dir = (bossScreen - edgeScreenPos).normalized;
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.up;

            // arrow orbits around skull in that direction
            arrow.anchoredPosition = dir * arrowRadius;

            // rotate arrow (base rotation = prefab's -45°)
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float finalAngle = angleDeg - 90f + arrowBaseRotation;
            arrow.localRotation = Quaternion.Euler(0f, 0f, finalAngle);
        }
    }

    // called by BossMarker.OnEnable
    public void RegisterBoss(Transform boss)
    {
        if (boss == null || indicators.ContainsKey(boss) || indicatorPrefab == null || canvasRect == null)
            return;

        RectTransform inst = Instantiate(indicatorPrefab, canvas.transform);
        inst.gameObject.SetActive(false); // will be enabled when boss is off-screen

        indicators.Add(boss, inst);
    }

    // called by BossMarker.OnDisable
    public void UnregisterBoss(Transform boss)
    {
        if (boss == null || !indicators.TryGetValue(boss, out RectTransform indicator))
            return;

        if (indicator != null)
            Destroy(indicator.gameObject);

        indicators.Remove(boss);
    }
}
