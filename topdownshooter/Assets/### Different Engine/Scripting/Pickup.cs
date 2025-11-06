using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private int gears = 1;
    private bool pulling;
    private Transform target;

    public void PullTo(Transform t)
    {
        pulling = true;
        target = t;
    }

    private void Update()
    {
        if (!pulling || target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 8f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent(out PlayerInventory inv))
            inv.AddGears(gears);

        Destroy(gameObject);
    }
}
