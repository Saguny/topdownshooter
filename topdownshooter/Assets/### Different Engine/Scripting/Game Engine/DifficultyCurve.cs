using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/DifficultyCurve")]
public class DifficultyCurve : ScriptableObject
{
    [Header("Time (seconds) -> multiplier")]
    [SerializeField, HideInInspector] private AnimationCurve spawnDensityVisual;
    public AnimationCurve enemyHealth = AnimationCurve.Linear(0, 1, 1800, 12);
    public AnimationCurve enemySpeed = AnimationCurve.Linear(0, 1, 1800, 2);
    public AnimationCurve enemyDamage = AnimationCurve.Linear(0, 1, 1800, 4);

    [Header("Spawn Density Parameters")]
    [SerializeField] private float spawnDensityMax = 6f;
    [SerializeField] private float spawnDensityPlateauTime = 900f; // 15 min
    [SerializeField] private float spawnDensityCurveSharpness = 3f;
    [SerializeField] private int curveSamplePoints = 32;

#if UNITY_EDITOR
    [SerializeField, Space(10)]
    [Tooltip("Preview of spawn density curve (auto-generated)")]
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
