using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/EnemyArchetype")]
public class EnemyArchetype : ScriptableObject
{
    public GameObject prefab;
    [Min(1)] public int cost = 1;               // how much of the spawn budget it costs
    [Range(0f, 1f)] public float weight = 0.5f;  // relative selection weight
    public float baseHealth = 10f;
    public float baseSpeed = 1.5f;
    public float baseDamage = 5f;
}
