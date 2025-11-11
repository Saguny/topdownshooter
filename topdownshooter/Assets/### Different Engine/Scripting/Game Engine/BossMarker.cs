using UnityEngine;

public class BossMarker : MonoBehaviour
{
    private BossIndicatorManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<BossIndicatorManager>();
    }

    private void Start()
    {
        if (manager != null)
        {
            manager.RegisterBoss(transform);
        }
    }  

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.UnregisterBoss(transform);
        }
    }
}
