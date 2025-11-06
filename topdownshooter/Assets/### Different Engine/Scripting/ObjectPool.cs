using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int warmup = 32;
    private readonly Queue<GameObject> q = new();

    private void Awake()
    {
        for (int i = 0; i < warmup; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            q.Enqueue(go);
        }
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        var go = q.Count > 0 ? q.Dequeue() : Instantiate(prefab, transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        go.SetActive(false);
        q.Enqueue(go);
    }
}
