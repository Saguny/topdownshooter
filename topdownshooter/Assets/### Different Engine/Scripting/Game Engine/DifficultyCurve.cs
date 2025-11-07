using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/DifficultyCurve")]
public class DifficultyCurve : ScriptableObject
{
    [Header("Time (seconds) -> multiplier")]
    public AnimationCurve spawnDensity = AnimationCurve.Linear(0, 1, 600, 6);
    public AnimationCurve enemyHealth = AnimationCurve.Linear(0, 1, 600, 12);
    public AnimationCurve enemySpeed = AnimationCurve.Linear(0, 1, 600, 2);
    public AnimationCurve enemyDamage = AnimationCurve.Linear(0, 1, 600, 4);

    public float DensityAt(float t) => Mathf.Max(0.1f, spawnDensity.Evaluate(t));
    public float HealthAt(float t) => Mathf.Max(0.5f, enemyHealth.Evaluate(t));
    public float SpeedAt(float t) => Mathf.Max(0.5f, enemySpeed.Evaluate(t));
    public float DamageAt(float t) => Mathf.Max(0.5f, enemyDamage.Evaluate(t));
}