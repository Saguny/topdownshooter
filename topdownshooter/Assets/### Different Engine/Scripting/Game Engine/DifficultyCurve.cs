using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/DifficultyCurve")]
public class DifficultyCurve : ScriptableObject
{
    [SerializeField, HideInInspector] private AnimationCurve spawnDensityVisual;
    public AnimationCurve enemyHealth = AnimationCurve.Linear(0, 1, 1800, 12);
    public AnimationCurve enemySpeed = AnimationCurve.Linear(0, 1, 1800, 2);
    public AnimationCurve enemyDamage = AnimationCurve.Linear(0, 1, 1800, 4);

    [Header("Spawn Density Parameters")]
    [SerializeField] private float spawnDensityMax = 6f;
    [SerializeField] private float spawnDensityPlateauTime = 900f;
    [SerializeField] private float spawnDensityCurveSharpness = 3f;
    [SerializeField] private int curveSamplePoints = 32;

    [Header("Level -> Gears Needed")]
    public AnimationCurve gearsNeeded = new AnimationCurve(
        new Keyframe(1, 5),
        new Keyframe(10, 25),
        new Keyframe(20, 45)
    );

#if UNITY_EDITOR
    [SerializeField]
    private AnimationCurve spawnDensityPreview = new AnimationCurve();
#endif

    public float DensityAt(float t)
    {
        if (t <= 0f) return 1f;
        if (t >= spawnDensityPlateauTime) return spawnDensityMax;

        float x = t / spawnDensityPlateauTime;
        float k = spawnDensityCurveSharpness;
        float num = 1f - Mathf.Exp(-k * x);
        float den = 1f - Mathf.Exp(-k);
        float f = num / den;
        float value = 1f + (spawnDensityMax - 1f) * f;
        return Mathf.Max(0.1f, value);
    }

    public float HealthAt(float t) => Mathf.Max(0.5f, enemyHealth.Evaluate(t));
    public float SpeedAt(float t) => Mathf.Max(0.5f, enemySpeed.Evaluate(t));
    public float DamageAt(float t) => Mathf.Max(0.5f, enemyDamage.Evaluate(t));

    public int GearsForLevel(int level)
    {
        if (gearsNeeded == null || gearsNeeded.length == 0)
            return 5;

        float x = Mathf.Max(1, level);
        float y = gearsNeeded.Evaluate(x);
        return Mathf.Max(1, Mathf.RoundToInt(y));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnDensityPreview == null)
            spawnDensityPreview = new AnimationCurve();

        spawnDensityPreview.keys = new Keyframe[0];

        for (int i = 0; i <= curveSamplePoints; i++)
        {
            float t = Mathf.Lerp(0f, spawnDensityPlateauTime, i / (float)curveSamplePoints);
            float y = DensityAt(t);
            spawnDensityPreview.AddKey(t, y);
        }

        spawnDensityPreview.AddKey(spawnDensityPlateauTime * 1.2f, spawnDensityMax);
    }
#endif
}
