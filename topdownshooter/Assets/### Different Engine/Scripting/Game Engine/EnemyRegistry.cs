using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistry
{
    private static readonly HashSet<GameObject> enemies = new HashSet<GameObject>();

    public static int Count => enemies.Count;

    public static IEnumerable<GameObject> All => enemies;

    public static void Register(GameObject go)
    {
        if (go != null) enemies.Add(go);
    }

    public static void Unregister(GameObject go)
    {
        if (go != null) enemies.Remove(go);
    }
}
