using UnityEngine;

[DefaultExecutionOrder(10)]
public class HealthBarAutoAttach : MonoBehaviour
{
    [Header("bar settings")]
    public Sprite barSprite;                         // optional; HealthBarSprite will generate if null
    public string sortingLayer = "Default";
    public int bgOrder = 199;
    public int fgOrder = 200;

    public Vector2 size = new Vector2(2.0f, 0.18f);
    public Vector3 offset = new Vector3(0f, 0.8f, 0f);
    public float bgPadding = 0.06f;
    public float pixelSnap = 0f;

    private void Start()
    {
        var health = GetComponent<IHealth>();
        if (health == null)
        {
            Debug.LogWarning($"[HealthBarAutoAttach] {name} has no IHealth. No bar created.");
            return;
        }

        // remove stale children
        var existing = transform.Find("HealthBar");
        if (existing != null) Destroy(existing.gameObject);

        // create new
        var barRoot = new GameObject("HealthBar");
        barRoot.layer = gameObject.layer;
        barRoot.transform.SetParent(transform, false);
        barRoot.transform.localPosition = Vector3.zero;
        barRoot.SetActive(true);

        var hb = barRoot.AddComponent<HealthBarSprite>();
        hb.enabled = true;

        // visuals
        hb.barSprite = barSprite;        // can be null, HealthBarSprite will auto-gen
        hb.sortingLayer = sortingLayer;
        hb.bgOrder = bgOrder;
        hb.fgOrder = fgOrder;
        hb.size = size;
        hb.offset = offset;
        hb.bgPadding = bgPadding;
        hb.pixelSnap = pixelSnap;

        // bind
        hb.Bind(transform, health);
    }
}
